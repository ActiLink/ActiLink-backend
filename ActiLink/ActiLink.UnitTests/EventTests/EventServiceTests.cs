using ActiLink.Events;
using ActiLink.Events.Service;
using ActiLink.Hobbies;
using ActiLink.Organizers.BusinessClients;
using ActiLink.Organizers.Users;
using ActiLink.Shared.Model;
using ActiLink.Shared.Repositories;
using AutoMapper;
using Moq;


namespace ActiLink.UnitTests.EventTests
{
    [TestClass]
    public class EventServiceTests
    {
        private Mock<IUnitOfWork> _unitOfWorkMock = null!;
        private Mock<IMapper> _mapperMock = null!;
        private Mock<IRepository<Event>> _mockEventRepository = null!;
        private Mock<IRepository<Hobby>> _mockHobbyRepository = null!;
        private Mock<IRepository<User>> _mockUserRepository = null!;
        private EventService _eventService = null!;

        [TestInitialize]
        public void Setup()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _mockEventRepository = new Mock<IRepository<Event>>();
            _mockHobbyRepository = new Mock<IRepository<Hobby>>();
            _mockUserRepository = new Mock<IRepository<User>>();
            _eventService = new EventService(_unitOfWorkMock.Object, _mapperMock.Object);

        }

        [TestMethod]
        public async Task CreateEventByUserAsync_Success_ReturnsSuccessResult()
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
            var hobbyNames = new List<string>() { "Tenis, Piłka nożna" };
            var hobbies = hobbyNames.Select(name => new Hobby(name));

            var createEventObject = new CreateEventObject(userId, eventTitle, eventDescription, startTime, endTime, location, price, minUsers, maxUsers, hobbyNames, null);
            var organizer = new User("TestUser", "test@example.com") { Id = userId };
            var createdEvent = new Event(organizer, eventTitle, eventDescription, startTime, endTime, location, price, minUsers, maxUsers, []);

            _mockHobbyRepository
                 .Setup(r => r.Query())
                 .Returns(new TestAsyncEnumerable<Hobby>(hobbies));
            _unitOfWorkMock.Setup(u => u.HobbyRepository).Returns(_mockHobbyRepository.Object);

            // Setup mapper to map from CreateEventObject to Event.
            _mapperMock
                .Setup(m => m.Map<Event>(It.IsAny<CreateEventObject>(), It.IsAny<Action<IMappingOperationOptions<object, Event>>>()))
                .Returns(createdEvent);

            // Setup repository add and save.
            _unitOfWorkMock
                .Setup(u => u.OrganizerRepository.GetByIdAsync(userId))
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

            _mockEventRepository
               .Setup(r => r.Query())
               .Returns(new TestAsyncEnumerable<Event>(events));

            _unitOfWorkMock.Setup(u => u.EventRepository).Returns(_mockEventRepository.Object);


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

            _mockEventRepository
                .Setup(r => r.Query())
                .Returns(new TestAsyncEnumerable<Event>([]));

            _unitOfWorkMock.Setup(u => u.EventRepository).Returns(_mockEventRepository.Object);



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

            var organizer = new User("TestUser", "test@example.com") { Id = userId };

            var existingEvent1 = new Event(organizer, eventTitle, eventDescription, startTime, endTime, location, price, minUsers, maxUsers, []);
            var existingEvent2 = new Event(organizer, eventTitle, eventDescription, startTime, endTime, location, price, minUsers, maxUsers, []);
            Utils.SetupEventGuid(existingEvent2, eventId2);

            var events = new List<Event> { existingEvent1, existingEvent2 };

            _mockEventRepository
                .Setup(r => r.Query())
                .Returns(new TestAsyncEnumerable<Event>(events));

            _unitOfWorkMock.Setup(u => u.EventRepository).Returns(_mockEventRepository.Object);

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

            _mockEventRepository
                .Setup(r => r.Query())
                .Returns(new TestAsyncEnumerable<Event>([]));

            _unitOfWorkMock.Setup(u => u.EventRepository).Returns(_mockEventRepository.Object);


            // When
            var resultEvents = await _eventService.GetAllEventsAsync();

            // Then
            Assert.IsNotNull(resultEvents);
            Assert.AreEqual(0, resultEvents.Count());
        }

        [TestMethod]
        public async Task UpdateEventByUserAsync_Success_ReturnsSuccessResult()
        {
            // Given
            var userId = "TestUserId";
            var eventId = new Guid("030B4A82-1B7C-11CF-9D53-00AA003C9CB6");
            var eventTitle = "Updated Event";
            var eventDescription = "This is an updated event.";
            var startTime = new DateTime(2024, 3, 1);
            var endTime = startTime.AddHours(2);
            var location = new Location(1, 1);
            var price = 75m;
            var minUsers = 2;
            var maxUsers = 50;
            var hobbyNames = new List<string>();

            var updateEventObject = new UpdateEventObject(eventTitle, eventDescription, startTime, endTime,
                                                        location, price, minUsers, maxUsers, hobbyNames, null);
            var organizer = new User("TestUser", "test@example.com") { Id = userId };
            var existingEvent = new Event(organizer, "Old Title", "Old Description", new DateTime(2024, 2, 6), new DateTime(2024, 2, 6).AddHours(3),
                                         new Location(0, 0), 50.0m, 1, 10, []);
            Utils.SetupEventGuid(existingEvent, eventId);

            _mockEventRepository
                .Setup(r => r.Query())
                .Returns(new TestAsyncEnumerable<Event>([existingEvent]));

            _unitOfWorkMock.Setup(u => u.EventRepository).Returns(_mockEventRepository.Object);
            _unitOfWorkMock
                .Setup(u => u.OrganizerRepository.GetByIdAsync(userId))
                .ReturnsAsync(organizer);

            _mockHobbyRepository
                .Setup(r => r.Query())
                .Returns(new TestAsyncEnumerable<Hobby>([]));
            _unitOfWorkMock.Setup(u => u.HobbyRepository).Returns(_mockHobbyRepository.Object);

            _unitOfWorkMock
                .Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);


            _mapperMock
                 .Setup(m => m.Map<UpdateEventObject, Event>(It.IsAny<UpdateEventObject>(), It.IsAny<Event>(), It.IsAny<Action<IMappingOperationOptions<UpdateEventObject, Event>>>()))
                 .Callback<UpdateEventObject, Event, Action<IMappingOperationOptions<UpdateEventObject, Event>>>((src, dest, opt) =>
                 {
                     typeof(Event).GetProperty(nameof(Event.Title))!.SetValue(dest, src.Title);
                     typeof(Event).GetProperty(nameof(Event.Description))!.SetValue(dest, src.Description);
                     typeof(Event).GetProperty(nameof(Event.StartTime))!.SetValue(dest, src.StartTime);
                     typeof(Event).GetProperty(nameof(Event.EndTime))!.SetValue(dest, src.EndTime);
                     typeof(Event).GetProperty(nameof(Event.Location))!.SetValue(dest, src.Location);
                     typeof(Event).GetProperty(nameof(Event.Price))!.SetValue(dest, src.Price);
                     typeof(Event).GetProperty(nameof(Event.MinUsers))!.SetValue(dest, src.MinUsers);
                     typeof(Event).GetProperty(nameof(Event.MaxUsers))!.SetValue(dest, src.MaxUsers);
                 });


            // When
            var result = await _eventService.UpdateEventAsync(eventId, updateEventObject, userId);

            // Then
            Assert.IsTrue(result.Succeeded);
            Assert.IsNotNull(result.Data);
            Assert.AreEqual(eventTitle, result.Data.Title);
            Assert.AreEqual(eventDescription, result.Data.Description);
            Assert.AreEqual(startTime, result.Data.StartTime);
            Assert.AreEqual(endTime, result.Data.EndTime);
            _unitOfWorkMock.Verify(u => u.EventRepository.Update(It.IsAny<Event>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
        [TestMethod]
        public async Task UpdateEventAsync_EventDoesNotExist_ReturnsFailureResult()
        {
            // Given
            var userId = "TestUserId";
            var eventId = new Guid("44494479-076b-47e1-8004-399a5aa58156");
            var updateEventObject = new UpdateEventObject("Updated Title", "Updated Description", DateTime.Now, DateTime.Now.AddHours(1),
                                                          new Location(0, 0), 50.0m, 1, 10, [], null);

            _mockEventRepository
                .Setup(r => r.Query())
                .Returns(new TestAsyncEnumerable<Event>([]));

            _unitOfWorkMock.Setup(u => u.EventRepository).Returns(_mockEventRepository.Object);

            // When
            var result = await _eventService.UpdateEventAsync(eventId, updateEventObject, userId);

            // Then
            Assert.IsFalse(result.Succeeded);
            Assert.IsTrue(result.Errors.Contains("Event not found"));
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [TestMethod]
        public async Task DeleteEventByIdByUserAsync_Success_ReturnsSuccessResult()
        {
            // Given
            var userId = "TestUserId";
            var eventId = new Guid("44494479-076b-47e1-8004-399a5aa58156");
            var organizer = new User("TestUser", "test@example.com") { Id = userId };
            var eventToDelete = new Event(organizer, "Event Title", "Event Description", DateTime.Now, DateTime.Now.AddHours(1),
                                          new Location(0, 0), 50.0m, 1, 10, []);
            Utils.SetupEventGuid(eventToDelete, eventId);

            _mockEventRepository
                .Setup(r => r.Query())
                .Returns(new TestAsyncEnumerable<Event>([eventToDelete]));

            _unitOfWorkMock.Setup(u => u.EventRepository).Returns(_mockEventRepository.Object);
            _unitOfWorkMock
                .Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // When
            var result = await _eventService.DeleteEventByIdAsync(eventId, userId);

            // Then
            Assert.IsTrue(result.Succeeded);
            _unitOfWorkMock.Verify(u => u.EventRepository.Delete(It.IsAny<Event>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [TestMethod]
        public async Task DeleteEventByIdAsync_EventDoesNotExist_ReturnsFailureResult()
        {
            // Given
            var userId = "TestUserId";
            var eventId = new Guid("44494479-076b-47e1-8004-399a5aa58156");

            _mockEventRepository
                .Setup(r => r.Query())
                .Returns(new TestAsyncEnumerable<Event>([]));

            _unitOfWorkMock.Setup(u => u.EventRepository).Returns(_mockEventRepository.Object);

            // When
            var result = await _eventService.DeleteEventByIdAsync(eventId, userId);

            // Then
            Assert.IsFalse(result.Succeeded);
            Assert.IsTrue(result.Errors.Contains("Event not found"));
            _unitOfWorkMock.Verify(u => u.EventRepository.Delete(It.IsAny<Event>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }


        [TestMethod]
        public async Task CreateEventByBusinessClient_Success_ReturnsSuccessResult()
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
            var hobbyNames = new List<string>() { "Tenis, Piłka nożna" };
            var hobbies = hobbyNames.Select(name => new Hobby(name));

            var createEventObject = new CreateEventObject(userId, eventTitle, eventDescription, startTime, endTime, location, price, minUsers, maxUsers, hobbyNames, null);
            var organizer = new BusinessClient("TestUser", "test@example.com", "PL123456789") { Id = userId };
            var createdEvent = new Event(organizer, eventTitle, eventDescription, startTime, endTime, location, price, minUsers, maxUsers, []);

            _mockHobbyRepository
                 .Setup(r => r.Query())
                 .Returns(new TestAsyncEnumerable<Hobby>(hobbies));
            _unitOfWorkMock.Setup(u => u.HobbyRepository).Returns(_mockHobbyRepository.Object);

            // Setup mapper to map from CreateEventObject to Event.
            _mapperMock
                .Setup(m => m.Map<Event>(It.IsAny<CreateEventObject>(), It.IsAny<Action<IMappingOperationOptions<object, Event>>>()))
                .Returns(createdEvent);

            // Setup repository add and save.
            _unitOfWorkMock
                .Setup(u => u.OrganizerRepository.GetByIdAsync(userId))
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
            _unitOfWorkMock.Verify(u => u.EventRepository.AddAsync(It.IsAny<Event>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
            _unitOfWorkMock.Verify(u => u.OrganizerRepository.GetByIdAsync(userId), Times.Once);
            _mapperMock.Verify(m => m.Map<Event>(It.IsAny<CreateEventObject>(), It.IsAny<Action<IMappingOperationOptions<object, Event>>>()), Times.Once);
        }

        [TestMethod]
        public async Task UpdateEventByBusinessClientAsync_Success_ReturnsSuccessResult()
        {
            // Given
            var userId = "TestUserId";
            var eventId = new Guid("030B4A82-1B7C-11CF-9D53-00AA003C9CB6");
            var eventTitle = "Updated Event";
            var eventDescription = "This is an updated event.";
            var startTime = new DateTime(2024, 3, 1);
            var endTime = startTime.AddHours(2);
            var location = new Location(1, 1);
            var price = 75m;
            var minUsers = 2;
            var maxUsers = 50;
            var hobbyNames = new List<string>();

            var updateEventObject = new UpdateEventObject(eventTitle, eventDescription, startTime, endTime,
                                                        location, price, minUsers, maxUsers, hobbyNames, "");
            var organizer = new BusinessClient("TestUser", "test@example.com", "PL123456789") { Id = userId };
            var existingEvent = new Event(organizer, "Old Title", "Old Description", new DateTime(2024, 2, 6), new DateTime(2024, 2, 6).AddHours(3),
                                         new Location(0, 0), 50.0m, 1, 10, []);
			Utils.SetupEventGuid(existingEvent, eventId);

			_mockEventRepository
				.Setup(r => r.Query())
				.Returns(new TestAsyncEnumerable<Event>([existingEvent]));

			_unitOfWorkMock.Setup(u => u.EventRepository).Returns(_mockEventRepository.Object);
			_unitOfWorkMock
				.Setup(u => u.OrganizerRepository.GetByIdAsync(userId))
				.ReturnsAsync(organizer);

			_mockHobbyRepository
				.Setup(r => r.Query())
				.Returns(new TestAsyncEnumerable<Hobby>([]));
			_unitOfWorkMock.Setup(u => u.HobbyRepository).Returns(_mockHobbyRepository.Object);

			_unitOfWorkMock
				.Setup(u => u.SaveChangesAsync())
				.ReturnsAsync(1);


			_mapperMock
				 .Setup(m => m.Map<UpdateEventObject, Event>(It.IsAny<UpdateEventObject>(), It.IsAny<Event>(), It.IsAny<Action<IMappingOperationOptions<UpdateEventObject, Event>>>()))
				 .Callback<UpdateEventObject, Event, Action<IMappingOperationOptions<UpdateEventObject, Event>>>((src, dest, opt) =>
				 {
                     typeof(Event).GetProperty(nameof(Event.Title))!.SetValue(dest, src.Title);
                     typeof(Event).GetProperty(nameof(Event.Description))!.SetValue(dest, src.Description);
                     typeof(Event).GetProperty(nameof(Event.StartTime))!.SetValue(dest, src.StartTime);
                     typeof(Event).GetProperty(nameof(Event.EndTime))!.SetValue(dest, src.EndTime);
                     typeof(Event).GetProperty(nameof(Event.Location))!.SetValue(dest, src.Location);
                     typeof(Event).GetProperty(nameof(Event.Price))!.SetValue(dest, src.Price);
                     typeof(Event).GetProperty(nameof(Event.MinUsers))!.SetValue(dest, src.MinUsers);
                     typeof(Event).GetProperty(nameof(Event.MaxUsers))!.SetValue(dest, src.MaxUsers);
				 }).Returns(existingEvent);


			// When
			var result = await _eventService.UpdateEventAsync(eventId, updateEventObject, userId);

            // Then
            Assert.IsTrue(result.Succeeded);
            Assert.IsNotNull(result.Data);
            Assert.AreEqual(eventTitle, result.Data.Title);
            Assert.AreEqual(eventDescription, result.Data.Description);
            Assert.AreEqual(startTime, result.Data.StartTime);
            Assert.AreEqual(endTime, result.Data.EndTime);
            _unitOfWorkMock.Verify(u => u.EventRepository.Update(It.IsAny<Event>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [TestMethod]
        public async Task UpdateEventByBusinessClientAsync_EventDoesNotExist_ReturnsFailureResult()
        {
            // Given
            var userId = "TestUserId";
            var eventId = new Guid("44494479-076b-47e1-8004-399a5aa58156");
            var updateEventObject = new UpdateEventObject("Updated Title", "Updated Description", DateTime.Now, DateTime.Now.AddHours(1),
                                                          new Location(0, 0), 50.0m, 1, 10, [], "");
            _mockEventRepository
                .Setup(r => r.Query())
                .Returns(new TestAsyncEnumerable<Event>([]));
            _unitOfWorkMock.Setup(u => u.EventRepository).Returns(_mockEventRepository.Object);
            // When
            var result = await _eventService.UpdateEventAsync(eventId, updateEventObject, userId);
            // Then
            Assert.IsFalse(result.Succeeded);
            Assert.IsTrue(result.Errors.Contains("Event not found"));
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }


        [TestMethod]
        public async Task EnrollEventByUserAsync_Success_ReturnsSuccessResult()
        {
            // Given
            //var organizerId = "TestOrganizerId";
            var userId = "TestUserId";
            var eventId = new Guid("44494479-076b-47e1-8004-399a5aa58156");
            //var organizer = new User("TestOrganizer", "testorganizer@email.com") { Id = organizerId };
            var user = new User("TestUser", "testuser@email.com") { Id = userId };
            var eventToSignUp = new Event(null!, "Test Event", "This is a test event.", DateTime.Now, DateTime.Now.AddHours(1),
                                          new Location(0, 0), 50.0m, 1, 10, []);

            Utils.SetupEventGuid(eventToSignUp, eventId);

            // Setup the event repository to return the event
            _mockEventRepository
                .Setup(r => r.Query())
                .Returns(new TestAsyncEnumerable<Event>([eventToSignUp]));

            _mockUserRepository
                .Setup(r => r.Query())
                .Returns(new TestAsyncEnumerable<User>([user]));

            _unitOfWorkMock
                .Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);


            _unitOfWorkMock.Setup(u => u.EventRepository).Returns(_mockEventRepository.Object);
            _unitOfWorkMock.Setup(u => u.UserRepository).Returns(_mockUserRepository.Object);

            // When
            var result = await _eventService.SignUpForEventAsync(eventId, userId);

            // Then
            Assert.IsTrue(result.Succeeded);
            Assert.IsTrue(user.SignedUpEvents.Contains(eventToSignUp));
            Assert.IsTrue(eventToSignUp.SignUpList.Contains(user));
            Assert.IsNotNull(result.Data);
            Assert.AreEqual(eventToSignUp, result.Data);
            _unitOfWorkMock.Verify(u => u.EventRepository.Query(), Times.Once);
            _unitOfWorkMock.Verify(u => u.UserRepository.Query(), Times.Once);
        }

        [TestMethod]
        public async Task EnrollEventByUserAsync_EventDoesNotExist_ReturnsFailureResult()
        {
            // Given
            var userId = "TestUserId";
            var user = new User("TestUser", "testuser@email.com") { Id = userId };
            var eventId = new Guid("44494479-076b-47e1-8004-399a5aa58156");

            _mockEventRepository
                .Setup(r => r.Query())
                .Returns(new TestAsyncEnumerable<Event>([]));

            _mockUserRepository
                .Setup(r => r.Query())
                .Returns(new TestAsyncEnumerable<User>([user]));

            _unitOfWorkMock.Setup(u => u.EventRepository).Returns(_mockEventRepository.Object);
            _unitOfWorkMock.Setup(u => u.UserRepository).Returns(_mockUserRepository.Object);

            // When
            var result = await _eventService.SignUpForEventAsync(eventId, userId);

            // Then
            Assert.IsFalse(result.Succeeded);
            Assert.IsTrue(result.Errors.Contains("Event not found"));
        }

        [TestMethod]
        public async Task EnrollEventByUserAsync_UserIsAlreadySignedUp_ReturnsFailureResult()
        {
            // Given
            var userId = "TestUserId";
            var user = new User("TestUser", "testuser@email.com") { Id = userId };
            var eventId = new Guid("44494479-076b-47e1-8004-399a5aa58156");

            var eventToSignUp = new Event(null!, "Test Event", "This is a test event.", DateTime.Now, DateTime.Now.AddHours(1),
                              new Location(0, 0), 50.0m, 1, 10, []);

            Utils.SetupEventGuid(eventToSignUp, eventId);

            user.SignedUpEvents.Add(eventToSignUp);
            eventToSignUp.SignUpList.Add(user);

            _mockEventRepository
                .Setup(r => r.Query())
                .Returns(new TestAsyncEnumerable<Event>([eventToSignUp]));

            _mockUserRepository
                .Setup(r => r.Query())
                .Returns(new TestAsyncEnumerable<User>([user]));

            _unitOfWorkMock.Setup(u => u.EventRepository).Returns(_mockEventRepository.Object);
            _unitOfWorkMock.Setup(u => u.UserRepository).Returns(_mockUserRepository.Object);


            // When
            var result = await _eventService.SignUpForEventAsync(eventId, userId);

            // Then
            Assert.IsFalse(result.Succeeded);
            Assert.IsTrue(result.Errors.Contains("You are already signed up for this event."));
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [TestMethod]
        public async Task EnrollEventByUserAsync_UserLimitAchieved_ReturnsFailureResult()
        {
            // Given
            var user1Id = "TestUserId1";
            var user1 = new User("TestUser1", "testuser1@email.com") { Id = user1Id };
            var user2Id = "TestUserId2";
            var user2 = new User("TestUser2", "testuser2@email.com") { Id = user2Id };
            var eventId = new Guid("44494479-076b-47e1-8004-399a5aa58156");

            var eventToSignUp = new Event(null!, "Test Event", "This is a test event.", DateTime.Now, DateTime.Now.AddHours(1),
                              new Location(0, 0), 50.0m, 0, 1, []);

            Utils.SetupEventGuid(eventToSignUp, eventId);

            eventToSignUp.SignUpList.Add(user1);
            user1.SignedUpEvents.Add(eventToSignUp);

            _mockEventRepository
                .Setup(r => r.Query())
                .Returns(new TestAsyncEnumerable<Event>([eventToSignUp]));
            _mockUserRepository
                .Setup(r => r.Query())
                .Returns(new TestAsyncEnumerable<User>([user1, user2]));


            _unitOfWorkMock.Setup(u => u.EventRepository).Returns(_mockEventRepository.Object);
            _unitOfWorkMock.Setup(u => u.UserRepository).Returns(_mockUserRepository.Object);


            // When
            var result = await _eventService.SignUpForEventAsync(eventId, user2Id);

            // Then
            Assert.IsFalse(result.Succeeded);
            Assert.IsTrue(result.Errors.Contains("Event is full"));
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [TestMethod]
        public async Task EnrollEventByUserAsync_NoLimit_ReturnsSuccessResult()
        {
            // Given
            var userId = "TestUserId";
            var eventId = new Guid("44494479-076b-47e1-8004-399a5aa58156");
            var user = new User("TestUser", "testuser@email.com") { Id = userId };
            var eventToSignUp = new Event(null!, "Test Event", "This is a test event.", DateTime.Now, DateTime.Now.AddHours(1),
                                          new Location(0, 0), 50.0m, 1, 0, []);

            Utils.SetupEventGuid(eventToSignUp, eventId);

            // Setup the event repository to return the event
            _mockEventRepository
                .Setup(r => r.Query())
                .Returns(new TestAsyncEnumerable<Event>([eventToSignUp]));

            _mockUserRepository
                .Setup(r => r.Query())
                .Returns(new TestAsyncEnumerable<User>([user]));

            _unitOfWorkMock
                .Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);


            _unitOfWorkMock.Setup(u => u.EventRepository).Returns(_mockEventRepository.Object);
            _unitOfWorkMock.Setup(u => u.UserRepository).Returns(_mockUserRepository.Object);

            // When
            var result = await _eventService.SignUpForEventAsync(eventId, userId);

            // Then
            Assert.IsTrue(result.Succeeded);
            Assert.IsTrue(user.SignedUpEvents.Contains(eventToSignUp));
            Assert.IsTrue(eventToSignUp.SignUpList.Contains(user));
            Assert.IsNotNull(result.Data);
            Assert.AreEqual(eventToSignUp, result.Data);
            _unitOfWorkMock.Verify(u => u.EventRepository.Query(), Times.Once);
            _unitOfWorkMock.Verify(u => u.UserRepository.Query(), Times.Once);
        }

        [TestMethod]
        public async Task UnsignUserFromEvent_Success_ReturnsSuccessResult()
        {
            // Given
            var userId = "TestUserId";
            var eventId = new Guid("44494479-076b-47e1-8004-399a5aa58156");
            var user = new User("TestUser", "testuser@email.com") { Id = userId };
            var eventToUnsign = new Event(null!, "Test Event", "This is a test event.", DateTime.Now, DateTime.Now.AddHours(1),
                                          new Location(0, 0), 50.0m, 1, 10, []);
            Utils.SetupEventGuid(eventToUnsign, eventId);

            eventToUnsign.SignUpList.Add(user);
            user.SignedUpEvents.Add(eventToUnsign);

            _mockEventRepository
                .Setup(r => r.Query())
                .Returns(new TestAsyncEnumerable<Event>([eventToUnsign]));
            _mockUserRepository
                .Setup(r => r.Query())
                .Returns(new TestAsyncEnumerable<User>([user]));
            _unitOfWorkMock.Setup(u => u.EventRepository).Returns(_mockEventRepository.Object);
            _unitOfWorkMock.Setup(u => u.UserRepository).Returns(_mockUserRepository.Object);
            _unitOfWorkMock
                .Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // When
            var result = await _eventService.WithdrawFromEventAsync(eventId, userId);

            // Then
            Assert.IsTrue(result.Succeeded);
            Assert.IsFalse(user.SignedUpEvents.Contains(eventToUnsign));
            Assert.IsFalse(eventToUnsign.SignUpList.Contains(user));
            Assert.IsNotNull(result.Data);
            Assert.AreEqual(eventToUnsign, result.Data);
            _unitOfWorkMock.Verify(u => u.EventRepository.Query(), Times.Once);
            _unitOfWorkMock.Verify(u => u.UserRepository.Query(), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [TestMethod]
        public async Task UnsignUserFromEvent_UserIsNotSignUp_ReturnsSuccessResult()
        {
            // Given
            var userId = "TestUserId";
            var eventId = new Guid("44494479-076b-47e1-8004-399a5aa58156");
            var user = new User("TestUser", "testuser@email.com") { Id = userId };
            var eventToUnsign = new Event(null!, "Test Event", "This is a test event.", DateTime.Now, DateTime.Now.AddHours(1),
                                          new Location(0, 0), 50.0m, 1, 10, []);
            Utils.SetupEventGuid(eventToUnsign, eventId);

            _mockEventRepository
                .Setup(r => r.Query())
                .Returns(new TestAsyncEnumerable<Event>([eventToUnsign]));
            _mockUserRepository
                .Setup(r => r.Query())
                .Returns(new TestAsyncEnumerable<User>([user]));
            _unitOfWorkMock.Setup(u => u.EventRepository).Returns(_mockEventRepository.Object);
            _unitOfWorkMock.Setup(u => u.UserRepository).Returns(_mockUserRepository.Object);

            // When
            var result = await _eventService.WithdrawFromEventAsync(eventId, userId);

            // Then
            Assert.IsFalse(result.Succeeded);
            Assert.IsTrue(result.Errors.Contains("You are not signed up for this event."));
            _unitOfWorkMock.Verify(u => u.EventRepository.Query(), Times.Once);
            _unitOfWorkMock.Verify(u => u.UserRepository.Query(), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }
    }


}
