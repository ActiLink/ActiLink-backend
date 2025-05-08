using ActiLink.Organizers.DTOs;
using AutoMapper;

namespace ActiLink.Organizers.Infrastructure
{
    public class OrganizerProfile : Profile
    {
        public OrganizerProfile()
        {
            CreateMap<Organizer, OrganizerDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.UserName));
        }
    }
}
