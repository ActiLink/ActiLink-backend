using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ActiLink.Model;
using Microsoft.IdentityModel.Tokens;

namespace ActiLink.Services
{
    public class JwtTokenProvider
    {
        private readonly string _jwtSecret;
        private readonly string _jwtIssuer;
        private readonly string _jwtAudience;

        public JwtTokenProvider()
        {
            _jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? throw new ArgumentNullException("JWT_SECRET_KEY environment variable is not set.");
            _jwtIssuer = Environment.GetEnvironmentVariable("JWT_VALID_ISSUER") ?? throw new ArgumentNullException("JWT_VALID_ISSUER environment variable is not set.");
            _jwtAudience = Environment.GetEnvironmentVariable("JWT_VALID_AUDIENCE") ?? throw new ArgumentNullException("JWT_VALID_AUDIENCE environment variable is not set.");
        }

        public string GenerateAccessToken(Organizer user)
        {
            var key = Encoding.ASCII.GetBytes(_jwtSecret);
            var tokenHandler = new JwtSecurityTokenHandler();

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id ?? ""),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Name, user.UserName ?? "")
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = _jwtIssuer,
                Audience = _jwtAudience,
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }
}
