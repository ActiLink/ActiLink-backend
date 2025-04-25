using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ActiLink.Configuration;
using ActiLink.Organizers.Users;
using ActiLink.Organizers.BusinessClients;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ActiLink.Organizers.Authentication.Tokens
{
    public class JwtTokenProvider : IJwtTokenProvider
    {
        private readonly string _jwtSecret;
        private readonly string _jwtIssuer;
        private readonly string _jwtAudience;
        private readonly JwtSettings _jwtSettings;

        public JwtTokenProvider(IOptions<JwtSettings> jwtOptions)
        {
            _jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? throw new ArgumentNullException("JWT_SECRET_KEY environment variable is not set.");
            _jwtIssuer = Environment.GetEnvironmentVariable("JWT_VALID_ISSUER") ?? throw new ArgumentNullException("JWT_VALID_ISSUER environment variable is not set.");
            _jwtAudience = Environment.GetEnvironmentVariable("JWT_VALID_AUDIENCE") ?? throw new ArgumentNullException("JWT_VALID_AUDIENCE environment variable is not set.");
            _jwtSettings = jwtOptions.Value;
        }

        public string GenerateAccessToken(Organizer user)
        {
            var key = Encoding.ASCII.GetBytes(_jwtSecret);
            var tokenHandler = new JwtSecurityTokenHandler();

            var claims = new List<Claim>
            {
                new Claim("token_type", "access"),
                new Claim(ClaimTypes.NameIdentifier, user.Id ?? ""),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Name, user.UserName ?? "")
            };
            AddRoleClaim(user, claims);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = _jwtIssuer,
                Audience = _jwtAudience,
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string GenerateRefreshToken(string userId)
        {
            var key = Encoding.ASCII.GetBytes(_jwtSecret);
            var tokenHandler = new JwtSecurityTokenHandler();

            var claims = new[]
            {
                new Claim("token_type", "refresh"),
                new Claim(ClaimTypes.NameIdentifier, userId)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = _jwtIssuer,
                Audience = _jwtAudience,
                Expires = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        private void AddRoleClaim(Organizer user, List<Claim> claims)
        {
            var userType = user.GetType(); //skull emoji

            if (userType == typeof(User))
            {
                claims.Add(new Claim(ClaimTypes.Role, _jwtSettings.Roles.UserRole));
            }
            else if (userType == typeof(BusinessClient))
            {
                claims.Add(new Claim(ClaimTypes.Role, _jwtSettings.Roles.BusinessClientRole));
            }
        }
    }
}
