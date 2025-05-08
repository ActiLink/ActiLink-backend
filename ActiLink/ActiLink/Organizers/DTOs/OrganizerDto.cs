namespace ActiLink.Organizers.DTOs
{
    public record OrganizerDto(string Id, string Name)
    {
        public OrganizerDto() : this(default!, default!) { }
    }
}
