namespace ActiLink.Organizers.BusinessClients.DTOs
{
    public record VenueOwnerDto(
        string Id,
        string Name
        )
    {
        public VenueOwnerDto() : this(default!, default!) { }
    }
}
