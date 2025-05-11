using ActiLink.Shared.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ActiLink.Venues.Infrastructure
{
    public static class VenueRepositoryExtensions
    {
        public static Task<Venue?> GetWholeVenueByIdAsync(this IRepository<Venue> repository, Guid id)
        {
            return repository
                .Query()
                .Include(v => v.Owner)
                .Include(v => v.Events)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public static async Task<IEnumerable<Venue>> GetAllWholeVenuesAsync(this IRepository<Venue> repository)
        {
            return await repository
                .Query()
                .Include(v => v.Owner)
                .Include(v => v.Events)
                .ToListAsync();
        }
    }
}
