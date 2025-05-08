using ActiLink.Shared.Model;

namespace ActiLink.Venues.DTOs
{
    public record NewVenueDto(
        string Name,
        string Description,
        Location Location,
        string Address
    );
}
