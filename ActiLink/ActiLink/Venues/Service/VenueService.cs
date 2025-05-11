using ActiLink.Shared.Repositories;
using ActiLink.Shared.ServiceUtils;
using ActiLink.Venues.Infrastructure;
using AutoMapper;

namespace ActiLink.Venues.Service
{
    public class VenueService : IVenueService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public VenueService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }


        /// <summary>
        /// Creates a new venue.
        /// </summary>
        /// <param name="cvo">The CreateVenueObject containing the details of the venue to be created.</param>
        /// <param name="userIdFromToken">The user ID from the token.</param>
        /// <returns></returns>
        public async Task<GenericServiceResult<Venue>> CreateVenueAsync(CreateVenueObject cvo)
        {
            var owner = await _unitOfWork.BusinessClientRepository.GetByIdAsync(cvo.OwnerId);
            if (owner is null)
                return GenericServiceResult<Venue>.Failure(["User not found"], ErrorCode.NotFound);

            var venue = _mapper.Map<Venue>(cvo, opts =>
            {
                opts.Items["Owner"] = owner;
            });

            await _unitOfWork.VenueRepository.AddAsync(venue);

            var result = await _unitOfWork.SaveChangesAsync();

            return result > 0
                ? GenericServiceResult<Venue>.Success(venue)
                : GenericServiceResult<Venue>.Failure(["Failed to create venue"]);
        }

        /// <summary>
        /// Deletes the specified venue.
        /// </summary>
        /// <param name="venueToDelete"></param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="ServiceResult"/> indicating the success or failure of the operation.
        /// </returns>
        public async Task<ServiceResult> DeleteVenueAsync(Venue venueToDelete)
        {
            _unitOfWork.VenueRepository.Delete(venueToDelete);
            int result = await _unitOfWork.SaveChangesAsync();

            return result > 0
                ? ServiceResult.Success()
                : ServiceResult.Failure(["Failed to delete venue"]);
        }

        /// <summary>
        /// Retrieves a venue by its ID.
        /// </summary>
        /// <param name="venueId">The ID of the venue to retrieve.</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="Venue"/> object if found, or null if not found.
        /// </returns>
        public async Task<Venue?> GetVenueByIdAsync(Guid venueId)
        {
            return await _unitOfWork.VenueRepository.GetVenueByIdAsync(venueId);
        }

        /// <summary>
        /// Retrieves all venues.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing an <see cref="IEnumerable{Venue}"/> of all venues.
        /// </returns>
        public async Task<IEnumerable<Venue>> GetAllVenuesAsync()
        {
            return await _unitOfWork.VenueRepository.GetAllVenuesAsync();
        }
    }
}
