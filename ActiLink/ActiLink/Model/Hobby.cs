using System.ComponentModel.DataAnnotations;

namespace ActiLink.Model
{
    /// <summary>
    /// Represents a hobby
    /// </summary>
    public class Hobby
    {
        public Guid Id { get; private set; } = Guid.NewGuid();

        [MaxLength(50)]
        public string Name { get; private set; } = string.Empty;

        public Hobby(string name)
        {
            Name = name;
        }

        public ICollection<User> Users { get; private set; } = [];
        public ICollection<Event> Events { get; private set; } = [];
    }
}
