using ActiLink.Events;
using ActiLink.Hobbies;
using ActiLink.Organizers;
using ActiLink.Organizers.Authentication;
using ActiLink.Organizers.Users;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ActiLink
{
    /// <summary>
    /// Database context for the API
    /// </summary>
    internal class ApiContext : IdentityDbContext<Organizer>
    {
        //DbSet<Organizer> Organizers { get; set; }
        DbSet<Event> Events { get; set; }  // Added DbSet for Event
        DbSet<Hobby> Hobbies { get; set; }
        DbSet<RefreshToken> RefreshTokens { get; set; } // Added DbSet for RefreshToken

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
