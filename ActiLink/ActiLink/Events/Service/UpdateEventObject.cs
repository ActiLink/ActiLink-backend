using ActiLink.Shared.Model;

namespace ActiLink.Events.Service
{
    public record UpdateEventObject(
        Guid Id,
        string Title,
        string Description,
        DateTime StartTime,
        DateTime EndTime,
        Location Location,
        decimal Price,
        int MinUsers,
        int MaxUsers,
        IEnumerable<Guid> RelatedHobbyIds
    );
}
