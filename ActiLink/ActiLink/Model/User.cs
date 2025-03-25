namespace ActiLink.Model
{
    public class User : Organizer
    {
        public ICollection<Hobby> Hobbies { get; } = [];
    }
}
