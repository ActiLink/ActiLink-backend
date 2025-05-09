using ActiLink.Shared.ServiceUtils;

namespace ActiLink.Events.Service
{
    public interface IEventService
    {
        public Task<GenericServiceResult<Event>> CreateEventAsync(CreateEventObject ceo);
        public Task<ServiceResult> DeleteEventAsync(Event eventToDelete);
        public Task<Event?> GetEventByIdAsync(Guid eventId);
        public Task<IEnumerable<Event>> GetAllEventsAsync();
        public Task<GenericServiceResult<Event>> UpdateEventAsync(Guid eventId, UpdateEventObject eventToUpdate, string userIdFromToken);
        public Task<ServiceResult> DeleteEventByIdAsync(Guid eventId, string userIdFromToken);
        public Task<GenericServiceResult<Event>> SignUpForEventAsync(Guid eventId, string userIdFromToken);
    }
}
