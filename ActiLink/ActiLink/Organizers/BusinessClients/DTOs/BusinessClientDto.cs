namespace ActiLink.Organizers.BusinessClients.DTOs
{
    /// <summary>
    /// Data transfer object for a business client
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="Name"></param>
    /// <param name="Email"></param>
    /// <param name="TaxId"></param>
    public record BusinessClientDto(string Id, string Name, string Email, string TaxId)
    {
        public BusinessClientDto() : this(default!, default!, default!, default!) { }
    }
}
