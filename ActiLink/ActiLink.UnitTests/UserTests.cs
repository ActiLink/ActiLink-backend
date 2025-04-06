using ActiLink.Controllers;
using ActiLink.DTOs;
using ActiLink.Model;
using ActiLink.Repositories;
using ActiLink.Services;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace ActiLink.UnitTests
{
    [TestClass]
    public class UserAuthTests
    {
        private Mock<UserManager<Organizer>> _mockUserManager = null!;
        private Mock<IUnitOfWork> _mockUnitOfWork = null!;
        private Mock<ILogger<UsersController>> _mockLogger = null!;
        private Mock<IMapper> _mockMapper = null!;
        private UsersController _controller = null!;
        private UserService _userService = null!;
        private JwtTokenProvider _tokenGenerator = null!;

        [TestInitialize]
        public void Setup()
        {
            // Mockowanie zależności dla UserService
            var store = new Mock<IUserStore<Organizer>>();
            _mockUserManager = new Mock<UserManager<Organizer>>(
                store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            _mockUnitOfWork = new Mock<IUnitOfWork>();

            // Ustawienie zmiennych środowiskowych dla TokenGenerator
            Environment.SetEnvironmentVariable("JWT_SECRET_KEY", "test_secret_key_12345678901234567890");
            Environment.SetEnvironmentVariable("JWT_VALID_ISSUER", "TestIssuer");
            Environment.SetEnvironmentVariable("JWT_VALID_AUDIENCE", "TestAudience");

            // Inicjalizacja TokenGenerator z mockowaną konfiguracją
            _tokenGenerator = new JwtTokenProvider();

            // Tworzenie rzeczywistej instancji UserService z mockami
            _userService = new UserService(
                _mockUnitOfWork.Object,
                _mockUserManager.Object,
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
            Assert.IsInstanceOfType<CreatedAtActionResult>(result);
            var createdAtResult = (CreatedAtActionResult)result;
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
                .ReturnsAsync(IdentityResult.Failed([.. errors.Select(e => new IdentityError { Description = e })]));

            // Act
            var result = await _controller.CreateUserAsync(newUserDto);

            // Assert
            Assert.IsInstanceOfType<BadRequestObjectResult>(result);
            var badRequestResult = (BadRequestObjectResult)result;
            Assert.IsNotNull(badRequestResult.Value);

            if (badRequestResult.Value is IEnumerable<string> errorsResult)
            {
                Assert.IsTrue(errorsResult.Any(e => e.Contains("Invalid email") || e.Contains("Password too short")));
            }
            else
            {
                Assert.Fail("Expected IEnumerable<string> as result value");
            }
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

            var (AccessToken, RefreshToken) = result.Data;
            Assert.IsFalse(string.IsNullOrEmpty(AccessToken));
            Assert.IsFalse(string.IsNullOrEmpty(RefreshToken));
        }

        [TestMethod]
        public async Task Login_InvalidEmail_ReturnsBadRequest()
        {
            // Arrange
            var loginDto = new LoginDto("wrong@example.com", "Password123!");

            _mockUserManager.Setup(x => x.FindByEmailAsync(loginDto.Email))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _controller.LoginAsync(loginDto);

            // Assert
            Assert.IsInstanceOfType<BadRequestObjectResult>(result);
            var badRequestResult = (BadRequestObjectResult)result;

            if (badRequestResult.Value is IEnumerable<string> errors)
            {
                Assert.IsTrue(errors.Any(e => e.Contains("Invalid email or password")));
            }
            else
            {
                Assert.Fail("Expected IEnumerable<string> as result value");
            }
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
            Assert.IsInstanceOfType<BadRequestObjectResult>(result);
            var badRequestResult = (BadRequestObjectResult)result;

            if (badRequestResult.Value is IEnumerable<string> errors)
            {
                Assert.IsTrue(errors.Any(e => e.Contains("Invalid email or password")));
            }
            else
            {
                Assert.Fail("Expected IEnumerable<string> as result value");
            }
        }
    }
}