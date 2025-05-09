﻿using ActiLink.Events;
using ActiLink.Hobbies;
using ActiLink.Organizers.Authentication.Roles;

namespace ActiLink.Organizers.Users
{
    /// <summary>
    /// Represents a user
    /// </summary>
    public class User : Organizer
    {

        public ICollection<Hobby> Hobbies { get; set; } = [];
        public ICollection<Event> SignedUpEvents { get; private set; } = [];

        public User(string userName, string email) : base(userName, email) { }
        public override void AcceptRoleVisitor(IRoleVisitor visitor)
        {
            visitor.VisitUser(this);
        }
    }
}
