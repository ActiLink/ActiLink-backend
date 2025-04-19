using ActiLink.Organizers;
using ActiLink.Organizers.BusinessClients;
using ActiLink.Organizers.BusinessClients.DTOs;
using ActiLink.Organizers.BusinessClients.Service;
using ActiLink.Shared.ServiceUtils;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace ActiLink.UnitTests.BusinessClientTests
{
    [TestClass]
    public class BusinessClientsControllerTests
    {
        private Mock<UserManager<Organizer>> _mockUserManager = null!;
        private Mock<IBusinessClientService> _mockBusinessClientService = null!;
        private Mock<IMapper> _mockMapper = null!;
        private BusinessClientsController _businessClientsController = null!;

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
            _mockBusinessClientService = new Mock<IBusinessClientService>();
            _mockMapper = new Mock<IMapper>();

            // Initialize the BusinessClientsController with mocked dependencies
            _businessClientsController = new BusinessClientsController(Mock.Of<ILogger<BusinessClientsController>>(), _mockBusinessClientService.Object, _mockMapper.Object);
        }

        [TestMethod]
        public async Task CreateBusinessClient_ValidBusinessClient_ReturnsCreatedAtActionResult()
        {
            // Given
            var newBusinessClientDto = new NewBusinessClientDto(username, email, password, taxId);
            var businessClient = new BusinessClient(username, email, taxId) { Id = id };
            var expectedBusinessClientDto = new BusinessClientDto(id, username, email, taxId);

            _mockBusinessClientService.Setup(x => x.CreateBusinessClientAsync(username, email, password, taxId))
                .ReturnsAsync(GenericServiceResult<BusinessClient>.Success(businessClient));

            _mockMapper.Setup(m => m.Map<BusinessClientDto>(businessClient))
                .Returns(expectedBusinessClientDto);


            // When
            var result = await _businessClientsController.CreateBusinessClientAsync(newBusinessClientDto);

            // Then
            var createdAtActionResult = result as CreatedAtActionResult;
            Assert.IsNotNull(createdAtActionResult, "Expected CreatedAtActionResult");
            Assert.AreEqual(nameof(BusinessClientsController.GetBusinessClientByIdAsync), createdAtActionResult.ActionName);

            var businessClientDto = createdAtActionResult.Value as BusinessClientDto;
            Assert.IsNotNull(businessClientDto, "Expected BusinessClientDto");

            Assert.IsNotNull(createdAtActionResult.RouteValues);
            Assert.AreEqual(expectedBusinessClientDto, businessClientDto);
            Assert.AreEqual(expectedBusinessClientDto.Id, createdAtActionResult.RouteValues["id"]);
            _mockBusinessClientService.Verify(x => x.CreateBusinessClientAsync(username, email, password, taxId), Times.Once);
            _mockMapper.Verify(m => m.Map<BusinessClientDto>(businessClient), Times.Once);
        }

        [TestMethod]
        public async Task CreateBusinessClient_InvalidBusinessClient_ReturnsBadRequest()
        {
            // Given
            var newBusinessClientDto = new NewBusinessClientDto(username, email, password, taxId);
            var errors = new[] { "Invalid email", "Password too short" };

            _mockBusinessClientService.Setup(x => x.CreateBusinessClientAsync(username, email, password, taxId))
                .ReturnsAsync(GenericServiceResult<BusinessClient>.Failure(errors));

            // When
            var result = await _businessClientsController.CreateBusinessClientAsync(newBusinessClientDto);

            // Then
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult, "Expected BadRequestObjectResult");
            Assert.IsNotNull(badRequestResult.Value);
            Assert.AreEqual(errors, badRequestResult.Value);
        }

        [TestMethod]
        public async Task CreateBusinessClient_ExceptionThrown_ReturnsInternalServerError()
        {
            // Given
            var newBusinessClientDto = new NewBusinessClientDto(username, email, password, taxId);
            var exceptionMessage = "An unexpected error occurred";
            _mockBusinessClientService.Setup(x => x.CreateBusinessClientAsync(username, email, password, taxId))
                .ThrowsAsync(new Exception(exceptionMessage));

            // When
            var result = await _businessClientsController.CreateBusinessClientAsync(newBusinessClientDto);

            // Then
            var internalServerErrorResult = result as ObjectResult;
            Assert.IsNotNull(internalServerErrorResult, "Expected ObjectResult");
            Assert.AreEqual(StatusCodes.Status500InternalServerError, internalServerErrorResult.StatusCode);
            Assert.AreEqual(exceptionMessage, internalServerErrorResult.Value);
        }

        [TestMethod]
        public async Task GetBusinessClientByIdAsync_ValidId_ReturnsOkResult()
        {
            // Given
            var businessClient = new BusinessClient(username, email, taxId) { Id = id };
            var expectedBusinessClientDto = new BusinessClientDto(id, username, email, taxId);

            _mockBusinessClientService.Setup(x => x.GetBusinessClientByIdAsync(id))
                .ReturnsAsync(businessClient);

            _mockMapper.Setup(m => m.Map<BusinessClientDto>(businessClient))
                .Returns(expectedBusinessClientDto);


            // When
            var result = await _businessClientsController.GetBusinessClientByIdAsync(id);

            // Then
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult, "Expected OkObjectResult");
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
            var businessClientDto = okResult.Value as BusinessClientDto;
            Assert.IsNotNull(businessClientDto, "Expected BusinessClientDto");
            Assert.AreEqual(expectedBusinessClientDto, businessClientDto);
        }

        [TestMethod]
        public async Task GetBusinessClientByIdAsync_InvalidId_ReturnsNotFoundResult()
        {
            // Given
            var invalidId = "invalid-id";
            _mockBusinessClientService.Setup(x => x.GetBusinessClientByIdAsync(invalidId))
                .ReturnsAsync((BusinessClient?)null);

            // When
            var result = await _businessClientsController.GetBusinessClientByIdAsync(invalidId);

            // Then
            var notFoundResult = result as NotFoundResult;
            Assert.IsNotNull(notFoundResult, "Expected NotFoundResult");
            Assert.AreEqual(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        }

        [TestMethod]
        public async Task GetBusinessClientsAsync_ReturnsOkResult()
        {
            // Given
            var businessClients = new List<BusinessClient>
            {
                new(username, email, taxId),
                new(username, email, taxId)
            };

            var expectedBusinessClientDtos = new List<BusinessClientDto>
            {
                new(id, username, email, taxId),
                new(id, username, email, taxId)
            };

            _mockBusinessClientService.Setup(x => x.GetBusinessClientsAsync())
                .ReturnsAsync(businessClients);

            _mockMapper.Setup(m => m.Map<IEnumerable<BusinessClientDto>>(businessClients))
                .Returns(expectedBusinessClientDtos);

            // When
            var result = await _businessClientsController.GetBusinessClientsAsync();

            // Then
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult, "Expected OkObjectResult");
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
            var businessClientDtos = okResult.Value as IEnumerable<BusinessClientDto>;
            Assert.IsNotNull(businessClientDtos, "Expected IEnumerable<BusinessClientDto>");
            Assert.AreEqual(expectedBusinessClientDtos, businessClientDtos);
        }

        [TestMethod]
        public async Task GetBusinessClientsAsync_ReturnsOkResultWithEmptyEnumerable()
        {
            // Given
            var businessClients = new List<BusinessClient>();
            var expectedBusinessClientDtos = new List<BusinessClientDto>();

            _mockBusinessClientService.Setup(x => x.GetBusinessClientsAsync())
                .ReturnsAsync(businessClients);

            _mockMapper.Setup(m => m.Map<IEnumerable<BusinessClientDto>>(businessClients))
                .Returns(expectedBusinessClientDtos);

            // When
            var result = await _businessClientsController.GetBusinessClientsAsync();

            // Then
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult, "Expected OkObjectResult");
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
            var businessClientDtos = okResult.Value as IEnumerable<BusinessClientDto>;
            Assert.IsNotNull(businessClientDtos, "Expected IEnumerable<BusinessClientDto>");
            Assert.AreEqual(0, businessClientDtos.Count());
        }
    }
}
