using ActiLink.Hobbies;
using ActiLink.Organizers.Users;
using Microsoft.EntityFrameworkCore;

namespace ActiLink.IntegrationTests
{
    [TestClass]
    public class UserTests
    {
        private DbContextOptions<ApiContext> _options = null!; // Wyraźne oznaczenie jako non-null

        [TestInitialize]
        public void Setup()
        {
            var databaseName = $"TestDatabase_{Guid.NewGuid()}";
            _options = new DbContextOptionsBuilder<ApiContext>()
                .UseInMemoryDatabase(databaseName: databaseName)
                .Options;

            using var context = new ApiContext(_options);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }

        [TestMethod]
        public async Task CanAddUserToDatabase()
        {
            using var context = new ApiContext(_options);
            var user = new User("Janek", "Jan@gmail.com");

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var savedUser = context.Users.FirstOrDefault(u => u.UserName == "Janek");
            Assert.IsNotNull(savedUser);
        }

        [TestMethod]
        public async Task CanAddHobbyToDatabase()
        {
            using var context = new ApiContext(_options);
            var user = new User("Janek", "Jan@gmail.com");
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var hobby = new Hobby("Programming");
            context.Set<Hobby>().Add(hobby);
            await context.SaveChangesAsync();

            var savedHobby = context.Set<Hobby>().FirstOrDefault(h => h.Name == "Programming");
            Assert.IsNotNull(savedHobby);
        }

        [TestMethod]
        public async Task CanUpdateUserInDatabase()
        {
            string userId;
            using (var context = new ApiContext(_options))
            {
                var user = new User("Janek", "Jan@gmail.com");
                context.Users.Add(user);
                await context.SaveChangesAsync();
                userId = user.Id;
            }

            using (var context = new ApiContext(_options))
            {
                var user = await context.Users.FindAsync(userId);
                Assert.IsNotNull(user);

                user!.Email = "UpdatedEmail@gmail.com";
                await context.SaveChangesAsync();
            }

            using (var context = new ApiContext(_options))
            {
                var user = await context.Users.FindAsync(userId);
                Assert.IsNotNull(user);
                Assert.AreEqual("UpdatedEmail@gmail.com", user!.Email);
            }
        }

        [TestMethod]
        public async Task CanFindUserByEmail()
        {
            using (var context = new ApiContext(_options))
            {
                var user = new User("Janek", "Jan@gmail.com");
                context.Users.Add(user);
                await context.SaveChangesAsync();
            }

            using (var context = new ApiContext(_options))
            {
                var user = context.Users.FirstOrDefault(u => u.Email == "Jan@gmail.com");
                Assert.IsNotNull(user);
                Assert.AreEqual("Jan@gmail.com", user!.Email);
            }
        }

        [TestMethod]
        public async Task CanAddHobbyToUser()
        {
            using (var context = new ApiContext(_options))
            {
                var user = new User("Janek", "Jan@gmail.com");
                var hobby = new Hobby("Programming");
                user.Hobbies.Add(hobby);

                await context.Users.AddAsync(user);
                await context.SaveChangesAsync();
            }

            using (var context = new ApiContext(_options))
            {
                var user = await context.Users
                    .OfType<User>()
                    .Include(u => u.Hobbies)
                    .FirstOrDefaultAsync(u => u.UserName == "Janek");

                Assert.IsNotNull(user);
                Assert.AreEqual(1, user!.Hobbies.Count);
                Assert.AreEqual("Programming", user.Hobbies.First().Name);
            }
        }

        [TestMethod]
        public async Task CanDeleteUserWithHobbies()
        {
            string userId = null!;
            Guid hobbyId;

            using (var context = new ApiContext(_options))
            {
                var hobby = new Hobby("Programming");
                var user = new User("Janek", "Jan@gmail.com");
                user.Hobbies.Add(hobby);

                await context.Users.AddAsync(user);
                context.Set<Hobby>().Add(hobby);
                await context.SaveChangesAsync();

                userId = user.Id;
                hobbyId = hobby.Id;
            }

            using (var context = new ApiContext(_options))
            {
                var user = await context.Users
                    .OfType<User>()
                    .Include(u => u.Hobbies)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                Assert.IsNotNull(user);
                context.Users.Remove(user!);
                await context.SaveChangesAsync();
            }

            using (var context = new ApiContext(_options))
            {
                var remainingHobby = await context.Set<Hobby>().FindAsync(hobbyId);
                Assert.IsNotNull(remainingHobby);
                Assert.AreEqual("Programming", remainingHobby!.Name);
            }
        }
    }
}