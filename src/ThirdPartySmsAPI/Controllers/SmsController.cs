using Microsoft.AspNetCore.Mvc;
using ThirdPartySmsAPI.Dto;

namespace ThirdPartySmsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SmsController : ControllerBase
    {
        [HttpPost]
        public ActionResult CreateCommandForPlatform(SmsMessageDto smsMessageDto)
        {
            if (smsMessageDto == null)
            {
                return NotFound();
            }
            Console.WriteLine("--> sms sent from 3rd party");
            return Ok(smsMessageDto);
        }
    }
}