using ActiLink.Configuration;
using ActiLink.Organizers.Authentication.Tokens;
using ActiLink.Organizers.Users;
using ActiLink.Organizers.BusinessClients;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ActiLink.UnitTests.AuthenticationTests
{
    [TestClass]
    public class JwtTokenProviderTests
    {
        private IJwtTokenProvider _tokenProvider = null!;
        private JwtSettings _jwtSettings = null!;

        [TestInitialize]
        public void Setup()
        {
            Environment.SetEnvironmentVariable("JWT_SECRET_KEY", "test_secret_key_12345678901234567890");
            Environment.SetEnvironmentVariable("JWT_VALID_ISSUER", "TestIssuer");
            Environment.SetEnvironmentVariable("JWT_VALID_AUDIENCE", "TestAudience");

            _jwtSettings = new JwtSettings
            {
                AccessTokenExpiryMinutes = 60,
                RefreshTokenExpiryDays = 30,
                Roles = new JwtSettings.RoleNames
                {
                    UserRole = "User",
                    BusinessClientRole = "BusinessClient"
                }
            };

            _tokenProvider = new JwtTokenProvider(Options.Create(_jwtSettings));
        }

        [TestMethod]
        public void GenerateAccessToken_ForUser_IncludesUserRole()
        {
            // Arrange
            var user = new User("TestUser", "user@test.com") { Id = "123" };

            // Act
            var token = _tokenProvider.GenerateAccessToken(user);

            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            // Assert
            Assert.IsNotNull(jsonToken);

            var roleClaim = jsonToken!.Claims.FirstOrDefault(c => c.Type == "role");
            Assert.IsNotNull(roleClaim, "Token powinien zawierać claim roli");
            Assert.AreEqual(_jwtSettings.Roles.UserRole, roleClaim!.Value, "Token powinien zawierać rolę User");
        }

        [TestMethod]
        public void GenerateAccessToken_ForBusinessClient_IncludesBusinessClientRole()
        {
            // Arrange
            var businessClient = new BusinessClient("TestBusiness", "business@test.com", "123456789") { Id = "456" };

            // Act
            var token = _tokenProvider.GenerateAccessToken(businessClient);

            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            // Assert
            Assert.IsNotNull(jsonToken);

            var roleClaim = jsonToken!.Claims.FirstOrDefault(c => c.Type == "role");
            Assert.IsNotNull(roleClaim, "Token powinien zawierać claim roli");
            Assert.AreEqual(_jwtSettings.Roles.BusinessClientRole, roleClaim!.Value, "Token powinien zawierać rolę BusinessClient");
        }

        [TestMethod]
        public void GenerateAccessToken_ContainsAllRequiredClaims()
        {
            // Arrange
            var user = new User("TestUser", "user@test.com") { Id = "123" };

            // Act
            var token = _tokenProvider.GenerateAccessToken(user);

            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            // Assert
            Assert.IsNotNull(jsonToken);

            Assert.IsTrue(jsonToken!.Claims.Any(c => c.Type == "token_type" && c.Value == "access"), "Token powinien zawierać token_type: access");

            Assert.IsTrue(jsonToken.Claims.Any(c => c.Type == "nameid" && c.Value == user.Id), "Token powinien zawierać identyfikator użytkownika");
            Assert.IsTrue(jsonToken.Claims.Any(c => c.Type == "email" && c.Value == user.Email), "Token powinien zawierać email użytkownika");
            Assert.IsTrue(jsonToken.Claims.Any(c => c.Type == "unique_name" && c.Value == user.UserName), "Token powinien zawierać nazwę użytkownika");
            Assert.IsTrue(jsonToken.Claims.Any(c => c.Type == "role" && c.Value == "User"), "Token powinien zawierać rolę użytkownika");

        }
    }
}
