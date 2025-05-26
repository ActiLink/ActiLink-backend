using ActiLink.Events.DTOs;
using ActiLink.Events.Service;
using ActiLink.Hobbies;
using ActiLink.Organizers;
using ActiLink.Organizers.Users;
using ActiLink.Venues;
using AutoMapper;

namespace ActiLink.Events.Infrastructure
{
    public class EventProfile : Profile
    {
        public EventProfile()
        {
            // Map Event to EventDto
            CreateMap<Event, EventDto>()
                //.ForMember(dest => dest.Organizer, opt => opt.MapFrom(src => src.Organizer.Id))
                .ForMember(dest => dest.Participants, opt => opt.MapFrom(src => src.SignUpList))
                .ForMember(dest => dest.Hobbies, opt => opt.MapFrom(src => src.RelatedHobbies));

            // Map NewEventDto to CreateEventObject (used in services)
            CreateMap<NewEventDto, CreateEventObject>()
                .ConstructUsing((src, context) =>
                {
                    var organizerId = context.Items["OrganizerId"] as string
                        ?? throw new InvalidOperationException("OrganizerId must be provided in context items");


					return new CreateEventObject(
                        organizerId,
                        src.Title,
                        src.Description,
                        src.StartTime,
                        src.EndTime,
                        src.Location,
                        src.Price,
                        src.MinUsers,
                        src.MaxUsers,
                        src.RelatedHobbies.Select(h => h.Name),
                        src.VenueId);
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
					// Venue
					if (context.Items.TryGetValue("Venue", out var venueObj) &&
						venueObj is Venue venue)
					{
						typeof(Event)
							.GetProperty(nameof(Event.Venue))?
							.SetValue(dest, venue);
					}

				});

            // Map UpdateEventDto to UpdateEventObject 
            CreateMap<UpdateEventDto, UpdateEventObject>()
                .ForMember(dest => dest.RelatedHobbyNames,
                opt => opt.MapFrom(src => src.RelatedHobbies.Select(h => h.Name).ToList()));

			// Map UpdateEventObject to Event
			CreateMap<UpdateEventObject, Event>()
                .ForMember(dest => dest.Organizer, opt => opt.Ignore())
                .ForMember(dest => dest.RelatedHobbies, opt => opt.Ignore())
                 .AfterMap((src, dest, context) =>
                 {
                     // Hobbies
                     if (context.Items.TryGetValue("Hobbies", out var hobbiesObj) &&
                         hobbiesObj is IEnumerable<Hobby> hobbies)
                     {
                         dest.RelatedHobbies.Clear();
                         foreach (var hobby in hobbies)
                             dest.RelatedHobbies.Add(hobby);
                     }
                     // Venue
                     if (context.Items.TryGetValue("Venue", out var venueObj) &&
                         venueObj is Venue venue)
                     {
                         typeof(Event)
                             .GetProperty(nameof(Event.Venue))?
                             .SetValue(dest, venue);
                     }
                     else
                     {
                         typeof(Event)
                             .GetProperty(nameof(Event.Venue))?
                             .SetValue(dest, null);
                     }
				 });

            CreateMap<Event, ReducedEventDto>();
        }
	}
}