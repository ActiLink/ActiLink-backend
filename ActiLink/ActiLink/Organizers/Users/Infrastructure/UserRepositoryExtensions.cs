using ActiLink.Shared.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ActiLink.Organizers.Users.Infrastructure
{
    public static class UserRepositoryExtensions
    {
        public static async Task<User?> GetUserWithHobbiesByIdAsync(this IRepository<User> repository, string userId)
        {
            return await repository
                .Query()
                .Include(u => u.Hobbies)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }
    }
}
