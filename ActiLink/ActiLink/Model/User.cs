namespace ActiLink.Model
{
    /// <summary>
    /// Represents a user
    /// </summary>
    public class User : Organizer
    {

        public ICollection<Hobby> Hobbies { get; } = [];

        public User(string userName, string email) : base(userName, email) { }
    }
}
