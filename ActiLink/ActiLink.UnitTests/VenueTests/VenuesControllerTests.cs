using System.Security.Claims;
using ActiLink.Organizers.BusinessClients;
using ActiLink.Organizers.BusinessClients.DTOs;
using ActiLink.Shared.Model;
using ActiLink.Shared.ServiceUtils;
using ActiLink.Venues;
using ActiLink.Venues.DTOs;
using ActiLink.Venues.Service;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace ActiLink.UnitTests.VenueTests
{
    [TestClass]
    public class VenuesControllerTests
    {
        private Mock<ILogger<VenuesController>> _loggerMock = null!;
        private Mock<IMapper> _mapperMock = null!;
        private Mock<IVenueService> _venueServiceMock = null!;
        private VenuesController _controller = null!;

        const string businessClientId = "TestBCId";
        readonly Guid venueId = new("030B4A82-1B7C-11CF-9D53-00AA003C9CB6");
        const string venueName = "Test Venue";
        const string venueDescription = "Test Description";
        const string venueAddress = "Test Address";
        readonly Location venueLocation = new(1.0, 2.0);

        [TestInitialize]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<VenuesController>>();
            _mapperMock = new Mock<IMapper>();
            _venueServiceMock = new Mock<IVenueService>();

            var controllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var authOptions = new AuthorizationOptions();
            authOptions.AddPolicy("BusinessClient", policy => policy.RequireRole("BusinessClient"));

            var authServiceMock = new Mock<IAuthorizationService>();

            authServiceMock
               .Setup(auth => auth.AuthorizeAsync(
                   It.Is<ClaimsPrincipal>(u => u.IsInRole("BusinessClient")),
                   It.IsAny<object>(),
                   It.Is<string>(p => p == "BusinessClient")))
               .ReturnsAsync(AuthorizationResult.Success());

            authServiceMock
               .Setup(auth => auth.AuthorizeAsync(
                   It.Is<ClaimsPrincipal>(u => !u.IsInRole("BusinessClient")),
                   It.IsAny<object>(),
                   It.Is<string>(p => p == "BusinessClient")))
               .ReturnsAsync(AuthorizationResult.Failed());

            var services = new ServiceCollection();
            services.AddSingleton(authServiceMock.Object);
            controllerContext.HttpContext.RequestServices = services.BuildServiceProvider();


            _controller = new VenuesController(_venueServiceMock.Object, _loggerMock.Object, _mapperMock.Object)
            {
                ControllerContext = controllerContext
            };
        }

        [TestMethod]
        public async Task CreateVenueAsyncByBusinessClient_ValidRequest_ReturnsCreatedResult()
        {
            // Given
            var owner = new BusinessClient("Test Owner", "testowner@email.com", "PL1234567890") { Id = businessClientId };
            var ownerDto = new VenueOwnerDto(owner.Id, owner.UserName!);
            var newVenueDto = new NewVenueDto(venueName, venueDescription, venueLocation, venueAddress);
            var createVenueObject = new CreateVenueObject(businessClientId, venueName, venueDescription, venueLocation, venueAddress);
            var venue = new Venue(owner, venueName, venueDescription, venueLocation, venueAddress);
            var expectedVenueDto = new VenueDto(venueId, venueName, venueDescription, venueLocation, venueAddress, ownerDto, []);
            Utils.SetupVenueId(venue, venueId);

            _mapperMock.Setup(m => m.Map(newVenueDto, It.IsAny<Action<IMappingOperationOptions<object, CreateVenueObject>>>()))
                .Returns(createVenueObject);

            _mapperMock.Setup(m => m.Map<VenueDto>(venue))
                .Returns(expectedVenueDto);

            _venueServiceMock.Setup(vs => vs.CreateVenueAsync(createVenueObject))
                .ReturnsAsync(GenericServiceResult<Venue>.Success(venue));

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, businessClientId),
                new(ClaimTypes.Role, "BusinessClient")
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext.HttpContext.User = principal;

            var authService = _controller.ControllerContext.HttpContext.RequestServices
                .GetRequiredService<IAuthorizationService>();

            // When
            var authResult = await authService.AuthorizeAsync(principal, null, "BusinessClient");
            var result = authResult.Succeeded ?
                await _controller.CreateVenueAsync(newVenueDto) :
                new ForbidResult();

            // Then
            Assert.IsNotNull(result);
            var createdAtActionResult = result as CreatedAtActionResult;
            Assert.IsNotNull(createdAtActionResult);
            Assert.AreEqual(StatusCodes.Status201Created, createdAtActionResult.StatusCode);
            Assert.AreEqual(nameof(_controller.GetVenueByIdAsync), createdAtActionResult.ActionName);
            Assert.IsNotNull(createdAtActionResult.Value);
            Assert.IsInstanceOfType<VenueDto>(createdAtActionResult.Value);
            Assert.AreEqual(expectedVenueDto, createdAtActionResult.Value);
            Assert.IsNotNull(createdAtActionResult.RouteValues);
            Assert.AreEqual(venueId, createdAtActionResult.RouteValues["id"]);
        }

        [TestMethod]
        public async Task CreateVenueAsyncByUser_InvalidRequest_ReturnsForbidden()
        {
            // Given
            var newVenueDto = new NewVenueDto(venueName, venueDescription, venueLocation, venueAddress);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, businessClientId),
                new(ClaimTypes.Role, "User")
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext.HttpContext.User = principal;

            var authService = _controller.ControllerContext.HttpContext.RequestServices
                .GetRequiredService<IAuthorizationService>();

            // When
            var authResult = await authService.AuthorizeAsync(principal, null, "BusinessClient");
            var result = authResult.Succeeded ?
                await _controller.CreateVenueAsync(newVenueDto) :
                new ForbidResult();

            // Then
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType<ForbidResult>(result);
        }
    }
}
