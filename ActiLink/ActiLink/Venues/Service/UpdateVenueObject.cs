using ActiLink.Shared.Model;

namespace ActiLink.Venues.Service
{
    public record UpdateVenueObject(
        string Name,
        string Description,
        Location Location,
        string Address
    )
    {
        private UpdateVenueObject() : this(
            default!,
            string.Empty,
            new Location(0, 0),
            string.Empty)
        { }
    }
}
