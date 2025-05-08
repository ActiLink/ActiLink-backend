using ActiLink.Events.DTOs;
using ActiLink.Organizers.BusinessClients.DTOs;
using ActiLink.Shared.Model;

namespace ActiLink.Venues.DTOs
{
    public record VenueDto(
        Guid Id,
        string Name,
        string Description,
        Location Location,
        string Address,
        VenueOwnerDto Owner,
        List<ReducedEventDto> Events
        );
}
