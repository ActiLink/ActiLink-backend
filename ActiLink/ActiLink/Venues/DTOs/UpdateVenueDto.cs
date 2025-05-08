using ActiLink.Shared.Model;

namespace ActiLink.Venues.DTOs
{
    public record UpdateVenueDto(
        string Name,
        string Description,
        Location Location,
        string Address
    );
}
