﻿using ActiLink.Configuration;
using ActiLink.Model;
using ActiLink.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ActiLink.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<Organizer> _userManager;
        private static readonly string[] InvalidLoginError = ["Invalid email or password."];
        private static readonly string[] InvalidRefreshTokenError = ["Invalid refresh token."];
        private readonly JwtTokenProvider _tokenProvider;
        private readonly JwtSettings _jwtSettings;
        public UserService(IUnitOfWork unitOfWork, UserManager<Organizer> userManager, JwtTokenProvider provider, IOptions<JwtSettings> jwtOptions)
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
            await _unitOfWork.SaveChangesAsync();

            return GenericServiceResult<(string, string)>.Success((accessToken, refreshToken));

        }

        /// <summary>
        /// Refreshes the access token using the specified <paramref name="refreshToken"/>.
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="GenericServiceResult{T}"/> of the operation 
        /// with the new access and refresh tokens or null and error messages if the refresh token is invalid or expired.
        /// </returns>
        public async Task<GenericServiceResult<(string AccessToken, string RefreshToken)>> RefreshTokenAsync(string refreshToken)
        {
            var refreshTokenEntity = await _unitOfWork
                .RefreshTokenRepository
                .Query()
                .Include(rt => rt.TokenOwner)
                .FirstOrDefaultAsync(rt =>
                    rt.Token == refreshToken &&
                    rt.ExpiryTimeUtc > DateTime.UtcNow);

            if (refreshTokenEntity == null)
                return GenericServiceResult<(string, string)>.Failure(InvalidRefreshTokenError);

            var user = refreshTokenEntity.TokenOwner;

            var newAccessToken = _tokenProvider.GenerateAccessToken(user);
            var newRefreshToken = _tokenProvider.GenerateRefreshToken(user.Id!);

            _unitOfWork.RefreshTokenRepository.Delete(refreshTokenEntity);

            var newToken = new RefreshToken
            {
                Token = newRefreshToken,
                ExpiryTimeUtc = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
                TokenOwner = user
            };

            await _unitOfWork.RefreshTokenRepository.AddAsync(newToken);
            await _unitOfWork.SaveChangesAsync();

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
