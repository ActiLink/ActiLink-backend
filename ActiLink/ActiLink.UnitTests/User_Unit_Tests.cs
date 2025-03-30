using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ActiLink.Controllers;
using ActiLink.Services;
using ActiLink.DTOs;
using ActiLink.Model;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using ActiLink.Repositories;
using AutoMapper;
using Microsoft.Extensions.Configuration;

namespace ActiLink.UnitTests
{
    [TestClass]
    public class UserAuthTests
    {
        private Mock<UserManager<Organizer>> _mockUserManager;
        private Mock<SignInManager<Organizer>> _mockSignInManager;
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<ILogger<UsersController>> _mockLogger;
        private Mock<IMapper> _mockMapper;
        private Mock<IConfiguration> _mockConfiguration;
        private UsersController _controller;
        private UserService _userService;
        private TokenGenerator _tokenGenerator;

        [TestInitialize]
        public void Setup()
        {
            // Mockowanie zależności dla UserService
            var store = new Mock<IUserStore<Organizer>>();
            _mockUserManager = new Mock<UserManager<Organizer>>(
                store.Object, null, null, null, null, null, null, null, null);

            _mockSignInManager = new Mock<SignInManager<Organizer>>(
                _mockUserManager.Object,
                Mock.Of<Microsoft.AspNetCore.Http.IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<Organizer>>(),
                null, null, null, null);

            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockConfiguration = new Mock<IConfiguration>();

            // Ustawienie zmiennych środowiskowych dla TokenGenerator
            Environment.SetEnvironmentVariable("JWT_SECRET_KEY", "test_secret_key_12345678901234567890");
            Environment.SetEnvironmentVariable("JWT_VALID_ISSUER", "TestIssuer");
            Environment.SetEnvironmentVariable("JWT_VALID_AUDIENCE", "TestAudience");

            // Inicjalizacja TokenGenerator z mockowaną konfiguracją
            _tokenGenerator = new TokenGenerator(_mockConfiguration.Object);

            // Tworzenie rzeczywistej instancji UserService z mockami
            _userService = new UserService(
                _mockUnitOfWork.Object,
                _mockUserManager.Object,
                _mockSignInManager.Object,
                _tokenGenerator);

            // Mockowanie zależności dla UsersController
            _mockLogger = new Mock<ILogger<UsersController>>();
            _mockMapper = new Mock<IMapper>();
            _controller = new UsersController(_mockLogger.Object, _userService, _mockMapper.Object);
        }

        [TestMethod]
        public async Task Register_ValidUser_ReturnsCreatedResult()
        {
            // Arrange
            var newUserDto = new NewUserDto("JanBachalski", "JB123@gmail.com", "Kuna123!");
            var user = new User(newUserDto.Name, newUserDto.Email) { Id = "123" };
            var userDto = new UserDto("123", newUserDto.Name, newUserDto.Email);

            // Mockowanie UserManager.CreateAsync
            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            // Mockowanie UserManager.FindByEmailAsync
            _mockUserManager.Setup(x => x.FindByEmailAsync(newUserDto.Email))
                .ReturnsAsync(user);

            _mockMapper.Setup(x => x.Map<UserDto>(It.IsAny<User>()))
                .Returns(userDto);

            // Act
            var result = await _controller.CreateUserAsync(newUserDto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(CreatedAtActionResult));
            var createdAtResult = result as CreatedAtActionResult;
            Assert.AreEqual(nameof(UsersController.GetUserByIdAsync), createdAtResult.ActionName);
            Assert.AreEqual(userDto, createdAtResult.Value);
        }

        [TestMethod]
        public async Task Register_InvalidUser_ReturnsBadRequest()
        {
            // Arrange
            var newUserDto = new NewUserDto("", "invalid-email", "short");
            var errors = new[] { "Invalid email", "Password too short" };

            // Mockowanie nieudanej rejestracji
            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(errors.Select(e => new IdentityError { Description = e }).ToArray()));

            // Act
            var result = await _controller.CreateUserAsync(newUserDto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult.Value);
        }

        [TestMethod]
        public async Task LoginAsync_ValidCredentials_ReturnsTokens()
        {
            // Arrange
            var email = "test@example.com";
            var password = "Password123!";
            var user = new User("TestUser", email)
            {
                Id = "123",
                Email = email,
                UserName = "TestUser"
            };

            // Mockowanie UserManager.FindByEmailAsync
            _mockUserManager.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync(user);

            // Mockowanie UserManager.CheckPasswordAsync
            _mockUserManager.Setup(x => x.CheckPasswordAsync(user, password))
                .ReturnsAsync(true);

            // Mockowanie UserManager.UpdateAsync
            _mockUserManager.Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _userService.LoginAsync(email, password);

            // Assert
            Assert.IsTrue(result.Succeeded);
            Assert.IsNotNull(result.Data);

            var tokens = result.Data;
            Assert.IsFalse(string.IsNullOrEmpty(tokens.AccessToken));
            Assert.IsFalse(string.IsNullOrEmpty(tokens.RefreshToken));
        }

        [TestMethod]
        public async Task Login_InvalidEmail_ReturnsBadRequest()
        {
            // Arrange
            var loginDto = new LoginDto("wrong@example.com", "Password123!");

            _mockUserManager.Setup(x => x.FindByEmailAsync(loginDto.Email))
                .ReturnsAsync((User)null);

            // Act
            var result = await _controller.LoginAsync(loginDto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequestResult = result as BadRequestObjectResult;
            var errors = badRequestResult.Value as IEnumerable<string>;
            Assert.IsTrue(errors.Any(e => e.Contains("Invalid email or password")));
        }

        [TestMethod]
        public async Task Login_InvalidPassword_ReturnsBadRequest()
        {
            // Arrange
            var loginDto = new LoginDto("test@example.com", "wrongpassword");
            var user = new User("testuser", loginDto.Email) { Id = "123" };

            _mockUserManager.Setup(x => x.FindByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);

            _mockUserManager.Setup(x => x.CheckPasswordAsync(user, loginDto.Password))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.LoginAsync(loginDto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequestResult = result as BadRequestObjectResult;
            var errors = badRequestResult.Value as IEnumerable<string>;
            Assert.IsTrue(errors.Any(e => e.Contains("Invalid email or password")));
        }

        //[TestMethod]
        //public async Task RefreshToken_ValidToken_ReturnsNewTokens()
        //{
        //    // Arrange
        //    var refreshToken = "valid_refresh_token";
        //    var user = new User("TestUser", "test@example.com")
        //    {
        //        Id = "123",
        //        RefreshToken = refreshToken,
        //        RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1)
        //    };

        //    _mockUserManager.Setup(x => x.Users)
        //        .Returns(new List<User> { user }.AsQueryable().BuildMockDbSet().Object);

        //    _mockUserManager.Setup(x => x.UpdateAsync(user))
        //        .ReturnsAsync(IdentityResult.Success);

        //    // Act
        //    var result = await _userService.RefreshTokenAsync(refreshToken);

        //    // Assert
        //    Assert.IsTrue(result.Succeeded);
        //    Assert.IsNotNull(result.Data);

        //    var tokens = result.Data;
        //    Assert.IsFalse(string.IsNullOrEmpty(tokens.AccessToken));
        //    Assert.IsFalse(string.IsNullOrEmpty(tokens.RefreshToken));
        //    Assert.AreNotEqual(refreshToken, tokens.RefreshToken); // Nowy refresh token powinien być inny
        //}
    }
}