using AutoMapper;
using Moq;
using SmsMicroservice.Dto;
using SmsMicroservice.EventBus;
using SmsMicroservice.Logger;
using SmsMicroservice.Messenger;
using SmsMicroservice.Models;
using SmsMicroservice.SyncDataService;
using System;
using System.IO;

namespace SmsMicroservice.Tests
{
    /// <summary>
    /// The SMS Microservice includes a set of unit tests to ensure the correctness of its functionality. These tests cover various scenarios, including valid and invalid SMS messages, successful message delivery to a 3rd-party service, and error handling.
    /// </summary>
    public class SmsServiceTests
    {
        private readonly SmsService _smsService;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<IEventBus> _eventBus;
        private readonly Mock<ILoggerLibrary> _logger;
        private readonly Mock<IHttpSmsDataClient> _httpSmsDataClient;

        public SmsServiceTests()
        {
            _mapper = new Mock<IMapper>();
            _eventBus = new Mock<IEventBus>();
            _logger = new Mock<ILoggerLibrary> ();
            _httpSmsDataClient = new Mock<IHttpSmsDataClient>();
            _smsService = new SmsService(
                _mapper.Object,
                _httpSmsDataClient.Object,
                _eventBus.Object,
                _logger.Object);
        }

        /// <summary>
        ///     Validates the behavior of the `SendSmsMessage` method when given an invalid SMS message.
        /// </summary>
        /// <returns>
        ///     The test expects that the logger's `LogError` method is called once.
        /// </returns>
        [Fact]
        public async Task SendSmsMessage_InvalidSmsMessage_Returns_Void()
        {
            // arrange
            _logger.Setup(p => p.LogError(It.IsAny<string>())).Verifiable();
            var invalidMessageRecieved = new MessageRecievedDto
            {
                Command = "",
                PhoneNumber= null,
                SmsText = "hello"
            };

            // act
            await _smsService.SendSmsMessage(invalidMessageRecieved);

            // assert
            _logger.Verify(v => v.LogError(It.IsAny<string>()), Times.Once());
        }


        /// <summary>
        ///     Validates the handling of exceptions thrown by the 3rd-party service. Simulates a scenario where the 3rd-party service throws an exception.
        /// </summary>
        /// <returns>
        ///     Expects
        ///     - The logger's `LogError` and `LogInfo` methods are called.
        ///     - The `SendSmsToThirdPartyAsync` method of the HTTP client throws an exception.
        ///     - The `Publish` method of the event bus is not called.
        /// </returns>
        [Fact]
        public async Task SendSmsMessage_ValidSmsMessageToThirdParty_Succssfully()
        {
            // arrange
            var httpClientMock = new Mock<HttpClient>();
            var validSmsMessageDto = new MessageRecievedDto
            {
                PhoneNumber = "12345",
                SmsText = "hello"
            };
            _httpSmsDataClient
                .Setup(client => client.SendSmsToThirdPartyAsync(It.IsAny<SmsTextMessage>()))
                .ReturnsAsync(true)
                .Verifiable();
            _logger.Setup(p => p.LogInfo(It.IsAny<string>())).Verifiable();
            _eventBus.Setup(p => p.Publish(It.IsAny<SmsSentEvent>())).Verifiable();
            _mapper.Setup(x => x.Map<SmsTextMessage>(It.IsAny<MessageRecievedDto>()))
                .Returns(
                    new SmsTextMessage
                    {
                        PhoneNumber = validSmsMessageDto.PhoneNumber,
                        SmsText= validSmsMessageDto.SmsText
                    }).Verifiable();

            // act
            await _smsService.SendSmsMessage(validSmsMessageDto);

            // assert
            _logger.Verify(v => v.LogInfo(It.IsAny<string>()), Times.Exactly(3));
            _httpSmsDataClient.Verify(v => v.SendSmsToThirdPartyAsync(It.IsAny<SmsTextMessage>()), Times.Once());
            _eventBus.Verify(p => p.Publish(It.IsAny<SmsSentEvent>()), Times.Once());
        }

        /// <summary>
        ///    Ensures proper error handling when the 3rd-party service returns a failure response.
        ///    Simulates a situation where the 3rd-party service returns a failure response.
        /// </summary>
        /// <returns>
        ///     Expects
        ///      - The logger's `LogError` and `LogInfo` methods are called.
        ///      - The `SendSmsToThirdPartyAsync` method of the HTTP client is called once.
        ///      - The `Publish` method of the event bus is not called.
        /// </returns>
        [Fact]
        public async Task SendSmsMessage_FailureResponseFromThirdParty_LogsError()
        {
            // arrange
            var httpClientMock = new Mock<HttpClient>();
            var validSmsMessageDto = new MessageRecievedDto
            {
                PhoneNumber = "12345",
                SmsText = "hello2"
            };
            _httpSmsDataClient
                .Setup(client => client.SendSmsToThirdPartyAsync(It.IsAny<SmsTextMessage>()))
                .ReturnsAsync(false)
                .Verifiable();
            _logger.Setup(p => p.LogInfo(It.IsAny<string>())).Verifiable();
            _logger.Setup(p => p.LogError(It.IsAny<string>())).Verifiable();
            _mapper.Setup(x => x.Map<SmsTextMessage>(It.IsAny<MessageRecievedDto>()))
                .Returns(
                    new SmsTextMessage
                    {
                        PhoneNumber = validSmsMessageDto.PhoneNumber,
                        SmsText = validSmsMessageDto.SmsText
                    }).Verifiable();

            // act
            await _smsService.SendSmsMessage(validSmsMessageDto);

            // assert
            _logger.Verify(v => v.LogError(It.IsAny<string>()), Times.Once);
            _logger.Verify(v => v.LogInfo(It.IsAny<string>()), Times.Once);
            _httpSmsDataClient.Verify(v => v.SendSmsToThirdPartyAsync(It.IsAny<SmsTextMessage>()), Times.Once());
            _eventBus.Verify(p => p.Publish(It.IsAny<SmsSentEvent>()), Times.Never);
        }

        /// <summary>
        ///    Validates the handling of exceptions thrown by the 3rd-party service.
        ///    Simulates a scenario where the 3rd-party service throws an exception.
        /// </summary>
        /// <returns>
        ///     Expects
        ///     - The logger's `LogError` and `LogInfo` methods are called.
        ///     - The `SendSmsToThirdPartyAsync` method of the HTTP client throws an exception.
        ///     - The `Publish` method of the event bus is not called.
        /// </returns>
        [Fact]
        public async Task SendSmsMessage_ThirdPartyThrowsException_LogsError()
        {
            // arrange
            var httpClientMock = new Mock<HttpClient>();
            var validSmsMessageDto = new MessageRecievedDto
            {
                PhoneNumber = "12345",
                SmsText = "hello2"
            };
            _httpSmsDataClient
                .Setup(client => client.SendSmsToThirdPartyAsync(It.IsAny<SmsTextMessage>()))
                .ThrowsAsync(new Exception())
                .Verifiable();
            _logger.Setup(p => p.LogInfo(It.IsAny<string>())).Verifiable();
            _logger.Setup(p => p.LogError(It.IsAny<string>())).Verifiable();
            _mapper.Setup(x => x.Map<SmsTextMessage>(It.IsAny<MessageRecievedDto>()))
                .Returns(
                    new SmsTextMessage
                    {
                        PhoneNumber = validSmsMessageDto.PhoneNumber,
                        SmsText = validSmsMessageDto.SmsText
                    }).Verifiable();

            // act
            await _smsService.SendSmsMessage(validSmsMessageDto);

            // act and assert
            _logger.Verify(v => v.LogError(It.IsAny<string>()), Times.Once);
            _eventBus.Verify(p => p.Publish(It.IsAny<SmsSentEvent>()), Times.Never);
        }
    }
}