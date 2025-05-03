namespace ActiLink.Organizers.BusinessClients.DTOs
{
    public record UpdateBusinessClientDto(string Name, string Email, string TaxId)
    {
        private UpdateBusinessClientDto() : this(default!, default!, default!) { }
    }
}
