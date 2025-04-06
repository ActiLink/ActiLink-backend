using ActiLink.Model;
using ActiLink.Repositories;
using AutoMapper;

namespace ActiLink.Services
{
    public class EventService : IEventService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public EventService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <summary>
        /// Creates a new event with the specified details.
        /// </summary>
        /// <param name="ceo"></param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="GenericServiceResult{T}"/> of the operation
        /// with the created <see cref="Event"/> object or null and error messages if the creation failed.
        /// </returns>
        public async Task<GenericServiceResult<Event>> CreateEventAsync(CreateEventObject ceo)
        {
            // Check if organizer exists
            var organizer = await _unitOfWork.UserRepository.GetByIdAsync(ceo.OrganizerId);
            if (organizer is null)
                return GenericServiceResult<Event>.Failure(["Organizer not found"]);

            // Map CreateEventObject to Event
            var newEvent = _mapper.Map<Event>(ceo, opts => opts.Items["Organizer"] = organizer);

            // Add entity to the repository
            await _unitOfWork.EventRepository.AddAsync(newEvent);
            var result = await _unitOfWork.SaveChangesAsync();

            // Check if the event was created successfully
            return result > 0
                ? GenericServiceResult<Event>.Success(newEvent)
                : GenericServiceResult<Event>.Failure(["Failed to create event"]);
        }

        /// <summary>
        /// Deletes the specified event.
        /// </summary>
        /// <param name="eventToDelete"></param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="ServiceResult"/> of the operation.
        /// </returns>
        public async Task<ServiceResult> DeleteEventAsync(Event eventToDelete)
        {
            _unitOfWork.EventRepository.Delete(eventToDelete);
            int result = await _unitOfWork.SaveChangesAsync();

            return result > 0
                ? ServiceResult.Success()
                : ServiceResult.Failure(["Failed to delete event"]);
        }

        /// <summary>
        /// Retrieves an event by its ID
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="GenericServiceResult{T}"/> of the operation
        /// with the <see cref="Event"/> object or null and error messages if the retrieval failed.
        /// </returns>
        public async Task<Event?> GetEventByIdAsync(Guid eventId)
        {
            return await _unitOfWork.EventRepository.GetByIdAsync(eventId);
        }

        /// <summary>
        /// Gets all events.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IEnumerable{T}"/> of all events.
        /// </returns>
        public async Task<IEnumerable<Event>> GetAllEventsAsync()
        {
            return await _unitOfWork.EventRepository.GetAllAsync();
        }
    }
}
