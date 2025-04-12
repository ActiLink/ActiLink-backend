using ActiLink.Model;
using ActiLink.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ActiLink.Extensions
{
    public static class EventRepositoryExtensions
    {
        public static async Task<Event?> GetByIdWithOrganizerAsync(this IRepository<Event> repository, Guid id)
        {
            return await repository.Query()
            .Include(e => e.Organizer)
            .FirstOrDefaultAsync(e => e.Id == id);
        }
    }
}
