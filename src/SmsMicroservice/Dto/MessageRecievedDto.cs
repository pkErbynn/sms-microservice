namespace SmsMicroservice.Dto
{
    public class MessageRecievedDto
    {
        public string Id { get; set; }
        public string PhoneNumber { get; set; }
        public string SmsText { get; set; }
        public string Command { get; set; }
    }
}
