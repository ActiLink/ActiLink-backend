using ActiLink.Model;

namespace ActiLink.Services
{
    public record CreateEventObject(
        string OrganizerId,
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
