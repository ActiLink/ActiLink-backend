using ActiLink.Organizers.DTOs;

namespace ActiLink.Events.DTOs
{
    public record ReducedEventDto(
        Guid Id,
        string Title,
        OrganizerDto Organizer,
        DateTime StartTime
        );
}
