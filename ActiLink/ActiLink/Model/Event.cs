namespace ActiLink.Model
{
    /// <summary>
    /// Represents an event
    /// </summary>
    public class Event
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public string Location { get; set; }
        public User Organizer { get; set; }
        public Hobby Type { get; set; }
        public ICollection<User> Participants { get; } = new List<User>();

        public Event(string id, string name, DateTime date, string location, User organizer, Hobby type)
        {
            Id = id;
            Name = name;
            Date = date;
            Location = location;
            Organizer = organizer;
            Type = type;
        }
    }
}
