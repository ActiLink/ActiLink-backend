using ActiLink.Model;

namespace ActiLink.Services
{
    public record UpdateEventObject(
        Guid Id,
        string OrganizerId,
        DateTime StartTime,
        DateTime EndTime,
        Location Location,
        decimal Price,
        int MinUsers,
        int MaxUsers,
        IEnumerable<Guid> RelatedHobbyIds
    );
}
