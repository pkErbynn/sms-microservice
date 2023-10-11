using AutoMapper;
using SmsMicroservice.Dto;
using SmsMicroservice.Models;

namespace SmsClient.Profiles
{
    public class MessageProfile : Profile
    {
        public MessageProfile()
        {
            CreateMap<MessageRecievedDto, SmsTextMessage>();
        }
    }
}
