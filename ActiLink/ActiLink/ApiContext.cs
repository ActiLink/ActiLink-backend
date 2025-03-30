using ActiLink.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ActiLink
{
    /// <summary>
    /// Database context for the API
    /// </summary>
    public class ApiContext : IdentityDbContext<Organizer>
    {
        //DbSet<Organizer> Organizers { get; set; }
        public DbSet<Hobby> Hobbies { get; set; }

        public ApiContext(DbContextOptions<ApiContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Organizer>()
                .HasDiscriminator<string>("OrganizerType")
                .HasValue<User>("User");
        }
    }
}
