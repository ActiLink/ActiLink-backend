using ActiLink.Hobbies.DTOs;
using ActiLink.Organizers.DTOs;
using ActiLink.Organizers.Users.DTOs;
using ActiLink.Venues.DTOs;
using ActiLink.Shared.Model;

namespace ActiLink.Events.DTOs
{
	/// <summary>
	/// Data transfer object for an event.
	/// </summary>
	/// <param name="Id">Unique identifier of the event.</param>
	/// <param name="Title"> Title of the event (max 100 characters).</param>
	/// <param name="Description"> Description of the event.</param>
	/// <param name="StartTime">Start time of the event.</param>
	/// <param name="EndTime">End time of the event.</param>
	/// <param name="Location"> Location of the event</param>
	/// <param name="Price">Price of the event.</param>
	/// <param name="MaxUsers">Maximum number of participants.</param>
	/// <param name="MinUsers">Minimum number of participants.</param>
	/// <param name="Participants">List of Participants.</param>
	/// <param name="Organizer">Organizer of the event.</param>
	/// <param name="Hobbies">List of Hobbies related to this event.</param>
	/// <param name="Venue">Venue of the event (nullable).</param>
	public record EventDto(
        Guid Id,
        string Title,
        string Description,
        DateTime StartTime,
        DateTime EndTime,
        Location Location,
        decimal Price,
        int MinUsers,
        int MaxUsers,
        List<HobbyDto> Hobbies,
        OrganizerDto Organizer,
        List<UserDto> Participants,
		ReducedVenueDto? Venue = null
	)
    {
        private EventDto()
            : this(
                default,
                string.Empty,
                string.Empty,
                default,
                default,
                new Location(0, 0),
                default,
                default,
                default,
                [],
                default!,
                [],
                null)
        { }
    }

}