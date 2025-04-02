using ActiLink.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

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

        public ApiContext(DbContextOptions<ApiContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Organizer>()
                .HasDiscriminator<string>("OrganizerType")
                .HasValue<User>("User");

            // Konfiguracja dla Event
            modelBuilder.Entity<Event>(e =>
            {
                // Konfiguracja Location jako owned entity
                e.OwnsOne(x => x.Location, l =>
                {
                    l.Property(p => p.Height).HasColumnName("Location_Latitude");
                    l.Property(p => p.Width).HasColumnName("Location_Longitude");
                });

                // Relacja z Organizer
                e.HasOne<Organizer>()
                    .WithMany()
                    .HasForeignKey(e => e.OrganizerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
