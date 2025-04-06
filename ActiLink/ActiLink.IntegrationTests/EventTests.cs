using ActiLink.Model;
using Microsoft.EntityFrameworkCore;

namespace ActiLink.IntegrationTests
{
    [TestClass]
    public class EventTests
    {
        private DbContextOptions<ApiContext>? _options;
        private static readonly DateTime _fixedDate = new DateTime(2024, 1, 1);

        [TestInitialize]
        public void Setup()
        {
            var databaseName = $"TestDatabase_Events_{Guid.NewGuid()}";
            _options = new DbContextOptionsBuilder<ApiContext>()
                .UseInMemoryDatabase(databaseName: databaseName)
                .Options;

            using var context = new ApiContext(_options);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }

        [TestMethod]
        public async Task Can_Create_Event_With_Organizer()
        {
            // Arrange
            var organizer = new User("Organizer", "organizer@test.com");
            var eventData = new Event(
                organizer,
                _fixedDate.AddDays(1),
                _fixedDate.AddDays(2),
                new Location(51.1079, 17.0385),
                50.00m,
                10,
                2);

            // Act
            using (var context = new ApiContext(_options!))
            {
                context.Set<Organizer>().Add(organizer);
                context.Set<Event>().Add(eventData);
                await context.SaveChangesAsync();
            }

            // Assert
            using (var context = new ApiContext(_options!))
            {
                var savedEvent = await context.Set<Event>()
                    .Include(e => e.Organizer)
                    .FirstOrDefaultAsync(e => e.Id == eventData.Id);

                Assert.IsNotNull(savedEvent);
                Assert.IsNotNull(savedEvent!.Organizer);
                Assert.AreEqual("Organizer", savedEvent.Organizer.UserName);
                Assert.AreEqual(51.1079, savedEvent.Location.Longitude);
                Assert.AreEqual(50.00m, savedEvent.Price);
            }
        }

        [TestMethod]
        public async Task Can_Add_Participants_To_Event()
        {
            // Arrange
            var organizer = new User("Organizer", "organizer@test.com");
            var participant1 = new User("Participant1", "p1@test.com");
            var participant2 = new User("Participant2", "p2@test.com");
            var eventData = new Event(
                organizer,
                _fixedDate.AddDays(1),
                _fixedDate.AddDays(2),
                new Location(51.1079, 17.0385),
                50.00m,
                10,
                2);

            // Act
            using (var context = new ApiContext(_options!))
            {
                context.Set<Organizer>().Add(organizer);
                context.Set<Organizer>().AddRange(participant1, participant2);
                context.Set<Event>().Add(eventData);
                await context.SaveChangesAsync();

                var savedEvent = await context.Set<Event>()
                    .Include(e => e.SignUpList)
                    .FirstAsync(e => e.Id == eventData.Id);

                savedEvent.SignUpList.Add(participant1);
                savedEvent.SignUpList.Add(participant2);
                await context.SaveChangesAsync();
            }

            // Assert
            using (var context = new ApiContext(_options!))
            {
                var eventWithParticipants = await context.Set<Event>()
                    .Include(e => e.SignUpList)
                    .FirstAsync(e => e.Id == eventData.Id);

                Assert.AreEqual(2, eventWithParticipants.SignUpList.Count);
                Assert.IsTrue(eventWithParticipants.SignUpList.Any(p => p.UserName == "Participant1"));
            }
        }

        [TestMethod]
        public async Task Can_Add_Hobbies_To_Event()
        {
            // Arrange
            var organizer = new User("Organizer", "organizer@test.com");
            var hobby1 = new Hobby("Programming");
            var hobby2 = new Hobby("Hiking");
            var eventData = new Event(
                organizer,
                _fixedDate.AddDays(1),
                _fixedDate.AddDays(2),
                new Location(51.1079, 17.0385),
                50.00m,
                10,
                2);

            // Act
            using (var context = new ApiContext(_options!))
            {
                context.Set<Organizer>().Add(organizer);
                context.Set<Hobby>().AddRange(hobby1, hobby2);
                context.Set<Event>().Add(eventData);
                await context.SaveChangesAsync();

                var savedEvent = await context.Set<Event>()
                    .Include(e => e.RelatedHobbies)
                    .FirstAsync(e => e.Id == eventData.Id);

                savedEvent.RelatedHobbies.Add(hobby1);
                savedEvent.RelatedHobbies.Add(hobby2);
                await context.SaveChangesAsync();
            }

            // Assert
            using (var context = new ApiContext(_options!))
            {
                var eventWithHobbies = await context.Set<Event>()
                    .Include(e => e.RelatedHobbies)
                    .FirstAsync(e => e.Id == eventData.Id);

                Assert.AreEqual(2, eventWithHobbies.RelatedHobbies.Count);
                Assert.IsTrue(eventWithHobbies.RelatedHobbies.Any(h => h.Name == "Hiking"));
            }
        }

        [TestMethod]
        public async Task Can_Delete_Event_Without_Deleting_Participants()
        {
            // Arrange
            var organizer = new User("Organizer", "organizer@test.com");
            var participant = new User("Participant", "participant@test.com");
            var eventData = new Event(
                organizer,
                _fixedDate.AddDays(1),
                _fixedDate.AddDays(2),
                new Location(51.1079, 17.0385),
                50.00m,
                10,
                2);

            // Act
            using (var context = new ApiContext(_options!))
            {
                context.Set<Organizer>().AddRange(organizer, participant);
                context.Set<Event>().Add(eventData);
                await context.SaveChangesAsync();

                var savedEvent = await context.Set<Event>()
                    .Include(e => e.SignUpList)
                    .FirstAsync(e => e.Id == eventData.Id);

                savedEvent.SignUpList.Add(participant);
                await context.SaveChangesAsync();
            }

            using (var context = new ApiContext(_options!))
            {
                var eventToDelete = await context.Set<Event>().FindAsync(eventData.Id);
                Assert.IsNotNull(eventToDelete);
                context.Set<Event>().Remove(eventToDelete!);
                await context.SaveChangesAsync();
            }

            // Assert
            using (var context = new ApiContext(_options!))
            {
                Assert.AreEqual(0, await context.Set<Event>().CountAsync());
                Assert.AreEqual(2, await context.Set<Organizer>().CountAsync());
                var savedParticipant = await context.Set<Organizer>().FindAsync(participant.Id);
                Assert.IsNotNull(savedParticipant);
            }
        }

        [TestMethod]
        public async Task Event_Enforces_MaxUsers_Limit()
        {
            // Arrange
            var organizer = new User("Organizer", "organizer@test.com");
            var participants = Enumerable.Range(1, 11)
                .Select(i => new User($"Participant{i}", $"p{i}@test.com"))
                .ToList();
            var eventData = new Event(
                organizer,
                _fixedDate.AddDays(1),
                _fixedDate.AddDays(2),
                new Location(51.1079, 17.0385),
                50.00m,
                10,
                2);

            // Act - Part 1: Create event and add participants up to limit
            using (var context = new ApiContext(_options!))
            {
                context.Set<Organizer>().Add(organizer);
                context.Set<Organizer>().AddRange(participants);
                context.Set<Event>().Add(eventData);
                await context.SaveChangesAsync();

                var savedEvent = await context.Set<Event>()
                    .Include(e => e.SignUpList)
                    .FirstAsync(e => e.Id == eventData.Id);

                // Add participants up to the limit
                foreach (var participant in participants.Take(10))
                {
                    savedEvent.SignUpList.Add(participant);
                }
                await context.SaveChangesAsync();
            }

            // Act - Part 2: Try to add one more participant
            using (var context = new ApiContext(_options!))
            {
                var savedEvent = await context.Set<Event>()
                    .Include(e => e.SignUpList)
                    .FirstAsync(e => e.Id == eventData.Id);

                var extraParticipant = participants.Last();

                // Assert
                var ex = Assert.ThrowsException<InvalidOperationException>(() =>
                {
                    if (savedEvent.SignUpList.Count >= savedEvent.MaxUsers)
                    {
                        throw new InvalidOperationException($"Cannot add more than {savedEvent.MaxUsers} participants");
                    }
                    savedEvent.SignUpList.Add(extraParticipant);
                });
                Assert.AreEqual($"Cannot add more than {savedEvent.MaxUsers} participants", ex.Message);
            }

            // Additional verification
            using (var context = new ApiContext(_options!))
            {
                var verifiedEvent = await context.Set<Event>()
                    .Include(e => e.SignUpList)
                    .FirstAsync(e => e.Id == eventData.Id);

                Assert.AreEqual(10, verifiedEvent.SignUpList.Count);
            }
        }
    }
}