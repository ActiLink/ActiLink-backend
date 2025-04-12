using ActiLink.Events;
using ActiLink.Events.DTOs;
using ActiLink.Events.Infrastructure;
using ActiLink.Events.Service;
using ActiLink.Hobbies;
using ActiLink.Organizers.Users;
using ActiLink.Shared.Model;
using AutoMapper;

namespace ActiLink.UnitTests.EventTests
{
    [TestClass]
    public class EventMapperTests
    {
        private IMapper _mapper = null!;

        [TestInitialize]
        public void Setup()
        {
            _mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile(new EventProfile())));
        }

        [TestMethod]
        public void MapCreateEventObjectToEvent_ShouldMapCorrectly()
        {
            // Given
            var userId = "TestUserId";
            var eventTitle = "Test Event";
            var eventDescription = "This is a test event.";
            var startTime = new DateTime(2024, 1, 1);
            var endTime = startTime.AddHours(2);
            var location = new Location(1, 2);
            var price = 100.00m;
            var minUsers = 5;
            var maxUsers = 10;
            var organizer = new User("TestUser", "test@example.com") { Id = userId };
            var hobbyIds = new List<Guid>();

            var ceoToMap = new CreateEventObject(userId, eventTitle, eventDescription, startTime, endTime, location, price, minUsers, maxUsers, hobbyIds);
            var expectedEvent = new Event(organizer, eventTitle, eventDescription, startTime, endTime, location, price, minUsers, maxUsers, []);


            // When
            var mappedEvent = _mapper.Map<Event>(ceoToMap, opts => opts.Items["Organizer"] = organizer);

            // Then
            Assert.IsNotNull(mappedEvent);
#pragma warning disable MSTEST0032 // Assertion condition is always true
            Assert.IsNotNull(mappedEvent.Organizer);
#pragma warning restore MSTEST0032 // Assertion condition is always true
            Assert.AreEqual(expectedEvent.Organizer.Id, mappedEvent.Organizer.Id);
            Assert.AreEqual(expectedEvent.StartTime, mappedEvent.StartTime);
            Assert.AreEqual(expectedEvent.EndTime, mappedEvent.EndTime);
            Assert.AreEqual(expectedEvent.Location, mappedEvent.Location);
            Assert.AreEqual(expectedEvent.Price, mappedEvent.Price);
            Assert.AreEqual(expectedEvent.MinUsers, mappedEvent.MinUsers);
            Assert.AreEqual(expectedEvent.MaxUsers, mappedEvent.MaxUsers);
        }

        [TestMethod]
        public void MapNewEventDtoToCreateEventObject_ShouldMapCorrectly()
        {
            // Given
            var userId = "TestUserId";
            var eventTitle = "Test Event";
            var eventDescription = "This is a test event.";
            var startTime = new DateTime(2024, 1, 1);
            var endTime = startTime.AddHours(2);
            var location = new Location(1, 2);
            var price = 100.00m;
            var minUsers = 5;
            var maxUsers = 10;
            var hobbyIds = new List<Guid>();

            var newEventDto = new NewEventDto(eventTitle, eventDescription, startTime, endTime, location, price, minUsers, maxUsers, hobbyIds);
            var expectedCeo = new CreateEventObject(userId, eventTitle, eventDescription, startTime, endTime, location, price, minUsers, maxUsers, hobbyIds);


            // When
            var mappedCeo = _mapper.Map<CreateEventObject>(newEventDto, opts => opts.Items["OrganizerId"] = userId);

            // Then
            Assert.IsNotNull(mappedCeo);
            Assert.AreEqual(expectedCeo.OrganizerId, mappedCeo.OrganizerId);
            Assert.AreEqual(expectedCeo.StartTime, mappedCeo.StartTime);
            Assert.AreEqual(expectedCeo.EndTime, mappedCeo.EndTime);
#pragma warning disable MSTEST0032 // Assertion condition is always true
            Assert.IsNotNull(mappedCeo.Location);
#pragma warning restore MSTEST0032 // Assertion condition is always true
            Assert.AreEqual(expectedCeo.Location, mappedCeo.Location);
            Assert.AreEqual(expectedCeo.Price, mappedCeo.Price);
            Assert.AreEqual(expectedCeo.MinUsers, mappedCeo.MinUsers);
            Assert.AreEqual(expectedCeo.MaxUsers, mappedCeo.MaxUsers);
        }

        [TestMethod]
        public void MapEventToEventDto_ShouldMapCorrectly()
        {
            // Given
            var userId = "TestUserId";
            var eventId = new Guid("030B4A82-1B7C-11CF-9D53-00AA003C9CB6");
            var eventTitle = "Test Event";
            var eventDescription = "This is a test event.";
            var startTime = new DateTime(2024, 1, 1);
            var endTime = startTime.AddHours(2);
            var location = new Location(1, 2);
            var price = 100.00m;
            var minUsers = 5;
            var maxUsers = 10;

            var organizer = new User("TestUser", "test@example.com") { Id = userId };
            var eventToMap = new Event(organizer, eventTitle, eventDescription, startTime, endTime, location, price, minUsers, maxUsers, []);
            Utils.SetupEventGuid(eventToMap, eventId);
            var expectedEventDto = new EventDto(eventId, userId, eventTitle, eventDescription, startTime, endTime, location, price, minUsers, maxUsers, [], []);

            // When
            var mappedEventDto = _mapper.Map<EventDto>(eventToMap);

            // Then
            Assert.IsNotNull(mappedEventDto);
            Assert.AreEqual(expectedEventDto.Id, mappedEventDto.Id);
            Assert.AreEqual(expectedEventDto.OrganizerId, mappedEventDto.OrganizerId);
            Assert.AreEqual(expectedEventDto.StartTime, mappedEventDto.StartTime);
            Assert.AreEqual(expectedEventDto.EndTime, mappedEventDto.EndTime);
            Assert.AreEqual(expectedEventDto.Location, mappedEventDto.Location);
            Assert.AreEqual(expectedEventDto.Price, mappedEventDto.Price);
            Assert.AreEqual(expectedEventDto.MinUsers, mappedEventDto.MinUsers);
            Assert.AreEqual(expectedEventDto.MaxUsers, mappedEventDto.MaxUsers);
            Assert.AreEqual(expectedEventDto.Participants.Count, mappedEventDto.Participants.Count);
            Assert.AreEqual(expectedEventDto.Hobbies.Count, mappedEventDto.Hobbies.Count);
        }
        [TestMethod]
        public void MapUpdateEventDtoToUpdateEventObject_ShouldMapCorrectly()
        {
            // Given
            var eventId = new Guid("030B4A82-1B7C-11CF-9D53-00AA003C9CB6");
            var eventTitle = "Updated Event";
            var eventDescription = "This is an updated test event.";
            var startTime = new DateTime(2024, 2, 1);
            var endTime = startTime.AddHours(3);
            var location = new Location(3, 4);
            var price = 150.00m;
            var minUsers = 3;
            var maxUsers = 15;
            var hobbyIds = new List<Guid> { new Guid("44494479-076b-47e1-8004-399a5aa58156") };

            var updateEventDto = new UpdateEventDto(eventTitle, eventDescription, startTime, endTime, location, price, minUsers, maxUsers, hobbyIds);
            var expectedUpdateObject = new UpdateEventObject(eventId, eventTitle, eventDescription, startTime, endTime, location, price, minUsers, maxUsers, hobbyIds);

            // When
            var mappedUpdateObject = _mapper.Map<UpdateEventObject>(updateEventDto, opts => opts.Items["EventId"] = eventId);

            // Then
            Assert.IsNotNull(mappedUpdateObject);
            Assert.AreEqual(expectedUpdateObject.Id, mappedUpdateObject.Id);
            Assert.AreEqual(expectedUpdateObject.Title, mappedUpdateObject.Title);
            Assert.AreEqual(expectedUpdateObject.Description, mappedUpdateObject.Description);
            Assert.AreEqual(expectedUpdateObject.StartTime, mappedUpdateObject.StartTime);
            Assert.AreEqual(expectedUpdateObject.EndTime, mappedUpdateObject.EndTime);
            Assert.AreEqual(expectedUpdateObject.Location, mappedUpdateObject.Location);
            Assert.AreEqual(expectedUpdateObject.Price, mappedUpdateObject.Price);
            Assert.AreEqual(expectedUpdateObject.MinUsers, mappedUpdateObject.MinUsers);
            Assert.AreEqual(expectedUpdateObject.MaxUsers, mappedUpdateObject.MaxUsers);
            CollectionAssert.AreEqual(expectedUpdateObject.RelatedHobbyIds.ToList(), mappedUpdateObject.RelatedHobbyIds.ToList());
        }

        [TestMethod]
        public void MapUpdateEventObjectToEvent_ShouldMapCorrectly()
        {
            // Given
            var eventId = new Guid("030B4A82-1B7C-11CF-9D53-00AA003C9CB6");
            var userId = "TestUserId";
            var eventTitle = "Updated Event";
            var eventDescription = "This is an updated test event.";
            var startTime = new DateTime(2024, 2, 1);
            var endTime = startTime.AddHours(3);
            var location = new Location(3, 4);
            var price = 150.00m;
            var minUsers = 3;
            var maxUsers = 15;

            var hobbyId = new Guid("44494479-076b-47e1-8004-399a5aa58156");
            var hobbyIds = new List<Guid> { hobbyId };
            var hobby = new Hobby("TestHobby");
            Utils.SetupHobbyGuid(hobby, hobbyId);
            var hobbies = new List<Hobby> { hobby };

            var organizer = new User("TestUser", "test@example.com") { Id = userId };
            var existingEvent = new Event(organizer, "Old Title", "Old Description", new DateTime(2022, 11, 11), new DateTime(2022, 11, 12),
                                         new Location(0, 0), 50.0m, 2, 8, []);
            Utils.SetupEventGuid(existingEvent, eventId);

            var updateEventObject = new UpdateEventObject(eventId, eventTitle, eventDescription, startTime, endTime, location, price, minUsers, maxUsers, hobbyIds);

            // When
            _mapper.Map(updateEventObject, existingEvent, opts => opts.Items["Hobbies"] = hobbies);

            // Then
            Assert.AreEqual(eventId, existingEvent.Id);
            Assert.AreEqual(eventTitle, existingEvent.Title);
            Assert.AreEqual(eventDescription, existingEvent.Description);
            Assert.AreEqual(startTime, existingEvent.StartTime);
            Assert.AreEqual(endTime, existingEvent.EndTime);
            Assert.AreEqual(location, existingEvent.Location);
            Assert.AreEqual(price, existingEvent.Price);
            Assert.AreEqual(minUsers, existingEvent.MinUsers);
            Assert.AreEqual(maxUsers, existingEvent.MaxUsers);
            Assert.AreEqual(1, existingEvent.RelatedHobbies.Count);
            Assert.AreEqual(hobbyId, existingEvent.RelatedHobbies.First().Id);
        }

    }
}
