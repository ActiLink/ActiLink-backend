using Microsoft.AspNetCore.Hosting;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ActiLink.IntegrationTests
{
    internal class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        public IServiceProvider TestServices { get; private set; } = null!;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            Environment.SetEnvironmentVariable("JWT_SECRET_KEY", "super_secret_test_key_12345678910");
            Environment.SetEnvironmentVariable("JWT_VALID_ISSUER", "TestIssuer");
            Environment.SetEnvironmentVariable("JWT_VALID_AUDIENCE", "TestAudience");

            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {

                services.AddDbContext<ApiContext>(options =>
                {
                    options.UseInMemoryDatabase("AuthorizationTestDb_" + Guid.NewGuid());
                });

                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApiContext>();
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                TestServices = sp;
            });
        }
    }
}
