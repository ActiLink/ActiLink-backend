using ActiLink.Model;
using Microsoft.EntityFrameworkCore;

namespace ActiLink
{
    internal class ApiContext : DbContext
    {
        DbSet<Organizer> Organizers { get; set; }
        DbSet<Hobby> Hobbies { get; set; }

        public ApiContext(DbContextOptions<ApiContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Organizer>().
                HasDiscriminator<string>("OrganizerType").
                HasValue<User>("User");
        }
    }
}
