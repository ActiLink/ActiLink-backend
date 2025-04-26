using ActiLink.Shared.ServiceUtils;

namespace ActiLink.Organizers.Authentication.Service
{
    public interface IAuthService
    {
        public Task<GenericServiceResult<(string AccessToken, string RefreshToken)>> RefreshTokenAsync(string refreshToken);
    }
}
