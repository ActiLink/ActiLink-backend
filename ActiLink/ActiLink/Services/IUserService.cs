using ActiLink.Model;
using Microsoft.AspNetCore.Identity;

namespace ActiLink.Services
{
    public interface IUserService
    {
        public Task<GenericServiceResult<User>> CreateUserAsync(string username, string email, string password);
        public Task<GenericServiceResult<(string AccessToken, string RefreshToken)>> LoginAsync(string email, string password);
        public Task<GenericServiceResult<(string AccessToken, string RefreshToken)>> RefreshTokenAsync(string refreshToken);
        public Task<IEnumerable<User>> GetUsersAsync();
        public Task<User?> GetUserByIdAsync(string id);
        public Task<ServiceResult> DeleteUserAsync(User user);
    }
}
