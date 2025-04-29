namespace ActiLink.Organizers.Users.Service
{
    public record UpdateUserObject(string Id, string Name, string Email, ICollection<string> HobbyNames)
    {
        public UpdateUserObject() : this(default!, default!, default!, new List<string>()) { }
        public string Id { get; set; } = Id;
    }
}
