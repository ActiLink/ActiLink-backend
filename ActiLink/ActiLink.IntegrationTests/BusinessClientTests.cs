using ActiLink.Organizers.BusinessClients;
using Microsoft.EntityFrameworkCore;

namespace ActiLink.IntegrationTests
{
    [TestClass]
    public class BusinessClientTests
    {
        const string userName = "TestUser";
        const string email = "testuser@email.com";
        const string taxId = "106-00-00-062";
        private DbContextOptions<ApiContext> GetInMemoryOptions()
        {
            return new DbContextOptionsBuilder<ApiContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase_" + Guid.NewGuid())
                .Options;
        }

        [TestMethod]
        public async Task Test_AddBusinessClient_ShouldInsertRecord()
        {
            // Configuration of in-memory database
            var options = GetInMemoryOptions();

            // Insert a new record
            using (var context = new ApiContext(options))
            {
                var client = new BusinessClient(userName, email, taxId);
                context.Users.Add(client);
                await context.SaveChangesAsync();
            }

            // Verification of the inserted record
            using (var context = new ApiContext(options))
            {
                var client = context.Users.FirstOrDefault(u => u.UserName == userName) as BusinessClient;
                Assert.IsNotNull(client);
                Assert.AreEqual(userName, client.UserName);
                Assert.AreEqual(email, client.Email);
                Assert.AreEqual(taxId, client.TaxId);
            }
        }

        [TestMethod]
        public async Task Test_UpdateBusinessClient_ShouldPersistChanges()
        {
            var options = GetInMemoryOptions();

            string clientId;
            string updatedUserName = "UpdatedUser";

            // Insert a record to update
            using (var context = new ApiContext(options))
            {
                var client = new BusinessClient(userName, email, taxId);
                context.Users.Add(client);
                await context.SaveChangesAsync();
                clientId = client.Id;
            }

            // Update the record
            using (var context = new ApiContext(options))
            {
                var client = context.Users.Find(clientId);
                Assert.IsNotNull(client);
                client.UserName = updatedUserName;
                await context.SaveChangesAsync();
            }

            // Verification of the update
            using (var context = new ApiContext(options))
            {
                var client = context.Users.Find(clientId);
                Assert.IsNotNull(client);
                Assert.AreEqual(updatedUserName, client.UserName);
            }
        }

        [TestMethod]
        public async Task Test_DeleteBusinessClient_ShouldRemoveRecord()
        {
            var options = GetInMemoryOptions();

            string clientId;

            // Insert a record to delete
            using (var context = new ApiContext(options))
            {
                var client = new BusinessClient(userName, email, taxId);
                context.Users.Add(client);
                await context.SaveChangesAsync();
                clientId = client.Id;
            }

            // Delete the record
            using (var context = new ApiContext(options))
            {
                var client = context.Users.Find(clientId);
                Assert.IsNotNull(client);
                context.Users.Remove(client);
                await context.SaveChangesAsync();
            }

            // Verification of the deletion
            using (var context = new ApiContext(options))
            {
                var client = context.Users.Find(clientId);
                Assert.IsNull(client);
            }
        }
    }
}
