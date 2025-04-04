using Microsoft.SqlServer;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;

namespace ActiLink.Model
{
    /// <summary>
    /// Represents an event in the system.
    /// </summary>
    public class Event
    {
        public Guid Id { get; private set; }
        public string OrganizerId { get; private set; }
        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }
        public Location Location { get; private set; } = new Location(0, 0);
        public decimal Price { get; private set; }
        public int MaxUsers { get; private set; }
        public int MinUsers { get; private set; }
        public ICollection<User> SignUpList { get; } = new List<User>();
        public ICollection<Hobby> RelatedHobbies { get; } = new List<Hobby>();

        /// <summary>
        /// Constructor for creating an event.
        /// </summary>

        public Event(string organizerId, DateTime startTime, DateTime endTime, Location location,
                     decimal price, int maxUsers, int minUsers)
        {
            Id = Guid.NewGuid();
            OrganizerId = organizerId;
            StartTime = startTime;
            EndTime = endTime;
            Location = location;
            Price = price;
            MaxUsers = maxUsers;
            MinUsers = minUsers;
        }

        /// <summary>
        /// Updates the event details.
        /// </summary>
        public void UpdateDetails(DateTime startTime, DateTime endTime, Location location,
                                  decimal price, int maxUsers, int minUsers)
        {
            StartTime = startTime;
            EndTime = endTime;
            Location = location;
            Price = price;
            MaxUsers = maxUsers;
            MinUsers = minUsers;
        }

        public void AddParticipant(User user)
        {
            if (SignUpList.Count >= MaxUsers)
            {
                throw new InvalidOperationException($"Cannot add more than {MaxUsers} participants");
            }
            SignUpList.Add(user);
        }
    }
}
