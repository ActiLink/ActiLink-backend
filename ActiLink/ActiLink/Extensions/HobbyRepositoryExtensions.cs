using ActiLink.Model;
using ActiLink.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ActiLink.Extensions
{
    public static class HobbyRepositoryExtensions
    {
        public static async Task<List<Hobby>> GetHobbiesByIdsAsync(this IRepository<Hobby> repository, IEnumerable<Guid> hobbyIds)
        {
            return await repository
                .Query()
                .Where(h => hobbyIds.Contains(h.Id))
                .ToListAsync();
        }
    }
}
