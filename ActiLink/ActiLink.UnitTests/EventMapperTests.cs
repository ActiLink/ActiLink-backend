using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ActiLink.DTOs;
using ActiLink.MapperProfiles;
using ActiLink.Model;
using ActiLink.Services;
using AutoMapper;
using Moq;

namespace ActiLink.UnitTests
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
            var startTime = new DateTime(2024, 1, 1);
            var endTime = startTime.AddHours(2);
            var location = new Location(1, 2);
            var price = 100.00m;
            var minUsers = 5;
            var maxUsers = 10;
            var organizer = new User("TestUser", "test@example.com") { Id = userId };

            var ceoToMap = new CreateEventObject(userId, startTime, endTime, location, price, minUsers, maxUsers);
            var expectedEvent = new Event(organizer, startTime, endTime, location, price, minUsers, maxUsers);


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
            var startTime = new DateTime(2024, 1, 1);
            var endTime = startTime.AddHours(2);
            var location = new Location(1, 2);
            var price = 100.00m;
            var minUsers = 5;
            var maxUsers = 10;

            var newEventDto = new NewEventDto(userId, startTime, endTime, location, price, minUsers, maxUsers);
            var expectedCeo = new CreateEventObject(userId, startTime, endTime, location, price, minUsers, maxUsers);


            // When
            var mappedCeo = _mapper.Map<CreateEventObject>(newEventDto);

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
            var startTime = new DateTime(2024, 1, 1);
            var endTime = startTime.AddHours(2);
            var location = new Location(1, 2);
            var price = 100.00m;
            var minUsers = 5;
            var maxUsers = 10;

            var organizer = new User("TestUser", "test@example.com") { Id = userId };
            var eventToMap = new Event(organizer, startTime, endTime, location, price, minUsers, maxUsers);
            Utils.SetupEventGuid(eventToMap, eventId);
            var expectedEventDto = new EventDto(eventId, userId, startTime, endTime, location, price, minUsers, maxUsers, [], []);

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


    }
}
