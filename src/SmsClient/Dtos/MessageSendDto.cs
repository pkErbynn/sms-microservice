using System.ComponentModel.DataAnnotations;

namespace SmsClient.Dtos
{
    public class MessageSendDto
    {
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public string SmsText { get; set; }
    }
}
