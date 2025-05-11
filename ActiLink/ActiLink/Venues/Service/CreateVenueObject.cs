using ActiLink.Shared.Model;

namespace ActiLink.Venues.Service
{
    public record CreateVenueObject(
        string OwnerId,
        string Name,
        string Description,
        Location Location,
        string Address
        )
    {
        private CreateVenueObject() : this(
            default!,
            string.Empty,
            string.Empty,
            new Location(0, 0),
            string.Empty)
        { }
    }
}
