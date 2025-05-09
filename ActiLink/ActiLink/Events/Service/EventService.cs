using ActiLink.Events.Infrastructure;
using ActiLink.Hobbies.Infrastructure;
using ActiLink.Organizers.Users.Infrastructure;
using ActiLink.Shared.Repositories;
using ActiLink.Shared.ServiceUtils;
using AutoMapper;
using Microsoft.EntityFrameworkCore;


namespace ActiLink.Events.Service
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
            var organizer = await _unitOfWork.OrganizerRepository.GetByIdAsync(ceo.OrganizerId);
            if (organizer is null)
                return GenericServiceResult<Event>.Failure(["Organizer not found"]);

            var hobbies = await _unitOfWork.HobbyRepository.GetHobbiesByNamesAsync(ceo.RelatedHobbyNames);

            if (hobbies.Count != ceo.RelatedHobbyNames.Count())
                return GenericServiceResult<Event>.Failure(["Some hobbies not found"], ErrorCode.ValidationError);

            // Map CreateEventObject to Event
            var newEvent = _mapper.Map<Event>(ceo, opts =>
            {
                opts.Items["Organizer"] = organizer;
                opts.Items["Hobbies"] = hobbies;
            });

            // Add entity to the repository
            await _unitOfWork.EventRepository.AddAsync(newEvent);
            var result = await _unitOfWork.SaveChangesAsync();

            // Check if the event was created successfully
            return result > 0
                ? GenericServiceResult<Event>.Success(newEvent)
                : GenericServiceResult<Event>.Failure(["Failed to create event"]);
        }
        /// <summary>
        /// Updates the specified event with the provided details.
        /// </summary>
        /// <param name="eventToUpdate"></param>
        /// <param name="requestingUserId"></param>
        /// <returns>
        /// Returns a <see cref="GenericServiceResult{T}"/> containing the updated <see cref="Event"/> object or null and error messages if the update failed.
        /// </returns>
        public async Task<GenericServiceResult<Event>> UpdateEventAsync(Guid eventId, UpdateEventObject eventToUpdate, string requestingUserId)
        {
            // Check if the event exists
            var existingEvent = await _unitOfWork.EventRepository.GetByIdWithOrganizerAsync(eventId);
            if (existingEvent is null)
                return GenericServiceResult<Event>.Failure(["Event not found"], ErrorCode.NotFound);

            if (existingEvent.Organizer.Id != requestingUserId)
                return GenericServiceResult<Event>.Failure(["You are not authorized to update this event."], ErrorCode.Forbidden);

            var hobbies = await _unitOfWork.HobbyRepository.GetHobbiesByNamesAsync(eventToUpdate.RelatedHobbyNames);

            if (hobbies.Count != eventToUpdate.RelatedHobbyNames.Count())
                return GenericServiceResult<Event>.Failure(["Some hobbies not found"], ErrorCode.ValidationError);

            // Map updated properties
            _mapper.Map(
               eventToUpdate,
               existingEvent,
               opts =>
               {
                   opts.Items["Hobbies"] = hobbies;
               });


            // Update entity in the repository
            _unitOfWork.EventRepository.Update(existingEvent);
            var result = await _unitOfWork.SaveChangesAsync();

            // Check if the event was updated successfully
            return result > 0
                ? GenericServiceResult<Event>.Success(existingEvent)
                : GenericServiceResult<Event>.Failure(["Failed to update event"]);
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
        /// Verifies the user and deletes the specified event.
        /// </summary>
        /// <param name="eventToDelete"></param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="ServiceResult"/> of the operation.
        /// </returns>
        public async Task<ServiceResult> DeleteEventByIdAsync(Guid eventId, string requestingUserId)
        {
            var eventToDelete = await _unitOfWork.EventRepository
                .Query()
                .Include(e => e.Organizer)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (eventToDelete is null)
                return ServiceResult.Failure(["Event not found"], ErrorCode.NotFound);

            if (eventToDelete.Organizer?.Id != requestingUserId)
                return ServiceResult.Failure(["You are not authorized to delete this event."], ErrorCode.Forbidden);

            _unitOfWork.EventRepository.Delete(eventToDelete);
            var result = await _unitOfWork.SaveChangesAsync();

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
            return await _unitOfWork.EventRepository.GetByIdWithOrganizerAsync(eventId);
        }

        /// <summary>
        /// Gets all events.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IEnumerable{T}"/> of all events.
        /// </returns>
        public async Task<IEnumerable<Event>> GetAllEventsAsync()
        {
            return await _unitOfWork.EventRepository.GetAllEventsAsync();
        }

        public async Task<GenericServiceResult<Event>> SignUpForEventAsync(Guid eventId, string userIdFromToken)
        {
            var eventToSignUp = await _unitOfWork.EventRepository.GetByIdWithOrganizerAsync(eventId);

            if (eventToSignUp is null)
                return GenericServiceResult<Event>.Failure(["Event not found"], ErrorCode.NotFound);

            if (eventToSignUp.SignUpList.Any(u => u.Id == userIdFromToken))
                return GenericServiceResult<Event>.Failure(["You are already signed up for this event."], ErrorCode.ValidationError);

            if (eventToSignUp.MaxUsers > 0 && eventToSignUp.SignUpList.Count >= eventToSignUp.MaxUsers)
                return GenericServiceResult<Event>.Failure(["Event is full"], ErrorCode.ValidationError);

            var user = await _unitOfWork.UserRepository.GetUserWithSignedUpEventsByIdAsync(userIdFromToken);
            if (user is null)
                return GenericServiceResult<Event>.Failure(["User not found"], ErrorCode.NotFound);

            eventToSignUp.SignUpList.Add(user);
            user.SignedUpEvents.Add(eventToSignUp);

            var result = await _unitOfWork.SaveChangesAsync();
            return result > 0
                ? GenericServiceResult<Event>.Success(eventToSignUp)
                : GenericServiceResult<Event>.Failure(["Failed to sign up for event"]);
        }
    }
}
