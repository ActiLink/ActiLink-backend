using ActiLink.Extensions;
using ActiLink.Model;
using ActiLink.Repositories;
using ActiLink.Services;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using System.ComponentModel;


namespace ActiLink.UnitTests.EventTests
{
    [TestClass]
    public class EventServiceTests
    {
        private Mock<IUnitOfWork> _unitOfWorkMock = null!;
        private Mock<IMapper> _mapperMock = null!;
        private Mock<IRepository<Event>> _mockRepository = null!;
        private EventService _eventService = null!;

        [TestInitialize]
        public void Setup()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _mockRepository = new Mock<IRepository<Event>>();
            _eventService = new EventService(_unitOfWorkMock.Object, _mapperMock.Object);

        }

        [TestMethod]
        public async Task CreateEventAsync_Success_ReturnsSuccessResult()
        {
            // Given
            var userId = "TestUserId";
            var eventTitle = "Test Event";
            var eventDescription = "This is a test event.";
            var startTime = new DateTime(2024, 1, 1);
            var endTime = startTime.AddHours(2);
            var location = new Location(0, 0);
            var price = 50m;
            var minUsers = 1;
            var maxUsers = 100;
            var hobbyIds = new List<Guid>();

            var createEventObject = new CreateEventObject(userId, eventTitle, eventDescription, startTime, endTime, location, price, minUsers, maxUsers, hobbyIds);
            var organizer = new User("TestUser", "test@example.com") { Id = userId };
            var createdEvent = new Event(organizer, eventTitle, eventDescription, startTime, endTime, location, price, minUsers, maxUsers, []);

            // Setup mapper to map from CreateEventObject to Event.
            _mapperMock
                .Setup(m => m.Map(It.IsAny<CreateEventObject>(), It.IsAny<Action<IMappingOperationOptions<object, Event>>>()))
                .Returns(createdEvent);

            // Setup repository add and save.
            _unitOfWorkMock
                .Setup(u => u.UserRepository.GetByIdAsync(userId))
                .ReturnsAsync(organizer);
            _unitOfWorkMock
                .Setup(u => u.EventRepository.AddAsync(It.IsAny<Event>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock
                .Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // When
            var result = await _eventService.CreateEventAsync(createEventObject);

            // Then
            Assert.IsTrue(result.Succeeded);
            Assert.IsNotNull(result.Data);
            Assert.AreEqual(createdEvent, result.Data);
        }

        [TestMethod]
        public async Task GetEventByIdAsync_EventExists_ReturnsEvent()
        {
            // Given
            var userId = "TestUserId";
            var eventId = new Guid("030B4A82-1B7C-11CF-9D53-00AA003C9CB6");
            var eventTitle = "Test Event";
            var eventDescription = "This is a test event.";
            var startTime = new DateTime(2024, 2, 6);
            var endTime = startTime.AddHours(3);
            var location = new Location(0, 0);
            var price = 50m;
            var minUsers = 1;
            var maxUsers = 100;

            var organizer = new User("TestUser", "test@example.com") { Id = userId };
            var existingEvent = new Event(organizer, eventTitle, eventDescription, startTime, endTime, location, price, minUsers, maxUsers, []);

            Utils.SetupEventGuid(existingEvent, eventId);

            var events = new List<Event> { existingEvent };

            _mockRepository
               .Setup(r => r.Query())
               .Returns(new TestAsyncEnumerable<Event>(events));

            _unitOfWorkMock.Setup(u => u.EventRepository).Returns(_mockRepository.Object);


            // When
            var resultEvent = await _eventService.GetEventByIdAsync(eventId);

            // Then
            Assert.IsNotNull(resultEvent);
            Assert.AreEqual(existingEvent, resultEvent);
        }

        [TestMethod]
        public async Task GetEventByIdAsync_EventDoesNotExist_ReturnsNull()
        {
            // Given
            var eventId = new Guid("030B4A82-1B7C-11CF-9D53-00AA003C9CB6");

            _mockRepository
                .Setup(r => r.Query())
                .Returns(new TestAsyncEnumerable<Event>(new List<Event>()));

            _unitOfWorkMock.Setup(u => u.EventRepository).Returns(_mockRepository.Object);



            // When
            var resultEvent = await _eventService.GetEventByIdAsync(eventId);

            // Then
            Assert.IsNull(resultEvent);
        }

        [TestMethod]
        public async Task GetAllEventsAsync_EventsExist_ReturnsEvents()
        {
            // Given
            var userId = "TestUserId";
            var eventId1 = new Guid("030B4A82-1B7C-11CF-9D53-00AA003C9CB6");
            var eventId2 = new Guid("030B4A82-1B7C-11CF-9D53-00AA003C9CB2");

            var eventTitle = "Test Event";
            var eventDescription = "This is a test event.";

            var startTime = new DateTime(2024, 5, 24);
            var endTime = startTime.AddHours(2);

            var location = new Location(0, 0);
            var price = 50m;
            var minUsers = 1;
            var maxUsers = 100;

            var ogranizer = new User("TestUser", "test@example.com") { Id = userId };

            var existingEvent1 = new Event(ogranizer, eventTitle, eventDescription, startTime, endTime, location, price, minUsers, maxUsers, []);
            var existingEvent2 = new Event(ogranizer, eventTitle, eventDescription, startTime, endTime, location, price, minUsers, maxUsers, []);
            Utils.SetupEventGuid(existingEvent1, eventId1);
            Utils.SetupEventGuid(existingEvent2, eventId2);

            var events = new List<Event> { existingEvent1, existingEvent2 };

            _mockRepository
                .Setup(r => r.Query())
                .Returns(new TestAsyncEnumerable<Event>(events));

            _unitOfWorkMock.Setup(u => u.EventRepository).Returns(_mockRepository.Object);

            // When
            var resultEvents = await _eventService.GetAllEventsAsync();

            // Then
            Assert.IsNotNull(resultEvents);
            Assert.AreEqual(2, resultEvents.Count());
            Assert.IsTrue(resultEvents.Contains(existingEvent1));
            Assert.IsTrue(resultEvents.Contains(existingEvent2));
        }

        [TestMethod]
        public async Task GetAllEventsAsync_EventsDoNotExist_ReturnsEmptyCollection()
        {
            // Given

            _mockRepository
                .Setup(r => r.Query())
                .Returns(new TestAsyncEnumerable<Event>(new List<Event>()));

            _unitOfWorkMock.Setup(u => u.EventRepository).Returns(_mockRepository.Object);


            // When
            var resultEvents = await _eventService.GetAllEventsAsync();

            // Then
            Assert.IsNotNull(resultEvents);
            Assert.AreEqual(0, resultEvents.Count());
        }


    }


}
