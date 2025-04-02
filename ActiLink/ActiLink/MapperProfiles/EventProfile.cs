using ActiLink.DTOs;
using ActiLink.Model;
using AutoMapper;

namespace ActiLink.MapperProfiles
{
    public class EventProfile : Profile
    {
        public EventProfile()
        {
            // Mapowanie Event -> EventDto
            CreateMap<Event, EventDto>()
                .ForMember(dest => dest.Height, opt => opt.MapFrom(src => src.Location.Height))
                .ForMember(dest => dest.Width, opt => opt.MapFrom(src => src.Location.Width));

            // Mapowanie NewEventDto -> Event (do tworzenia nowych wydarzeń)
            CreateMap<NewEventDto, Event>()
                .ConstructUsing(src => new Event(
                    src.OrganizerId.ToString(),
                    src.StartTime,
                    src.EndTime,
                    new Location(src.Height, src.Width), // Tworzenie lokalizacji
                    src.Price,
                    src.MaxUsers,
                    src.MinUsers
                ));
        }
    }
}
