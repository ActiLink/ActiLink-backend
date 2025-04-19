using ActiLink.Organizers;
using ActiLink.Organizers.BusinessClients;
using ActiLink.Organizers.BusinessClients.Service;
using ActiLink.Shared.Repositories;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace ActiLink.UnitTests.BusinessClientTests
{
    [TestClass]
    public class BusinessClientServiceTests
    {
        private Mock<UserManager<Organizer>> _mockUserManager = null!;
        private Mock<IUnitOfWork> _mockUnitOfWork = null!;
        private BusinessClientService _businessClientService = null!;

        private const string username = "testuser";
        private const string email = "testuser@email.com";
        private const string password = "TestPassword123!";
        private const string taxId = "106-00-00-062";
        private const string id = "030B4A82-1B7C-11CF-9D53-00AA003C9CB6";

        [TestInitialize]
        public void Setup()
        {
            // Mocking dependencies for BusinessClientService
            var store = new Mock<IUserStore<Organizer>>();
            _mockUserManager = new Mock<UserManager<Organizer>>(
                store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            _mockUnitOfWork = new Mock<IUnitOfWork>();

            // Initialize the BusinessClientService with mocked dependencies
            _businessClientService = new BusinessClientService(_mockUnitOfWork.Object, _mockUserManager.Object);
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
    }
}
