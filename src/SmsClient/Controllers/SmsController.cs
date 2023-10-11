using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SmsClient.AsyncDataServices;
using SmsClient.Dtos;
using SmsClient.Enums;

namespace SmsClient.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SmsController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IMessageBusClient _messageBusClient;

        public SmsController(IMessageBusClient messageBusClient, IMapper mapper)
        {
            _mapper = mapper;
            _messageBusClient = messageBusClient;
        }

        [HttpPost]
        public async Task<ActionResult<MessagePublishedDto>> SendSmsMessage(MessageSendDto messageSendDto)
        {
            if (messageSendDto == null)
            {
                return BadRequest();
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var messagePublishedDto = _mapper.Map<MessagePublishedDto>(messageSendDto);
            messagePublishedDto.Id = Guid.NewGuid();

            try
            {
                _messageBusClient.PublishNewSmsMessage(messagePublishedDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not send asynchronously: {ex.Message}");
            }

            return Ok(messagePublishedDto);
        }
    }
}
