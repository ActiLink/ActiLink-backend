using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ActiLink.Controllers;
using ActiLink.DTOs;
using ActiLink.Model;
using ActiLink.Services;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Security.Claims;

namespace ActiLink.UnitTests.EventTests
{
    [TestClass]
    public class EventsControllerTests
    {
        private Mock<ILogger<EventsController>> _loggerMock = null!;
        private Mock<IMapper> _mapperMock = null!;
        private Mock<IEventService> _eventServiceMock = null!;
        private EventsController _controller = null!;

        [TestInitialize]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<EventsController>>();
            _mapperMock = new Mock<IMapper>();
            _eventServiceMock = new Mock<IEventService>();

            var controllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _controller = new EventsController(_loggerMock.Object, _eventServiceMock.Object, _mapperMock.Object)
            {
                ControllerContext = controllerContext
            };
        }

        [TestMethod]
        public async Task CreateEventAsync_Success_ReturnsCreatedAtAction()
        {
            // Given
            var userId = "TestUserId";
            var eventId = new Guid("030B4A82-1B7C-11CF-9D53-00AA003C9CB6");
            var startTime = new DateTime(1410, 7, 15);
            var endTime = startTime.AddHours(24);
            var location = new Location(53.483413, 20.095220);
            var price = 0m;
            var minUsers = 25_000;
            var maxUsers = 30_000;
            var hobbyIds = new List<Guid>();

            var newEventDto = new NewEventDto(startTime, endTime, location, price, minUsers, maxUsers, hobbyIds);
            var organizer = new User("TestUser", "test@example.com") { Id = userId };
            var createdEvent = new Event(organizer, startTime, endTime, location, price, minUsers, maxUsers);
            Utils.SetupEventGuid(createdEvent, eventId);
            var eventDto = new EventDto(eventId, userId, startTime, endTime, location, price, minUsers, maxUsers, [], []);

            var serviceResult = GenericServiceResult<Event>.Success(createdEvent);

            _eventServiceMock
                .Setup(es => es.CreateEventAsync(It.IsAny<CreateEventObject>()))
                .ReturnsAsync(serviceResult);

            _mapperMock
                .Setup(m => m.Map<EventDto>(It.IsAny<Event>()))
                .Returns(eventDto);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext.HttpContext.User = principal;


            // When
            var actionResult = await _controller.CreateEventAsync(newEventDto);

            // Then
            Assert.IsInstanceOfType<CreatedAtActionResult>(actionResult);
            var createdAtActionResult = (CreatedAtActionResult)actionResult;
            Assert.IsNotNull(createdAtActionResult);
            Assert.AreEqual(StatusCodes.Status201Created, createdAtActionResult.StatusCode);
            Assert.AreEqual(nameof(_controller.GetEventByIdAsync), createdAtActionResult.ActionName);
            Assert.IsNotNull(createdAtActionResult.Value as EventDto);
            Assert.AreEqual(eventDto, createdAtActionResult.Value);
            Assert.IsNotNull(createdAtActionResult.RouteValues);
            Assert.AreEqual(eventId, createdAtActionResult.RouteValues["id"]);
        }

        [TestMethod]
        public async Task CreateEventAsync_Failure_ReturnsBadRequest()
        {
            // Given
            var userId = "TestUserId";
            var eventId = new Guid("030B4A82-1B7C-11CF-9D53-00AA003C9CB6");
            var startTime = new DateTime(1410, 7, 15);
            var endTime = startTime.AddHours(24);
            var location = new Location(53.483413, 20.095220);
            var price = 0m;
            var minUsers = 25_000;
            var maxUsers = 30_000;
            var hobbyIds = new List<Guid>();

            var newEventDto = new NewEventDto( startTime, endTime, location, price, minUsers, maxUsers, hobbyIds);

            var errors = new List<string> { "Failed to create event" };
            var serviceResult = GenericServiceResult<Event>.Failure(errors);

            _eventServiceMock
                .Setup(es => es.CreateEventAsync(It.IsAny<CreateEventObject>()))
                .ReturnsAsync(serviceResult);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext.HttpContext.User = principal;


            // When
            var actionResult = await _controller.CreateEventAsync(newEventDto);

            // Then
            Assert.IsInstanceOfType<BadRequestObjectResult>(actionResult);
            var badRequestResult = (BadRequestObjectResult)actionResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
            Assert.IsNotNull(badRequestResult.Value);
            Assert.IsInstanceOfType<List<string>>(badRequestResult.Value);
            var errorsResult = (List<string>)badRequestResult.Value;
            Assert.IsNotNull(errorsResult);
            Assert.AreEqual(errors.Count, errorsResult.Count);
            Assert.AreEqual(errors[0], errorsResult[0]);
        }

        [TestMethod]
        public async Task GetEventByIdAsync_EventFound_ReturnsOkObjectResult()
        {
            // Given
            var userId = "TestUserId";
            var eventId = new Guid("030B4A82-1B7C-11CF-9D53-00AA003C9CB6");
            var startTime = new DateTime(1410, 7, 15);
            var endTime = startTime.AddHours(24);
            var location = new Location(53.483413, 20.095220);
            var price = 0m;
            var minUsers = 25_000;
            var maxUsers = 30_000;

            var organizer = new User("TestUser", "test@example.com") { Id = userId };
            var foundEvent = new Event(organizer, startTime, endTime, location, price, minUsers, maxUsers);
            Utils.SetupEventGuid(foundEvent, eventId);
            var expectedEventDto = new EventDto(eventId, userId, startTime, endTime, location, price, minUsers, maxUsers, [], []);

            _eventServiceMock
                .Setup(es => es.GetEventByIdAsync(eventId))
                .ReturnsAsync(foundEvent);

            _mapperMock
                .Setup(m => m.Map<EventDto>(foundEvent))
                .Returns(expectedEventDto);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext.HttpContext.User = principal;

            // When
            var actionResult = await _controller.GetEventByIdAsync(eventId);

            // Then
            Assert.IsInstanceOfType<OkObjectResult>(actionResult);
            var okResult = (OkObjectResult)actionResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
            Assert.IsNotNull(okResult.Value);
            Assert.IsInstanceOfType<EventDto>(okResult.Value);
            var eventDtoResult = (EventDto)okResult.Value;
            Assert.IsNotNull(eventDtoResult);
            Assert.AreEqual(expectedEventDto, eventDtoResult);
        }

        [TestMethod]
        public async Task GetEventByIdAsync_EventNotFound_ReturnsNotFoundResult()
        {
            // Given
            var eventId = new Guid("030B4A82-1B7C-11CF-9D53-00AA003C9CB6");
            _eventServiceMock
                .Setup(es => es.GetEventByIdAsync(eventId))
                .ReturnsAsync((Event?)null);

            // When
            var actionResult = await _controller.GetEventByIdAsync(eventId);

            // Then
            Assert.IsInstanceOfType<NotFoundResult>(actionResult);
            var notFoundResult = (NotFoundResult)actionResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        }

        [TestMethod]
        public async Task GetAllEventsAsync_ReturnsIEnumerableOfEventDto()
        {
            // Given
            var userId = "TestUserId";
            var eventId1 = new Guid("030B4A82-1B7C-11CF-9D53-00AA003C9CB6");
            var eventId2 = new Guid("030B4A82-1B7C-11CF-9D53-00AA003C9CB2");

            var startTime = new DateTime(2024, 5, 24);
            var endTime = startTime.AddHours(2);

            var location = new Location(0, 0);
            var price = 50m;
            var minUsers = 1;
            var maxUsers = 100;

            var ogranizer = new User("TestUser", "test@example.com") { Id = userId };

            var existingEvent1 = new Event(ogranizer, startTime, endTime, location, price, minUsers, maxUsers);
            var existingEvent2 = new Event(ogranizer, startTime, endTime, location, price, minUsers, maxUsers);
            Utils.SetupEventGuid(existingEvent1, eventId1);
            Utils.SetupEventGuid(existingEvent2, eventId2);

            var expectedEventDto1 = new EventDto(eventId1, userId, startTime, endTime, location, price, minUsers, maxUsers, [], []);
            var expectedEventDto2 = new EventDto(eventId2, userId, startTime, endTime, location, price, minUsers, maxUsers, [], []);

            _eventServiceMock
                .Setup(es => es.GetAllEventsAsync())
                .ReturnsAsync([existingEvent1, existingEvent2]);

            _mapperMock
                .Setup(m => m.Map<IEnumerable<EventDto>>(It.IsAny<IEnumerable<Event>>()))
                .Returns([expectedEventDto1, expectedEventDto2]);


            // When
            var resultEvents = await _controller.GetAllEventsAsync();

            // Then
            Assert.IsInstanceOfType<IEnumerable<EventDto>>(resultEvents);
            Assert.IsNotNull(resultEvents);
            Assert.AreEqual(2, resultEvents.Count());
            Assert.IsTrue(resultEvents.Any(e => e.Id == eventId1));
            Assert.IsTrue(resultEvents.Any(e => e.Id == eventId2));
            Assert.IsTrue(resultEvents.All(e => e.OrganizerId == userId));
            Assert.IsTrue(resultEvents.All(e => e.StartTime == startTime));
            Assert.IsTrue(resultEvents.All(e => e.EndTime == endTime));
            Assert.IsTrue(resultEvents.All(e => e.Location == location));
            Assert.IsTrue(resultEvents.All(e => e.Price == price));
            Assert.IsTrue(resultEvents.All(e => e.MinUsers == minUsers));
            Assert.IsTrue(resultEvents.All(e => e.MaxUsers == maxUsers));
            Assert.IsTrue(resultEvents.All(e => e.Participants.Count == 0));
            Assert.IsTrue(resultEvents.All(e => e.Hobbies.Count == 0));
        }

        [TestMethod]
        public async Task GetAllEventsAsync_NoEvents_ReturnsEmptyIEnumerableOfEventDto()
        {
            // Given
            _eventServiceMock
                .Setup(es => es.GetAllEventsAsync())
                .ReturnsAsync([]);

            // When
            var resultEvents = await _controller.GetAllEventsAsync();

            // Then
            Assert.IsInstanceOfType<IEnumerable<EventDto>>(resultEvents);
            Assert.IsNotNull(resultEvents);
            Assert.IsFalse(resultEvents.Any());
        }
    }
}
