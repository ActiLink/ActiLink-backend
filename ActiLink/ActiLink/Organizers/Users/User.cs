using ActiLink.Events;
using ActiLink.Hobbies;

namespace ActiLink.Organizers.Users
{
    /// <summary>
    /// Represents a user
    /// </summary>
    public class User : Organizer
    {

        public ICollection<Hobby> Hobbies { get; } = [];
        public ICollection<Event> SignedUpEvents { get; private set; } = [];

        public User(string userName, string email) : base(userName, email) { }
    }
}
