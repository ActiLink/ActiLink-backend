namespace ActiLink.Organizers.Users.Service
{
    public record UpdateUserObject(string Name, string Email, ICollection<string> HobbyNames)
    {
        private UpdateUserObject() : this(default!, default!, new List<string>()) { }
    }
}
