using System.ComponentModel.DataAnnotations;

namespace SmsClient.Models
{
    public class Message
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public string SmsText { get; set; }
    }
}
