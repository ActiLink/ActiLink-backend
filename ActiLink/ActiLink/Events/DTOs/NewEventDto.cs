using ActiLink.Hobbies.DTOs;
using ActiLink.Shared.Model;

namespace ActiLink.Events.DTOs
{
    /// <summary>
    /// Data transfer object for creating a new event.
    /// </summary>
    /// <param name="Title"> Title of the event (max 100 characters).</param>
    /// <param name="Description"> Description of the event.</param>
    /// <param name="StartTime">Start time of the event.</param>
    /// <param name="EndTime">End time of the event.</param>
    /// <param name="Location"> Location of the event</param>
    /// <param name="Price">Price of the event.</param>
    /// <param name="MinUsers">Minimum number of participants.</param>
    /// <param name="MaxUsers">Maximum number of participants.</param>
    /// <param name="RelatedHobbies">Hobbies related to the event.</param>
    public record NewEventDto(
        string Title,
        string Description,
        DateTime StartTime,
        DateTime EndTime,
        Location Location,
        decimal Price,
        int MinUsers,
        int MaxUsers,
        IEnumerable<HobbyDto> RelatedHobbies,
        Guid? VenueId = null
    );
}