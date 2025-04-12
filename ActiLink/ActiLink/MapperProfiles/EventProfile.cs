using ActiLink.DTOs;
using ActiLink.Model;
using ActiLink.Services;
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

            // Map NewEventDto to CreateEventObject (used in services)
            CreateMap<NewEventDto, CreateEventObject>()
                .ConstructUsing((src,context) =>
                {
                    var organizerId = context.Items["OrganizerId"] as string
                        ?? throw new InvalidOperationException("OrganizerId must be provided in context items");
                    
                    return new CreateEventObject(
                        organizerId,
                        src.StartTime,
                        src.EndTime,
                        src.Location,
                        src.Price,
                        src.MinUsers,
                        src.MaxUsers,
                        src.RelatedHobbyIds);
                });

            // Map CreateEventObject to Event
            CreateMap<CreateEventObject, Event>()
                .ForMember(dest => dest.SignUpList, opt => opt.MapFrom(_ => new List<User>()))
                .ForMember(dest => dest.RelatedHobbies, opt => opt.MapFrom(_ => new List<Hobby>()))
                .AfterMap((src, dest, context) =>
                {
                    // Organizer
                    if (context.Items.TryGetValue("Organizer", out var organizerObj) &&
                        organizerObj is Organizer organizer)
                    {
                        typeof(Event)
                            .GetProperty(nameof(Event.Organizer))?
                            .SetValue(dest, organizer);
                    }

                    // Hobbies
                    if (context.Items.TryGetValue("Hobbies", out var hobbiesObj) &&
                        hobbiesObj is IEnumerable<Hobby> hobbies)
                    {
                        dest.RelatedHobbies.Clear();
                        foreach (var hobby in hobbies)
                            dest.RelatedHobbies.Add(hobby);
                    }
                });

            // Map UpdateEventDto to UpdateEventObject 
            CreateMap<UpdateEventDto,UpdateEventObject>()
                .ConstructUsing((src, context) =>
                {
                    var eventId = context.Items["EventId"] as Guid?
                        ?? throw new InvalidOperationException("EventId must be provided in context items");

                    return new UpdateEventObject(
                        eventId,
                        src.OrganizerId,
                        src.StartTime,
                        src.EndTime,
                        src.Location,
                        src.Price,
                        src.MinUsers,
                        src.MaxUsers,
                        src.RelatedHobbyIds);
                });

            // Map UpdateEventObject to Event
            CreateMap<UpdateEventObject, Event>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()) // Id is not changed
                .ForMember(dest => dest.Organizer, opt => opt.Ignore()) 
                .ForMember(dest => dest.RelatedHobbies, opt => opt.Ignore())
                 .AfterMap((src, dest, context) =>
                 {
                     // Organizer
                     if (context.Items.TryGetValue("Organizer", out var organizerObj) &&
                         organizerObj is Organizer organizer)
                     {
                         typeof(Event)
                             .GetProperty(nameof(Event.Organizer))?
                             .SetValue(dest, organizer);
                     }

                     // Hobbies
                     if (context.Items.TryGetValue("Hobbies", out var hobbiesObj) &&
                         hobbiesObj is IEnumerable<Hobby> hobbies)
                     {
                         dest.RelatedHobbies.Clear();
                         foreach (var hobby in hobbies)
                             dest.RelatedHobbies.Add(hobby);
                     }
                 });

        }
    }
}