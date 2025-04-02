namespace ActiLink.DTOs
{
    /// <summary>
    /// Data transfer object for an event
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="Name"></param>
    /// <param name="Date"></param>
    /// <param name="Location"></param>
    /// <param name="OrganizerId"></param>
    /// <param name="HobbyId"></param>
    public record EventDto(string Id, string Name, DateTime Date, string Location, string OrganizerId, string HobbyId)
    {
        public EventDto() : this(default!, default!, default!, default!, default!, default!) { }
    }
}
