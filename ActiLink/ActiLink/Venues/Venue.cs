using System.ComponentModel.DataAnnotations;
using ActiLink.Events;
using ActiLink.Organizers.BusinessClients;
using ActiLink.Shared.Model;

namespace ActiLink.Venues
{
    public class Venue
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public BusinessClient Owner { get; private set; } = null!;
        [MaxLength(100)]
        public string Name { get; private set; } = string.Empty;
        [MaxLength(2000)]
        public string Description { get; private set; } = string.Empty;
        public Location Location { get; private set; } = new Location(0, 0);
        [MaxLength(200)]
        public string Address { get; private set; } = string.Empty;
        public ICollection<Event> Events { get; private set; } = [];


        private Venue() { }

        public Venue(BusinessClient owner, string name, string description, Location location, string address)
        {
            Owner = owner;
            Name = name;
            Description = description;
            Location = location;
            Address = address;
        }
    }
}
