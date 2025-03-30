using ActiLink;
using ActiLink.Model;
using ActiLink.Repositories;
using ActiLink.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ActiLink.UnitTests
{
    [TestClass]
    public class DB_Integration_Tests
    {
        private DbContextOptions<ApiContext> _options;

        [TestInitialize]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<ApiContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new ApiContext(_options))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }
        }

        [TestMethod]
        public async Task CanAddUserToDatabase()
        {
            using (var context = new ApiContext(_options))
            {
                var user = new User("Janek", "Jan@gmail.com");

                context.Users.Add(user);
                await context.SaveChangesAsync();
            }

            using (var context = new ApiContext(_options))
            {
                var user = context.Users.FirstOrDefault(u => u.UserName == "Janek");
                Assert.IsNotNull(user);
            }
        }

        [TestMethod]
        public async Task CanAddHobbyToDatabase()
        {
            using (var context = new ApiContext(_options))
            {
                var user = new User("Janek", "Jan@gmail.com");
                await context.Users.AddAsync(user);
                await context.SaveChangesAsync();

                var hobby = new Hobby("Programming");
                context.Hobbies.Add(hobby);
                await context.SaveChangesAsync();
            }

            using (var context = new ApiContext(_options))
            {
                var hobby = context.Hobbies.FirstOrDefault(h => h.Name == "Programming");
                Assert.IsNotNull(hobby);
            }
        }


        // Test aktualizacji użytkownika
        [TestMethod]
        public async Task CanUpdateUserInDatabase()
        {
            using (var context = new ApiContext(_options))
            {
                var user = new User("Janek", "Jan@gmail.com");
                context.Users.Add(user);
                await context.SaveChangesAsync();
            }

            using (var context = new ApiContext(_options))
            {
                var user = context.Users.FirstOrDefault(u => u.UserName == "Janek");
                user.Email = "UpdatedEmail@gmail.com";
                await context.SaveChangesAsync();
            }

            using (var context = new ApiContext(_options))
            {
                var user = context.Users.FirstOrDefault(u => u.UserName == "Janek");
                Assert.IsNotNull(user);
                Assert.AreEqual("UpdatedEmail@gmail.com", user.Email);
            }
        }

        // Test wyszukiwania użytkownika po e-mailu
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
                Assert.AreEqual("Jan@gmail.com", user.Email);
            }
        }
    }
}
