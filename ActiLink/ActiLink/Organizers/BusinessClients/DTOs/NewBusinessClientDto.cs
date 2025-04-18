namespace ActiLink.Organizers.BusinessClients.DTOs
{
    /// <summary>
    /// Data transfer object for creating a new business client
    /// </summary>
    /// <param name="Name"></param>
    /// <param name="Email"></param>
    /// <param name="Password"></param>
    /// <param name="TaxId"></param>
    public record NewBusinessClientDto(string Name, string Email, string Password, string TaxId);
}
