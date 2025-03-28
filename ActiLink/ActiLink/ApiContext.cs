using ActiLink.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ActiLink
{
    internal class ApiContext : IdentityDbContext<Organizer>
    {
        //DbSet<Organizer> Organizers { get; set; }
        DbSet<Hobby> Hobbies { get; set; }

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
