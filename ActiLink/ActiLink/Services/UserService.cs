using ActiLink.DTOs;
using ActiLink.Model;
using ActiLink.Repositories;
using AutoMapper;
using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ActiLink.Services
{
    public class UserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<Organizer> _userManager;
        private readonly SignInManager<Organizer> _signInManager;
        private readonly string _jwtSecret;
        private readonly string _jwtIssuer;
        private readonly string _jwtAudience;
        public UserService(IUnitOfWork unitOfWork, UserManager<Organizer> userManager, SignInManager<Organizer> signInManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
            _jwtIssuer = Environment.GetEnvironmentVariable("JWT_VALID_ISSUER");
            _jwtAudience = Environment.GetEnvironmentVariable("JWT_VALID_AUDIENCE");
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
                return GenericServiceResult<(string,string)>.Failure(new[] { "Invalid email or password." });

            var result = await _signInManager.PasswordSignInAsync(user, password, isPersistent: false, lockoutOnFailure: false);

            if (!result.Succeeded)
                return GenericServiceResult<(string,string)>.Failure(new[] { "Invalid email or password." });

            var accessToken = GenerateJwtAccessToken(user);
            var refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(30);

            await _userManager.UpdateAsync(user);

            return GenericServiceResult<(string, string)>.Success((accessToken, refreshToken));

        }
        // generate access token for user
        private string GenerateJwtAccessToken(Organizer user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret); 
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                // TODO: in the future roles will need to be added to the claims to create a correct token for each user
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id ?? ""),
                    new Claim(ClaimTypes.Email, user.Email ?? ""),
                    new Claim(ClaimTypes.Name, user.UserName ?? "")
                }),
                Issuer = _jwtIssuer,
                Audience = _jwtAudience,
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        // generate refresh token for user
        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        public async Task<GenericServiceResult<(string AccessToken, string RefreshToken)>> RefreshTokenAsync(string refreshToken)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u =>
                    u.RefreshToken == refreshToken &&
                    u.RefreshTokenExpiryTime > DateTime.UtcNow);

            if (user == null)
                return GenericServiceResult<(string, string)>.Failure(new[] { "Invalid or expired refresh token." });

            var newAccessToken = GenerateJwtAccessToken(user);
            var newRefreshToken = GenerateRefreshToken();

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
