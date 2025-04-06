using ActiLink.Model;
using ActiLink.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ActiLink.Services
{
    public class UserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<Organizer> _userManager;
        private static readonly string[] InvalidLoginError = ["Invalid email or password."];
        private static readonly string[] InvalidRefreshTokenError = ["Invalid refresh token."];
        private readonly TokenGenerator _jwtTokenProvider;
        public UserService(IUnitOfWork unitOfWork, UserManager<Organizer> userManager, TokenGenerator tokenGenerator)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _jwtTokenProvider = tokenGenerator ?? throw new ArgumentNullException(nameof(tokenGenerator));
        }

        /// <summary>
        /// Creates a new user with the specified <paramref name="username"/>, <paramref name="email"/> and <paramref name="password"/>.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="email"></param>
        /// <param name="password">Must meet the password policy requirements</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="ServiceResult"/> of the operation.
        /// </returns>
        public async Task<GenericServiceResult<User>> CreateUserAsync(string username, string email, string password)
        {
            var user = new User(username, email);
            var result = await _userManager.CreateAsync(user, password);

            return result.Succeeded ? GenericServiceResult<User>.Success(user) : GenericServiceResult<User>.Failure(result.Errors.Select(e => e.Description));
        }

        /// <summary>
        /// Authenticates a user with the specified <paramref name="email"/> and <paramref name="password"/>.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="ServiceResult"/> of the operation.
        /// </returns>
        public async Task<GenericServiceResult<(string AccessToken, string RefreshToken)>> LoginAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                return GenericServiceResult<(string, string)>.Failure(InvalidLoginError);

            var result = await _userManager.CheckPasswordAsync(user, password);

            if (!result)
                return GenericServiceResult<(string, string)>.Failure(InvalidLoginError);

            var accessToken = _jwtTokenProvider.GenerateAccessToken(user);
            var refreshToken = _jwtTokenProvider.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(30);

            await _userManager.UpdateAsync(user);

            return GenericServiceResult<(string, string)>.Success((accessToken, refreshToken));

        }

        /// <summary>
        /// Refreshes the access token using the specified <paramref name="refreshToken"/>.
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="ServiceResult"/> of the operation with the new access and refresh tokens.
        /// </returns>
        public async Task<GenericServiceResult<(string AccessToken, string RefreshToken)>> RefreshTokenAsync(string refreshToken)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u =>
                    u.RefreshToken == refreshToken &&
                    u.RefreshTokenExpiryTime > DateTime.UtcNow);

            if (user == null)
                return GenericServiceResult<(string, string)>.Failure(InvalidRefreshTokenError);

            var newAccessToken = _jwtTokenProvider.GenerateAccessToken(user);
            var newRefreshToken = _jwtTokenProvider.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(30);

            await _userManager.UpdateAsync(user);

            return GenericServiceResult<(string, string)>.Success((newAccessToken, newRefreshToken));
        }

        /// <summary>
        /// Gets all users
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IEnumerable{T}"/> of all users.
        /// </returns>
        public async Task<IEnumerable<User>> GetUsersAsync()
        {
            return await _unitOfWork.UserRepository.GetAllAsync();
        }

        /// <summary>
        /// Finds and returns a user, if any, who has the specified <paramref name="id"/>.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing the user matching the specified <paramref name="id"/> if it exists.
        /// </returns>
        public async Task<User?> GetUserByIdAsync(string id)
        {
            return await _userManager.FindByIdAsync(id) as User;
        }

        /// <summary>
        /// Deletes a <paramref name="user"/> if exists
        /// </summary>
        /// <param name="user"></param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="ServiceResult"/> of the operation.
        /// </returns>
        public async Task<ServiceResult> DeleteUserAsync(User user)
        {
            var result = await _userManager.DeleteAsync(user);

            return result.Succeeded ? ServiceResult.Success() : ServiceResult.Failure(result.Errors.Select(e => e.Description));
        }

    }
}
