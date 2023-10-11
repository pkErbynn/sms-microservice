namespace SmsClient.Dtos
{
    public class MessagePublishedDto
    {
        public Guid Id { get; set; }
        public string PhoneNumber { get; set; }
        public string SmsText { get; set; }
        public string Command { get; set; }
    }
}
