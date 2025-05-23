﻿using ActiLink.Events;
using ActiLink.Events.DTOs;
using ActiLink.Events.Infrastructure;
using ActiLink.Events.Service;
using ActiLink.Hobbies;
using ActiLink.Hobbies.DTOs;
using ActiLink.Organizers.DTOs;
using ActiLink.Organizers.Infrastructure;
using ActiLink.Organizers.Users;
using ActiLink.Shared.Model;
using AutoMapper;

namespace ActiLink.UnitTests.EventTests
{
    [TestClass]
    public class EventMapperTests
    {
        private Mapper _mapper = null!;

        [TestInitialize]
        public void Setup()
        {
            _mapper = new Mapper(new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new EventProfile());
                cfg.AddProfile(new OrganizerProfile());
            }
                ));
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
            var hobbyNames = new List<string>();

            var ceoToMap = new CreateEventObject(userId, eventTitle, eventDescription, startTime, endTime, location, price, minUsers, maxUsers, hobbyNames);
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
            var hobbyNames = new List<string>();
            var hobbies = hobbyNames.Select(n => new HobbyDto(n));

            var newEventDto = new NewEventDto(eventTitle, eventDescription, startTime, endTime, location, price, minUsers, maxUsers, hobbies);
            var expectedCeo = new CreateEventObject(userId, eventTitle, eventDescription, startTime, endTime, location, price, minUsers, maxUsers, hobbyNames);


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
            var organizerDto = new OrganizerDto(organizer.Id, organizer.UserName!);
            var eventToMap = new Event(organizer, eventTitle, eventDescription, startTime, endTime, location, price, minUsers, maxUsers, []);
            Utils.SetupEventGuid(eventToMap, eventId);
            var expectedEventDto = new EventDto(eventId, eventTitle, eventDescription, startTime, endTime, location, price, minUsers, maxUsers, [], organizerDto, []);

            // When
            var mappedEventDto = _mapper.Map<EventDto>(eventToMap);

            // Then
            Assert.IsNotNull(mappedEventDto);
            Assert.AreEqual(expectedEventDto.Id, mappedEventDto.Id);
            Assert.AreEqual(expectedEventDto.Title, mappedEventDto.Title);
            Assert.AreEqual(expectedEventDto.Description, mappedEventDto.Description);
            Assert.AreEqual(expectedEventDto.StartTime, mappedEventDto.StartTime);
            Assert.AreEqual(expectedEventDto.EndTime, mappedEventDto.EndTime);
            Assert.AreEqual(expectedEventDto.Location, mappedEventDto.Location);
            Assert.AreEqual(expectedEventDto.Price, mappedEventDto.Price);
            Assert.AreEqual(expectedEventDto.MinUsers, mappedEventDto.MinUsers);
            Assert.AreEqual(expectedEventDto.MaxUsers, mappedEventDto.MaxUsers);
            Assert.AreEqual(expectedEventDto.Hobbies.Count, mappedEventDto.Hobbies.Count);
            Assert.AreEqual(expectedEventDto.Organizer.Id, mappedEventDto.Organizer.Id);
            Assert.AreEqual(expectedEventDto.Organizer.Name, mappedEventDto.Organizer.Name);
            Assert.AreEqual(expectedEventDto.Participants.Count, mappedEventDto.Participants.Count);
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
            var hobbyNames = new List<string> { new("Hobby1") };
            var hobbies = hobbyNames.Select(n => new HobbyDto(n));

            var updateEventDto = new UpdateEventDto(eventTitle, eventDescription, startTime, endTime, location, price, minUsers, maxUsers, hobbies);
            var expectedUpdateObject = new UpdateEventObject(eventTitle, eventDescription, startTime, endTime, location, price, minUsers, maxUsers, hobbyNames);

            // When
            var mappedUpdateObject = _mapper.Map<UpdateEventObject>(updateEventDto, opts => opts.Items["EventId"] = eventId);

            // Then
            Assert.IsNotNull(mappedUpdateObject);
            Assert.AreEqual(expectedUpdateObject.Title, mappedUpdateObject.Title);
            Assert.AreEqual(expectedUpdateObject.Description, mappedUpdateObject.Description);
            Assert.AreEqual(expectedUpdateObject.StartTime, mappedUpdateObject.StartTime);
            Assert.AreEqual(expectedUpdateObject.EndTime, mappedUpdateObject.EndTime);
            Assert.AreEqual(expectedUpdateObject.Location, mappedUpdateObject.Location);
            Assert.AreEqual(expectedUpdateObject.Price, mappedUpdateObject.Price);
            Assert.AreEqual(expectedUpdateObject.MinUsers, mappedUpdateObject.MinUsers);
            Assert.AreEqual(expectedUpdateObject.MaxUsers, mappedUpdateObject.MaxUsers);
            CollectionAssert.AreEqual(expectedUpdateObject.RelatedHobbyNames.ToList(), mappedUpdateObject.RelatedHobbyNames.ToList());
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

            var hobbyName = new string("Hobby1");
            var hobbyNames = new List<string> { hobbyName };
            var hobby = new Hobby("TestHobby");
            var hobbies = new List<Hobby> { hobby };

            var organizer = new User("TestUser", "test@example.com") { Id = userId };
            var existingEvent = new Event(organizer, "Old Title", "Old Description", new DateTime(2022, 11, 11), new DateTime(2022, 11, 12),
                                         new Location(0, 0), 50.0m, 2, 8, []);
            Utils.SetupEventGuid(existingEvent, eventId);

            var updateEventObject = new UpdateEventObject(eventTitle, eventDescription, startTime, endTime, location, price, minUsers, maxUsers, hobbyNames);

            // When
            _mapper.Map(updateEventObject, existingEvent, opts => opts.Items["Hobbies"] = hobbies);

            // Then
            Assert.AreEqual(eventTitle, existingEvent.Title);
            Assert.AreEqual(eventDescription, existingEvent.Description);
            Assert.AreEqual(startTime, existingEvent.StartTime);
            Assert.AreEqual(endTime, existingEvent.EndTime);
            Assert.AreEqual(location, existingEvent.Location);
            Assert.AreEqual(price, existingEvent.Price);
            Assert.AreEqual(minUsers, existingEvent.MinUsers);
            Assert.AreEqual(maxUsers, existingEvent.MaxUsers);
            Assert.AreEqual(1, existingEvent.RelatedHobbies.Count);
        }


        [TestMethod]
        public void MapEventToReducedEventDto_ShouldMapCorrectly()
        {
            // Given
            var eventId = new Guid("030B4A82-1B7C-11CF-9D53-00AA003C9CB6");
            var userId = "TestUserId";
            var eventTitle = "Test Event";
            var eventDescription = "This is an updated test event.";
            var startTime = new DateTime(2024, 2, 1);
            var endTime = startTime.AddHours(3);
            var location = new Location(3, 4);
            var price = 150.00m;
            var minUsers = 3;
            var maxUsers = 15;

            var organizer = new User("TestUser", "test@example.com") { Id = userId };
            var organizerDto = new OrganizerDto(organizer.Id, organizer.UserName!);
            var eventToMap = new Event(organizer, eventTitle, eventDescription, startTime, endTime, location, price, minUsers, maxUsers, []);
            Utils.SetupEventGuid(eventToMap, eventId);

            var expectedReducedEventDto = new ReducedEventDto(eventId, eventTitle, organizerDto, startTime);

            // When
            var mappedReducedEventDto = _mapper.Map<ReducedEventDto>(eventToMap);

            // Then
            Assert.IsNotNull(mappedReducedEventDto);
            Assert.AreEqual(expectedReducedEventDto, mappedReducedEventDto);
        }

    }
}
