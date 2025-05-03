using ActiLink.Events;
using ActiLink.Organizers.Authentication.Roles;
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
        public abstract void AcceptRoleVisitor(IRoleVisitor visitor);
    }
}
