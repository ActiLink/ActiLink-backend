using ActiLink.Shared.Model;

namespace ActiLink.Events.Service
{
    public record UpdateEventObject(
        string Title,
        string Description,
        DateTime StartTime,
        DateTime EndTime,
        Location Location,
        decimal Price,
        int MinUsers,
        int MaxUsers,
        IEnumerable<string> RelatedHobbyNames,
		string VenueId
	)
    {
        private UpdateEventObject() : this(
            default!,
            default!,
            default,
            default,
            default!,
            default,
            default,
            default,
            default!,
            default!)
        {
        }
    }
}
