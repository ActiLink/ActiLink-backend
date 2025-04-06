using ActiLink.DTOs;
using ActiLink.Model;
using AutoMapper;

namespace ActiLink.MapperProfiles
{
    public class EventProfile : Profile
    {
        public EventProfile()
        {
            // Map Hobby to HobbyDto
            CreateMap<Hobby, HobbyDto>();

            // Map Event to EventDto
            CreateMap<Event, EventDto>()
                .ForMember(dest => dest.OrganizerId, opt => opt.MapFrom(src => src.Organizer.Id))
                .ForMember(dest => dest.Participants, opt => opt.MapFrom(src => src.SignUpList))
                .ForMember(dest => dest.Hobbies, opt => opt.MapFrom(src => src.RelatedHobbies));

            // Map NewEventDto to Event
            CreateMap<NewEventDto, Event>()
                .ForMember(dest => dest.SignUpList, opt => opt.MapFrom(_ => new List<User>()))
                .ForMember(dest => dest.RelatedHobbies, opt => opt.MapFrom(_ => new List<Hobby>()))
                .AfterMap((src, dest, context) =>
                {
                    var organizer = context.Items["Organizer"] as Organizer
                        ?? throw new InvalidOperationException("Organizer must be provided in context items");

                    // Using reflection to set the private Organizer property
                    typeof(Event)
                        .GetProperty(nameof(Event.Organizer))?
                        .SetValue(dest, organizer);
                });
        }
    }
}