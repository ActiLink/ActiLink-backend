using ActiLink.Shared.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ActiLink.Hobbies.Infrastructure
{
    public static class HobbyRepositoryExtensions
    {
        public static async Task<List<Hobby>> GetHobbiesByNamesAsync(this IRepository<Hobby> repository, IEnumerable<string> hobbyNames)
        {
            return await repository
                .Query()
                .Where(h => hobbyNames.Contains(h.Name))
                .ToListAsync();
        }

        public static async Task<Hobby?> GetHobbyByNameAsync(this IRepository<Hobby> repository, string name)
        {
            return await repository
                .Query()
                .FirstOrDefaultAsync(h => h.Name == name);
        }
    }
}
