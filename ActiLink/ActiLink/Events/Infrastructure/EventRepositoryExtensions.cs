using ActiLink.Shared.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ActiLink.Events.Infrastructure
{
    public static class EventRepositoryExtensions
    {
        public static async Task<Event?> GetByIdWithOrganizerAsync(this IRepository<Event> repository, Guid id)
        {
            return await repository
                .Query()
                .Include(e => e.Organizer)
                .Include(e => e.SignUpList)
                .Include(e => e.RelatedHobbies)
                .FirstOrDefaultAsync(e => e.Id == id);
        }
        public static async Task<IEnumerable<Event>> GetAllEventsAsync(this IRepository<Event> repository)
        {
            return await repository
                .Query()
                .Include(e => e.Organizer)
                .Include(e => e.SignUpList)
                .Include(e => e.RelatedHobbies)
                .ToListAsync();
        }
    }
}
