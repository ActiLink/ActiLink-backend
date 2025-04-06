using ActiLink.Model;

namespace ActiLink.Services
{
    public interface IEventService
    {
        public Task<GenericServiceResult<Event>> CreateEventAsync(CreateEventObject ceo);
        public Task<ServiceResult> DeleteEventAsync(Event eventToDelete);
        public Task<Event?> GetEventByIdAsync(Guid eventId);
        public Task<IEnumerable<Event>> GetAllEventsAsync();
    }
}
