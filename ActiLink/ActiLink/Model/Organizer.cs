using Microsoft.AspNetCore.Identity;

namespace ActiLink.Model
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
