using Microsoft.AspNetCore.Identity;

namespace ActiLink.Model
{
    public abstract class Organizer : IdentityUser
    {
        protected Organizer(string username, string email) : base(username)
        {
            Email = email;
        }
    }
}
