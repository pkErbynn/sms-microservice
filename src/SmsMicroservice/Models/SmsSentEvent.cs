namespace SmsMicroservice.Models
{
    public class SmsSentEvent
    {
        public string PhoneNumber { get; set; }
        public string SmsText { get; set; }
        public string Event { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
