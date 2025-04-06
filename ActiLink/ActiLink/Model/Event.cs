using Microsoft.EntityFrameworkCore;

namespace ActiLink.Model
{
    /// <summary>
    /// Represents an event in the system.
    /// </summary>
    public class Event
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Organizer Organizer { get; private set; } = null!;
        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }
        public Location Location { get; private set; } = new Location(0, 0);
        [Precision(10, 2)]
        public decimal Price { get; private set; }
        public int MinUsers { get; private set; }
        public int MaxUsers { get; private set; }
        public ICollection<User> SignUpList { get; private set; } = [];
        public ICollection<Hobby> RelatedHobbies { get; private set; } = [];

        /// <summary>
        /// Constructor for creating an event.
        /// </summary>
        private Event() { }
        public Event(Organizer organizer, DateTime startTime, DateTime endTime, Location location,
                     decimal price, int minUsers, int maxUsers)
        {
            Organizer = organizer;
            StartTime = startTime;
            EndTime = endTime;
            Location = location;
            Price = price;
            MinUsers = minUsers;
            MaxUsers = maxUsers;
        }
    }
}
