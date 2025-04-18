using ActiLink.Organizers.BusinessClients.DTOs;
using AutoMapper;

namespace ActiLink.Organizers.BusinessClients.Infrastructure
{
    public class BusinessClientProfile : Profile
    {
        public BusinessClientProfile()
        {
            CreateMap<BusinessClient, BusinessClientDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.UserName));
        }
    }
}
