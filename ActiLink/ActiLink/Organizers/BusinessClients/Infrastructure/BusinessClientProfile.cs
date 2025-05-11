using ActiLink.Organizers.BusinessClients.DTOs;
using ActiLink.Organizers.BusinessClients.Service;
using AutoMapper;

namespace ActiLink.Organizers.BusinessClients.Infrastructure
{
    public class BusinessClientProfile : Profile
    {
        public BusinessClientProfile()
        {
            CreateMap<NewBusinessClientDto, BusinessClient>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Name));
            CreateMap<BusinessClient, BusinessClientDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.UserName));
            CreateMap<UpdateBusinessClientDto, UpdateBusinessClientObject>();

            CreateMap<BusinessClient, VenueOwnerDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.UserName));

        }
    }
}
