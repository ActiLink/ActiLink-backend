using ActiLink.Model;

namespace ActiLink.Services
{
    public record CreateEventObject(
        string OrganizerId,
        DateTime StartTime,
        DateTime EndTime,
        Location Location,
        decimal Price,
        int MinUsers
        int MaxUsers,
    );
}
