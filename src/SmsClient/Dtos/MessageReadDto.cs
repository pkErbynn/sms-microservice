using System.ComponentModel.DataAnnotations;

namespace SmsClient.Dtos
{
    public class MessageReadDto
    {
        public Guid Id { get; set; }
        public string PhoneNumber { get; set; }
        public string SmsText { get; set; }
    }
}
