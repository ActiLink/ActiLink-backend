using ActiLink.Shared.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ActiLink.Organizers.Authentication.Extensions
{
    public static class RefreshTokenRepositoryExtensions
    {
        public static async Task<RefreshToken?> GetValidTokenWithOwnerAsync(
            this IRepository<RefreshToken> repository,
            string token)
        {
            return await repository
                .Query()
                .Include(rt => rt.TokenOwner)
                .FirstOrDefaultAsync(rt =>
                    rt.Token == token &&
                    rt.ExpiryTimeUtc > DateTime.UtcNow);
        }
    }
}
