using AutoMapper;
using SmsMicroservice.Dto;
using SmsMicroservice.EventBus;
using SmsMicroservice.Logger;
using SmsMicroservice.Models;
using SmsMicroservice.SyncDataService;
using System.Text.Json;

namespace SmsMicroservice.Messenger
{
    /// <summary>
    /// The SmsService class, the major component, that sends SMS messages to customers and publishes "SmsSent" events to an event bus within the SMS Microservice.
    /// Validates incoming SMS messages.
    /// Manages duplicate message prevention.
    /// Handles SMS message sending and event publishing.
    /// Logs errors and warnings for invalid or failed messages.
    /// </summary>
    public class SmsService : ISmsService
    {
        private readonly IMapper _mapper;
        private readonly IHttpSmsDataClient _httpSmsDataClient;
        private readonly IEventBus _eventBus;
        private readonly ILoggerLibrary _logger;

        private readonly static Dictionary<string, DateTime> _sentSmsMessagesCache = new Dictionary<string, DateTime>();
        private const int WAITING_PERIOD_IN_MINUTE = 2;

        /// <summary>
        /// Dependencies needed to initialize the constructor
        /// </summary>
        /// <param name="mapper"> For mapping data transfer objects.</param>
        /// <param name="httpSmsDataClient"> For sending SMS messages via HTTP.</param>
        /// <param name="eventBus"> For event publishing.</param>
        /// <param name="logger"> For logging.</param>
        public SmsService(IMapper mapper, IHttpSmsDataClient httpSmsDataClient, IEventBus eventBus, ILoggerLibrary logger)
        {
            _mapper = mapper;
            _httpSmsDataClient = httpSmsDataClient;
            _eventBus = eventBus;
            _logger = logger;
        }

        public async Task SendSmsMessage(MessageRecievedDto message)
        {
            bool isSmsValid = IsValidSendSmsCommand(message);

            if (!isSmsValid)
            {
                _logger.LogError($"--> Invalid Sms message {JsonSerializer.Serialize(message)}");
                return;
            }
            _logger.LogInfo($"--> Sending sms {JsonSerializer.Serialize(message)}....!");
            var messageToSend = _mapper.Map<SmsTextMessage>(message);

            var cacheKey = $"{messageToSend.PhoneNumber}-{messageToSend.SmsText}";
            if (IsImmediateSmsMessageDuplicate(cacheKey)) return;

            try
            {
                var isSmsSentSuccess = await _httpSmsDataClient.SendSmsToThirdPartyAsync(messageToSend);
                if (isSmsSentSuccess)
                {
                    _logger.LogInfo("Publishing SmsSent event to global bus.....");

                    var smsSentEvent = new SmsSentEvent
                    {
                        PhoneNumber = messageToSend.PhoneNumber,
                        SmsText = messageToSend.SmsText,
                        Event = "SmsSent",
                        Timestamp = DateTime.UtcNow
                    };
                    _eventBus.Publish(smsSentEvent);
                    _sentSmsMessagesCache[cacheKey] = DateTime.UtcNow;

                    _logger.LogInfo("Event published toglobal event bus");
                }
                else
                {
                    _logger.LogError("Maximum retry attempts reached. SMS delivery failed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing SMS message: {ex.Message}");
            }
        }

        private bool IsValidSendSmsCommand(MessageRecievedDto receivedMessage)
        {
            if (receivedMessage != null &&
                !string.IsNullOrEmpty(receivedMessage.PhoneNumber) &&
                !string.IsNullOrEmpty(receivedMessage.SmsText))
            {
                return true;
            }

            return false;
        }

        private bool IsImmediateSmsMessageDuplicate(string cacheKey)
        {
            if (_sentSmsMessagesCache.ContainsKey(cacheKey) &&
               (DateTime.UtcNow - _sentSmsMessagesCache[cacheKey]).TotalMinutes <= WAITING_PERIOD_IN_MINUTE)
            {
                _logger.LogWarning($"Message already sent recently. Wait and resend in {WAITING_PERIOD_IN_MINUTE} minutes.");
                return true;
            }
            return false;
        }

    }
}
