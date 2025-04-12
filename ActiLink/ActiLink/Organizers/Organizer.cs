using ActiLink.Events;
using Microsoft.AspNetCore.Identity;

namespace ActiLink.Organizers
{
    /// <summary>
    /// Base class for users and business clients
    /// </summary>
    public abstract class Organizer : IdentityUser
    {
        public ICollection<Event> Events { get; private set; } = [];
        protected Organizer(string username, string email) : base(username)
        {
            Email = email;
        }
    }
}
