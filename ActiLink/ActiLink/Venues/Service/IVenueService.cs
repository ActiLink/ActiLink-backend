using ActiLink.Shared.ServiceUtils;

namespace ActiLink.Venues.Service
{
    public interface IVenueService
    {
        public Task<GenericServiceResult<Venue>> CreateVenueAsync(CreateVenueObject cvo);
        public Task<ServiceResult> DeleteVenueAsync(Venue venueToDelete);
    }
}
