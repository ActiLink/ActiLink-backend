using ActiLink.Model;

namespace ActiLink.DTOs
{
    public record UpdateEventDto(
    string OrganizerId,
    DateTime StartTime,
    DateTime EndTime,
    Location Location,
    decimal Price,
    int MinUsers,
    int MaxUsers,
    IEnumerable<Guid> RelatedHobbyIds // Could be names too idk
);
}
