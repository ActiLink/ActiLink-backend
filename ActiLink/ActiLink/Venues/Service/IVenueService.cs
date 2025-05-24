using ActiLink.Shared.ServiceUtils;

namespace ActiLink.Venues.Service
{
    public interface IVenueService
    {
        public Task<GenericServiceResult<Venue>> CreateVenueAsync(CreateVenueObject cvo);
        public Task<ServiceResult> DeleteVenueAsync(Venue venueToDelete);
        public Task<ServiceResult> DeleteVenueByIdAsync(Guid venueId, string requestingUserId);
        public Task<GenericServiceResult<Venue>> UpdateVenueAsync(Guid venueId, UpdateVenueObject uvo, string requestingUserId);
        public Task<Venue?> GetVenueByIdAsync(Guid venueId);
        public Task<IEnumerable<Venue>> GetAllVenuesAsync();
    }
}
