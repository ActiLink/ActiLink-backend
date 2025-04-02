using ActiLink.DTOs;
using ActiLink.Model;
using AutoMapper;
using System.Linq;

namespace ActiLink.MapperProfiles
{
    public class EventProfile : Profile
    {
        public EventProfile()
        {
            CreateMap<Event, EventDto>()
                .ForMember(dest => dest.Height, opt => opt.MapFrom(src => src.Location.Height))
                .ForMember(dest => dest.Width, opt => opt.MapFrom(src => src.Location.Width))
                .ForMember(dest => dest.Participants, opt => opt.MapFrom(src =>
                    src.SignUpList == null
                        ? new List<UserDto>()
                        : src.SignUpList.Select(u => new UserDto(
                            u.Id,
                            u.UserName ?? string.Empty,  // Zapewnia wartość domyślną jeśli null
                            u.Email ?? string.Empty
                        ))));

            // Mapowanie NewEventDto -> Event
            CreateMap<NewEventDto, Event>()
                .ConstructUsing(src => new Event(
                    src.OrganizerId.ToString(),
                    src.StartTime,
                    src.EndTime,
                    new Location(src.Height, src.Width),
                    src.Price,
                    src.MaxUsers,
                    src.MinUsers
                ));
        }
    }
}