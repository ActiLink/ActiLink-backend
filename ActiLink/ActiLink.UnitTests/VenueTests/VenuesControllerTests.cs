using System.Security.Claims;
using ActiLink.Events;
using ActiLink.Events.DTOs;
using ActiLink.Organizers.BusinessClients;
using ActiLink.Organizers.BusinessClients.DTOs;
using ActiLink.Organizers.DTOs;
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

        [TestMethod]
        public async Task GetVenueByIdAsync_Success_ReturnsOkResult()
        {
            // Given
            var owner = new BusinessClient("Test Owner", "testowner@email.com", "PL1234567890") { Id = businessClientId };
            var ownerDto = new VenueOwnerDto(owner.Id, owner.UserName!);
            var organizerDto = new OrganizerDto(owner.Id, owner.UserName!);
            var venue = new Venue(owner, venueName, venueDescription, venueLocation, venueAddress);
            var expectedVenueDto = new VenueDto(venueId, venueName, venueDescription, venueLocation, venueAddress, ownerDto, []);
            Utils.SetupVenueId(venue, venueId);
            var events = new List<Event>
            {
                new(owner, "Test Event", "Test Description", new DateTime(2023, 10, 1), new DateTime(2023, 10, 2), venueLocation, 20m, 1, 10, [], venue),
                new(owner, "Test Event 2", "Test Description 2", new DateTime(2023, 10, 3), new DateTime(2023, 10, 4), venueLocation, 30m, 1, 10, [], venue)
            };
            var eventIds = new List<Guid>
            {
                new("030B4A82-1B7C-11CF-9D53-00AA003C9CB6"),
                new("030B4A82-1B7C-11CF-9D53-00AA003C9CB7")
            };

            for (int i = 0; i < events.Count; i++)
                Utils.SetupEventGuid(events[i], eventIds[i]);

            events.ForEach(venue.Events.Add);
            var expectedEventDtos = events.Select(e => new ReducedEventDto(e.Id, e.Title, organizerDto, e.StartTime)).ToList();
            expectedVenueDto.Events.AddRange(expectedEventDtos);

            _venueServiceMock.Setup(vs => vs.GetVenueByIdAsync(venueId))
                .ReturnsAsync(venue);

            _mapperMock.Setup(m => m.Map<VenueDto>(venue))
                .Returns(expectedVenueDto);

            _mapperMock.Setup(m => m.Map<IEnumerable<ReducedEventDto>>(events))
                .Returns(expectedEventDtos);

            // When
            var result = await _controller.GetVenueByIdAsync(venueId);

            // Then
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
            Assert.IsNotNull(okResult.Value);
            Assert.IsInstanceOfType<VenueDto>(okResult.Value);
            Assert.AreEqual(expectedVenueDto, okResult.Value);
        }


        [TestMethod]
        public async Task GetVenueByIdAsync_VenueNotFound_ReturnsNotFoundResult()
        {
            // Given
            var nonExistentVenueId = Guid.NewGuid();
            _venueServiceMock.Setup(vs => vs.GetVenueByIdAsync(nonExistentVenueId))
                .ReturnsAsync((Venue?)null);

            // When
            var result = await _controller.GetVenueByIdAsync(nonExistentVenueId);

            // Then
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType<NotFoundResult>(result);
        }

        [TestMethod]
        public async Task GetAllVenuesAsync_VenuesFound_ReturnsOkResultWithListWithVenues()
        {
            var owner = new BusinessClient("Test Owner", "testowner@email.com", "PL1234567890") { Id = businessClientId };
            var ownerDto = new VenueOwnerDto(owner.Id, owner.UserName!);
            var organizerDto = new OrganizerDto(owner.Id, owner.UserName!);
            var venue1 = new Venue(owner, "Test Venue 1", "Test Description 1", venueLocation, venueAddress);
            var venue2 = new Venue(owner, "Test Venue 2", "Test Description 2", venueLocation, venueAddress);
            var expectedVenueDto1 = new VenueDto(venueId, venue1.Name, venue1.Description, venueLocation, venueAddress, ownerDto, []);
            var expectedVenueDto2 = new VenueDto(venueId, venue2.Name, venue2.Description, venueLocation, venueAddress, ownerDto, []);

            var events = new List<Event>
            {
                new(owner, "Test Event", "Test Description", new DateTime(2023, 10, 1), new DateTime(2023, 10, 2), venueLocation, 20m, 1, 10, [], venue1),
                new(owner, "Test Event 2", "Test Description 2", new DateTime(2023, 10, 3), new DateTime(2023, 10, 4), venueLocation, 30m, 1, 10, [], venue2)
            };
            var eventIds = new List<Guid>
            {
                new("030B4A82-1B7C-11CF-9D53-00AA003C9CB6"),
                new("030B4A82-1B7C-11CF-9D53-00AA003C9CB7")
            };
            for (int i = 0; i < events.Count; i++)
                Utils.SetupEventGuid(events[i], eventIds[i]);

            venue1.Events.Add(events[0]);
            venue2.Events.Add(events[1]);

            var expectedEventDtos = events.Select(e => new ReducedEventDto(e.Id, e.Title, organizerDto, e.StartTime)).ToList();
            expectedVenueDto1.Events.Add(expectedEventDtos[0]);
            expectedVenueDto2.Events.Add(expectedEventDtos[1]);

            var venues = new List<Venue> { venue1, venue2 };
            var expectedVenuesDto = new List<VenueDto> { expectedVenueDto1, expectedVenueDto2 };

            _venueServiceMock.Setup(vs => vs.GetAllVenuesAsync())
                .ReturnsAsync(venues);

            _mapperMock.Setup(m => m.Map<IEnumerable<VenueDto>>(venues))
                .Returns(expectedVenuesDto);

            // When
            var result = await _controller.GetAllVenuesAsync();

            // Then
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
            Assert.IsNotNull(okResult.Value);
            var venueDtos = okResult.Value as IEnumerable<VenueDto>;
            Assert.IsNotNull(venueDtos);
            CollectionAssert.AreEqual(expectedVenuesDto, venueDtos.ToList());
        }

        [TestMethod]
        public async Task GetAllVenuesAsyns_NoVenuesFound_ReturnsOkResultWithEmptyList()
        {
            // Given
            _venueServiceMock.Setup(vs => vs.GetAllVenuesAsync())
                .ReturnsAsync([]);

            _mapperMock.Setup(m => m.Map<IEnumerable<VenueDto>>(Enumerable.Empty<Venue>()))
                .Returns([]);

            // When
            var result = await _controller.GetAllVenuesAsync();

            // Then
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
            Assert.IsNotNull(okResult.Value);
            var venueDtos = okResult.Value as IEnumerable<VenueDto>;
            Assert.IsNotNull(venueDtos);
            Assert.AreEqual(0, venueDtos.Count());
        }
    }
}
