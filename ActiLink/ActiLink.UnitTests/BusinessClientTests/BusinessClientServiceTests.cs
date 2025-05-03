using ActiLink.Configuration;
using ActiLink.Organizers;
using ActiLink.Organizers.Authentication;
using ActiLink.Organizers.Authentication.Tokens;
using ActiLink.Organizers.BusinessClients;
using ActiLink.Organizers.BusinessClients.Service;
using ActiLink.Shared.Repositories;
using ActiLink.Shared.ServiceUtils;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;

namespace ActiLink.UnitTests.BusinessClientTests
{
    [TestClass]
    public class BusinessClientServiceTests
    {
        private Mock<UserManager<Organizer>> _mockUserManager = null!;
        private Mock<IUnitOfWork> _mockUnitOfWork = null!;
        private Mock<IJwtTokenProvider> _mockTokenProvider = null!;
        private BusinessClientService _businessClientService = null!;

        private const string username = "testuser";
        private const string email = "testuser@email.com";
        private const string password = "TestPassword123!";
        private const string taxId = "106-00-00-062";
        private const string id = "030B4A82-1B7C-11CF-9D53-00AA003C9CB6";
        private const string accessToken = "test.access.token";
        private const string refreshToken = "test.refresh.token";

        [TestInitialize]
        public void Setup()
        {
            // Mocking dependencies for BusinessClientService
            var store = new Mock<IUserStore<Organizer>>();
            _mockUserManager = new Mock<UserManager<Organizer>>(
                store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockTokenProvider = new Mock<IJwtTokenProvider>();
            _mockTokenProvider
                .Setup(t => t.GenerateAccessToken(It.IsAny<Organizer>()))
                .Returns(accessToken);
            _mockTokenProvider
                .Setup(t => t.GenerateRefreshToken(It.IsAny<string>()))
                .Returns(refreshToken);

            var jwtSettings = new JwtSettings
            {
                AccessTokenExpiryMinutes = 60,
                RefreshTokenExpiryDays = 30,
                Roles = new JwtSettings.RoleNames
                {
                    UserRole = "User",
                    BusinessClientRole = "BusinessClient"
                }
            };
            var jwtOptions = Options.Create(jwtSettings);

            // Initialize the BusinessClientService with mocked dependencies
            _businessClientService = new BusinessClientService(_mockUnitOfWork.Object, _mockUserManager.Object, _mockTokenProvider.Object, jwtOptions);
        }

        [TestMethod]
        public async Task Register_ValidBusinessClient_ReturnsGenericServiceResultSuccess()
        {
            // Given
            var businessClient = new BusinessClient(username, email, taxId);

            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<BusinessClient>(), password))
                .ReturnsAsync(IdentityResult.Success);

            _mockUserManager.Setup(x => x.FindByIdAsync(id))
                .ReturnsAsync(businessClient);

            // When
            var result = await _businessClientService.CreateBusinessClientAsync(username, email, password, taxId);

            // Then
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(0, result.Errors.Count());
            Assert.AreEqual(Shared.ServiceUtils.ErrorCode.None, result.ErrorCode);
            Assert.IsNotNull(result.Data);
            Assert.AreEqual(username, result.Data.UserName);
            Assert.AreEqual(email, result.Data.Email);
            Assert.AreEqual(taxId, result.Data.TaxId);

            _mockUserManager.Verify(x => x.CreateAsync(It.IsAny<BusinessClient>(), password), Times.Once);
        }

        [TestMethod]
        public async Task Register_BusinessClientWithExistingUsername_ReturnsGenericServiceResultFailure()
        {
            // Given
            var businessClient = new BusinessClient(username, email, taxId);

            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<BusinessClient>(), password))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Username already exists." }));

            // When
            var result = await _businessClientService.CreateBusinessClientAsync(username, email, password, taxId);

            // Then
            Assert.IsFalse(result.Succeeded);
            Assert.IsNull(result.Data);
            Assert.AreEqual(1, result.Errors.Count());
            Assert.AreEqual(Shared.ServiceUtils.ErrorCode.GeneralError, result.ErrorCode);
            Assert.AreEqual("Username already exists.", result.Errors.First());
            _mockUserManager.Verify(x => x.CreateAsync(It.IsAny<BusinessClient>(), password), Times.Once);
        }

        [TestMethod]
        public async Task Register_BusinessClientWithExistingEmail_ReturnsGenericServiceResultFailure()
        {
            // Given
            var businessClient = new BusinessClient(username, email, taxId);

            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<BusinessClient>(), password))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Email already exists." }));

            // When
            var result = await _businessClientService.CreateBusinessClientAsync(username, email, password, taxId);

            // Then
            Assert.IsFalse(result.Succeeded);
            Assert.IsNull(result.Data);
            Assert.AreEqual(1, result.Errors.Count());
            Assert.AreEqual(Shared.ServiceUtils.ErrorCode.GeneralError, result.ErrorCode);
            Assert.AreEqual("Email already exists.", result.Errors.First());
            _mockUserManager.Verify(x => x.CreateAsync(It.IsAny<BusinessClient>(), password), Times.Once);
        }

        [TestMethod]
        public async Task LoginAsync_ValidCredentials_ReturnsTokens()
        {
            // Given
            var businessClient = new BusinessClient(username, email, taxId) { Id = id };

            _mockUserManager.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync(businessClient);

            _mockUserManager.Setup(x => x.CheckPasswordAsync(businessClient, password))
                .ReturnsAsync(true);

            var mockRefreshTokenRepo = new Mock<IRepository<RefreshToken>>();
            mockRefreshTokenRepo.Setup(r => r.AddAsync(It.IsAny<RefreshToken>()))
                .Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.RefreshTokenRepository)
                .Returns(mockRefreshTokenRepo.Object);

            _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // When
            var result = await _businessClientService.LoginAsync(email, password);

            // Then
            Assert.IsTrue(result.Succeeded);
            Assert.IsNotNull(result.Data);
            Assert.AreEqual(accessToken, result.Data.AccessToken);
            Assert.AreEqual(refreshToken, result.Data.RefreshToken);
            Assert.AreEqual(ErrorCode.None, result.ErrorCode);

            _mockUserManager.Verify(x => x.FindByEmailAsync(email), Times.Once);
            _mockUserManager.Verify(x => x.CheckPasswordAsync(businessClient, password), Times.Once);
            _mockTokenProvider.Verify(t => t.GenerateAccessToken(businessClient), Times.Once);
            _mockTokenProvider.Verify(t => t.GenerateRefreshToken(id), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [TestMethod]
        public async Task LoginAsync_InvalidEmail_ReturnsFailure()
        {
            // Given
            _mockUserManager.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync((BusinessClient?)null);

            // When
            var result = await _businessClientService.LoginAsync(email, password);

            // Then
            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual(1, result.Errors.Count());
            Assert.AreEqual("Invalid email or password.", result.Errors.First());
            Assert.AreEqual(ErrorCode.GeneralError, result.ErrorCode);

            _mockUserManager.Verify(x => x.FindByEmailAsync(email), Times.Once);
            _mockUserManager.Verify(x => x.CheckPasswordAsync(It.IsAny<Organizer>(), It.IsAny<string>()), Times.Never);
            _mockTokenProvider.Verify(t => t.GenerateAccessToken(It.IsAny<Organizer>()), Times.Never);
        }

        [TestMethod]
        public async Task LoginAsync_InvalidPassword_ReturnsFailure()
        {
            // Given
            var businessClient = new BusinessClient(username, email, taxId) { Id = id };

            _mockUserManager.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync(businessClient);

            _mockUserManager.Setup(x => x.CheckPasswordAsync(businessClient, password))
                .ReturnsAsync(false);

            var mockRefreshTokenRepo = new Mock<IRepository<RefreshToken>>();
            mockRefreshTokenRepo.Setup(r => r.AddAsync(It.IsAny<RefreshToken>()))
                .Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.RefreshTokenRepository)
                .Returns(mockRefreshTokenRepo.Object);

            // When
            var result = await _businessClientService.LoginAsync(email, password);

            // Then
            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual(1, result.Errors.Count());
            Assert.AreEqual("Invalid email or password.", result.Errors.First());
            Assert.AreEqual(ErrorCode.GeneralError, result.ErrorCode);

            _mockUserManager.Verify(x => x.FindByEmailAsync(email), Times.Once);
            _mockUserManager.Verify(x => x.CheckPasswordAsync(businessClient, password), Times.Once);
            _mockTokenProvider.Verify(t => t.GenerateAccessToken(It.IsAny<Organizer>()), Times.Never);
        }

        [TestMethod]
        public async Task LoginAsync_FailedSaveChanges_ReturnsFailure()
        {
            // Given
            var businessClient = new BusinessClient(username, email, taxId) { Id = id };

            _mockUserManager.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync(businessClient);

            _mockUserManager.Setup(x => x.CheckPasswordAsync(businessClient, password))
                .ReturnsAsync(true);

            var mockRefreshTokenRepo = new Mock<IRepository<RefreshToken>>();
            mockRefreshTokenRepo.Setup(r => r.AddAsync(It.IsAny<RefreshToken>()))
                .Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.RefreshTokenRepository)
                .Returns(mockRefreshTokenRepo.Object);

            _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(0);

            // When
            var result = await _businessClientService.LoginAsync(email, password);

            // Then
            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual(1, result.Errors.Count());
            Assert.AreEqual("Failed to save the refresh token.", result.Errors.First());
            Assert.AreEqual(ErrorCode.GeneralError, result.ErrorCode);

            _mockUserManager.Verify(x => x.FindByEmailAsync(email), Times.Once);
            _mockUserManager.Verify(x => x.CheckPasswordAsync(businessClient, password), Times.Once);
            _mockTokenProvider.Verify(t => t.GenerateAccessToken(businessClient), Times.Once);
            _mockTokenProvider.Verify(t => t.GenerateRefreshToken(id), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [TestMethod]
        public async Task GetBusinessClientById_ValidId_ReturnsBusinessClient()
        {
            // Given
            var businessClient = new BusinessClient(username, email, taxId) { Id = id };
            _mockUserManager.Setup(x => x.FindByIdAsync(id))
                .ReturnsAsync(businessClient);

            // When
            var result = await _businessClientService.GetBusinessClientByIdAsync(id);

            // Then
            Assert.IsNotNull(result);
            Assert.AreEqual(businessClient, result);
            Assert.AreEqual(id, result.Id);
            Assert.AreEqual(username, result.UserName);
            Assert.AreEqual(email, result.Email);
            Assert.AreEqual(taxId, result.TaxId);
            _mockUserManager.Verify(x => x.FindByIdAsync(id), Times.Once);
        }

        [TestMethod]
        public async Task GetBusinessClientById_InvalidId_ReturnsNull()
        {
            // Given
            _mockUserManager.Setup(x => x.FindByIdAsync(id))
                .ReturnsAsync((BusinessClient?)null);

            // When
            var result = await _businessClientService.GetBusinessClientByIdAsync(id);

            // Then
            Assert.IsNull(result);
            _mockUserManager.Verify(x => x.FindByIdAsync(id), Times.Once);
        }

        [TestMethod]
        public async Task GetAllBusinessClients_ReturnsNonEmptyCollection()
        {
            // Given
            var businessClient1 = new BusinessClient(username, email, taxId);
            var businessClient2 = new BusinessClient(username, email, taxId);
            var businessClients = new List<BusinessClient> { businessClient1, businessClient2 };

            _mockUnitOfWork.Setup(x => x.BusinessClientRepository.GetAllAsync())
                .ReturnsAsync(businessClients);

            // When
            var result = await _businessClientService.GetBusinessClientsAsync();

            // Then
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual(businessClient1, result.First());
            Assert.AreEqual(businessClient2, result.Last());
            _mockUnitOfWork.Verify(x => x.BusinessClientRepository.GetAllAsync(), Times.Once);
        }

        [TestMethod]
        public async Task GetAllBusinessClients_EmptyCollection_ReturnsEmptyCollection()
        {
            // Given
            _mockUnitOfWork.Setup(x => x.BusinessClientRepository.GetAllAsync())
                .ReturnsAsync([]);

            // When
            var result = await _businessClientService.GetBusinessClientsAsync();

            // Then
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
            _mockUnitOfWork.Verify(x => x.BusinessClientRepository.GetAllAsync(), Times.Once);
        }

        [TestMethod]
        public async Task DeleteBusinessClient_ValidId_ReturnsTrue()
        {
            // Given
            var businessClient = new BusinessClient(username, email, taxId);
            _mockUserManager.Setup(x => x.DeleteAsync(businessClient))
                .ReturnsAsync(IdentityResult.Success);

            // When
            var result = await _businessClientService.DeleteBusinessClientAsync(businessClient);

            // Then
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(0, result.Errors.Count());
            Assert.AreEqual(result.ErrorCode, Shared.ServiceUtils.ErrorCode.None);
            _mockUserManager.Verify(x => x.DeleteAsync(businessClient), Times.Once);
        }

        public async Task UpdateBusinessClient_ValidData_ReturnsSuccess()
        {
            // Given
            var businessClient = new BusinessClient(username, email, taxId) { Id = id };
            var updatedName = "updateduser";
            var updatedEmail = "updated@email.com";
            var updatedTaxId = "123-45-67-890";
            var updateObject = new UpdateBusinessClientObject(updatedName, updatedEmail, updatedTaxId);

            _mockUserManager.Setup(m => m.FindByIdAsync(id))
                .ReturnsAsync(businessClient);

            _mockUserManager.Setup(m => m.SetUserNameAsync(businessClient, updatedName))
                .ReturnsAsync(IdentityResult.Success);

            _mockUserManager.Setup(m => m.SetEmailAsync(businessClient, updatedEmail))
                .ReturnsAsync(IdentityResult.Success);

            _mockUserManager.Setup(m => m.UpdateAsync(businessClient))
                .ReturnsAsync(IdentityResult.Success);

            // When
            var result = await _businessClientService.UpdateBusinessClientAsync(id, updateObject);

            // Then
            Assert.IsTrue(result.Succeeded);
            Assert.IsNotNull(result.Data);
            Assert.AreEqual(updatedName, businessClient.UserName);
            Assert.AreEqual(updatedEmail, businessClient.Email);
            Assert.AreEqual(updatedTaxId, businessClient.TaxId);
            Assert.AreEqual(ErrorCode.None, result.ErrorCode);

            _mockUserManager.Verify(m => m.FindByIdAsync(id), Times.Once);
            _mockUserManager.Verify(m => m.SetUserNameAsync(businessClient, updatedName), Times.Once);
            _mockUserManager.Verify(m => m.SetEmailAsync(businessClient, updatedEmail), Times.Once);
            _mockUserManager.Verify(m => m.UpdateAsync(businessClient), Times.Once);
        }


        [TestMethod]
        public async Task UpdateBusinessClient_UsernameUpdateFails_ReturnsValidationError()
        {
            // Given
            var businessClient = new BusinessClient(username, email, taxId) { Id = id };
            var updatedName = "updateduser";
            var updatedEmail = "updated@email.com";
            var updatedTaxId = "123-45-67-890";
            var updateObject = new UpdateBusinessClientObject(updatedName, updatedEmail, updatedTaxId);
            var identityErrors = new[] { new IdentityError { Description = "Nazwa użytkownika już istnieje" } };

            _mockUserManager.Setup(m => m.FindByIdAsync(id))
                .ReturnsAsync(businessClient);

            _mockUserManager.Setup(m => m.SetUserNameAsync(businessClient, updatedName))
                .ReturnsAsync(IdentityResult.Failed(identityErrors));

            // When
            var result = await _businessClientService.UpdateBusinessClientAsync(id, updateObject);

            // Then
            Assert.IsFalse(result.Succeeded);
            Assert.IsNull(result.Data);
            Assert.AreEqual(ErrorCode.ValidationError, result.ErrorCode);
            Assert.AreEqual("Nazwa użytkownika już istnieje", result.Errors.First());

            _mockUserManager.Verify(m => m.FindByIdAsync(id), Times.Once);
            _mockUserManager.Verify(m => m.SetUserNameAsync(businessClient, updatedName), Times.Once);
            _mockUserManager.Verify(m => m.SetEmailAsync(It.IsAny<BusinessClient>(), It.IsAny<string>()), Times.Never);
            _mockUserManager.Verify(m => m.UpdateAsync(It.IsAny<BusinessClient>()), Times.Never);
        }

        [TestMethod]
        public async Task UpdateBusinessClient_EmailUpdateFails_ReturnsValidationError()
        {
            // Given
            var businessClient = new BusinessClient(username, email, taxId) { Id = id };
            var updatedName = "updateduser";
            var updatedEmail = "updated@email.com";
            var updatedTaxId = "123-45-67-890";
            var updateObject = new UpdateBusinessClientObject(updatedName, updatedEmail, updatedTaxId);
            var identityErrors = new[] { new IdentityError { Description = "Email jest już zajęty" } };

            _mockUserManager.Setup(m => m.FindByIdAsync(id))
                .ReturnsAsync(businessClient);

            _mockUserManager.Setup(m => m.SetUserNameAsync(businessClient, updatedName))
                .ReturnsAsync(IdentityResult.Success);

            _mockUserManager.Setup(m => m.SetEmailAsync(businessClient, updatedEmail))
                .ReturnsAsync(IdentityResult.Failed(identityErrors));

            // When
            var result = await _businessClientService.UpdateBusinessClientAsync(id, updateObject);

            // Then
            Assert.IsFalse(result.Succeeded);
            Assert.IsNull(result.Data);
            Assert.AreEqual(ErrorCode.ValidationError, result.ErrorCode);
            Assert.AreEqual("Email jest już zajęty", result.Errors.First());

            _mockUserManager.Verify(m => m.FindByIdAsync(id), Times.Once);
            _mockUserManager.Verify(m => m.SetUserNameAsync(businessClient, updatedName), Times.Once);
            _mockUserManager.Verify(m => m.SetEmailAsync(businessClient, updatedEmail), Times.Once);
            _mockUserManager.Verify(m => m.UpdateAsync(It.IsAny<BusinessClient>()), Times.Never);
        }

    }
}
