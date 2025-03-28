namespace ActiLink.Model
{
    public class User : Organizer
    {

        public ICollection<Hobby> Hobbies { get; } = [];

        public User(string userName, string email) : base(userName, email) { }
    }
}
