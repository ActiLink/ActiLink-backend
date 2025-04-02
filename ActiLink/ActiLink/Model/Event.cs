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
        public Guid OrganizerId { get; private set; }
        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }
        public Location Location { get; private set; }
        public SqlMoney Price { get; private set; }
        public int MaxUsers { get; private set; }
        public int MinUsers { get; private set; }
        public ICollection<User> SignUpList { get; } = new List<User>();
        public ICollection<Hobby> RelatedHobbies { get; } = new List<Hobby>();

        /// <summary>
        /// Constructor for creating an event.
        /// </summary>
        public Event(Guid organizerId, DateTime startTime, DateTime endTime, Location location,
                     SqlMoney price, int maxUsers, int minUsers)
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
                                  SqlMoney price, int maxUsers, int minUsers)
        {
            StartTime = startTime;
            EndTime = endTime;
            Location = location;
            Price = price;
            MaxUsers = maxUsers;
            MinUsers = minUsers;
        }
    }
}
