﻿using Microsoft.AspNetCore.Identity;

namespace ActiLink.Model
{
    /// <summary>
    /// Base class for users and business clients
    /// </summary>
    public abstract class Organizer : IdentityUser
    {
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public ICollection<Event > Events { get; set; } = new List<Event>();
        protected Organizer(string username, string email) : base(username)
        {
            Email = email;
        }
    }
}
