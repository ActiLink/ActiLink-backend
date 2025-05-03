using ActiLink.Configuration;
using ActiLink.Hobbies.Infrastructure;
using ActiLink.Organizers.Authentication;
using ActiLink.Organizers.Authentication.Tokens;
using ActiLink.Organizers.Users.Infrastructure;
using ActiLink.Shared.Repositories;
using ActiLink.Shared.ServiceUtils;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ActiLink.Organizers.Users.Service
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<Organizer> _userManager;
        private static readonly string[] InvalidLoginError = ["Invalid email or password."];
        private static readonly string[] FailedRefreshTokenSave = ["Failed to save the refresh token."];
        private static readonly string[] UserNotFoundError = ["User not found."];
        private static readonly string[] SomeHobbiesNotFoundError = ["Some hobbies not found."];
        private readonly IJwtTokenProvider _tokenProvider;
        private readonly JwtSettings _jwtSettings;
        public UserService(IUnitOfWork unitOfWork, UserManager<Organizer> userManager, IJwtTokenProvider provider, IOptions<JwtSettings> jwtOptions)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _tokenProvider = provider ?? throw new ArgumentNullException(nameof(provider));
            _jwtSettings = jwtOptions.Value ?? throw new ArgumentNullException(nameof(jwtOptions));
        }

        /// <summary>
        /// Creates a new user with the specified <paramref name="username"/>, <paramref name="email"/> and <paramref name="password"/>.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="email"></param>
        /// <param name="password">Must meet the password policy requirements</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="GenericServiceResult{T}"/> of the operation
        /// with the created <see cref="User"/> object or null and error messages if the creation failed.
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
        /// The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="GenericServiceResult{T}"/> of the operation 
        /// with the access and refresh tokens or null and error messages if the authentication failed.
        /// </returns>
        public async Task<GenericServiceResult<(string AccessToken, string RefreshToken)>> LoginAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                return GenericServiceResult<(string, string)>.Failure(InvalidLoginError);

            var result = await _userManager.CheckPasswordAsync(user, password);

            if (!result)
                return GenericServiceResult<(string, string)>.Failure(InvalidLoginError);

            var accessToken = _tokenProvider.GenerateAccessToken(user);
            var refreshToken = _tokenProvider.GenerateRefreshToken(user.Id);

            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                ExpiryTimeUtc = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
                TokenOwner = user
            };

            await _unitOfWork.RefreshTokenRepository.AddAsync(refreshTokenEntity);
            var saveResult = await _unitOfWork.SaveChangesAsync();
            if (saveResult == 0)
                return GenericServiceResult<(string, string)>.Failure(FailedRefreshTokenSave);

            return GenericServiceResult<(string, string)>.Success((accessToken, refreshToken));
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
        /// Finds and returns a user, if any, who has the specified <paramref name="id"/> and includes their hobbies.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing the user matching the specified <paramref name="id"/> if it exists.
        /// </returns>
        public async Task<User?> GetUserWithHobbiesByIdAsync(string id)
        {
            return await _unitOfWork.UserRepository.GetUserWithHobbiesByIdAsync(id);
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


        /// <summary>
        /// Updates an existing user with the specified <paramref name="id"/> and details in the request body.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="updateUserObject"></param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="ServiceResult"/> of the operation.
        /// </returns>
        public async Task<GenericServiceResult<User>> UpdateUserAsync(string id, UpdateUserObject updateUserObject)
        {
            var user = await GetUserByIdAsync(id);
            if (user is null)
                return GenericServiceResult<User>.Failure(UserNotFoundError, ErrorCode.NotFound);

            var hobbies = await _unitOfWork.HobbyRepository.GetHobbiesByNamesAsync(updateUserObject.HobbyNames);

            if (hobbies.Count != updateUserObject.HobbyNames.Count)
                return GenericServiceResult<User>.Failure(SomeHobbiesNotFoundError, ErrorCode.ValidationError);

            var result = await _userManager.SetUserNameAsync(user, updateUserObject.Name);
            if (!result.Succeeded)
                return GenericServiceResult<User>.Failure(result.Errors.Select(e => e.Description), ErrorCode.ValidationError);

            result = await _userManager.SetEmailAsync(user, updateUserObject.Email);
            if (!result.Succeeded)
                return GenericServiceResult<User>.Failure(result.Errors.Select(e => e.Description), ErrorCode.ValidationError);


            user.Hobbies = hobbies;
            result = await _userManager.UpdateAsync(user);

            return result.Succeeded ? GenericServiceResult<User>.Success(user) : GenericServiceResult<User>.Failure(result.Errors.Select(e => e.Description));
        }

    }
}
