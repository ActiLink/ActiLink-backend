using ActiLink.Configuration;
using ActiLink.Shared.Repositories;
using ActiLink.Shared.ServiceUtils;
using ActiLink.Organizers.Authentication.Extensions;
using Microsoft.Extensions.Options;

namespace ActiLink.Organizers.Authentication.Service
{
    public class AuthService: IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly JwtTokenProvider _tokenProvider;
        private readonly JwtSettings _jwtSettings;

        private static readonly string[] InvalidRefreshTokenError = ["Invalid refresh token."];
        private static readonly string[] FailedRefreshTokenUpdate = ["Failed to update the refresh token."];

        public AuthService(IUnitOfWork unitOfWork, JwtTokenProvider provider, IOptions<JwtSettings> jwtOptions)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _jwtSettings = jwtOptions.Value ?? throw new ArgumentNullException(nameof(jwtOptions));
            _tokenProvider = provider ?? throw new ArgumentNullException(nameof(provider));
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
                .GetValidTokenWithOwnerAsync(refreshToken);

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
            var saveResult = await _unitOfWork.SaveChangesAsync();
            if (saveResult == 0)
                return GenericServiceResult<(string, string)>.Failure(FailedRefreshTokenUpdate);

            return GenericServiceResult<(string, string)>.Success((newAccessToken, newRefreshToken));
        }
    }
}
