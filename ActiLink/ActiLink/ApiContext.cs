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

            modelBuilder.Entity<Event>(e =>
            {
                e.OwnsOne(x => x.Location, l =>
                {
                    l.Property(p => p.Longitude).HasColumnName("Longitude");
                    l.Property(p => p.Latitude).HasColumnName("Latitude");
                });

                e.HasOne(ev => ev.Organizer)
                            .WithMany(o => o.Events)
                            .IsRequired()
                            .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
