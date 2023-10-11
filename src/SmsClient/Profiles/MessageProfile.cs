using AutoMapper;
using SmsClient.Dtos;

namespace SmsClient.Profiles
{
    public class MessageProfile : Profile
    {
        public MessageProfile()
        {
            CreateMap<MessageSendDto, MessagePublishedDto>();
        }
    }
}
