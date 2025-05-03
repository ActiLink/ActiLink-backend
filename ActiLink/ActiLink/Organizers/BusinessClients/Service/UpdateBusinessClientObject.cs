namespace ActiLink.Organizers.BusinessClients.Service
{
    public record UpdateBusinessClientObject(string Name, string Email, string TaxId)
    {
        private UpdateBusinessClientObject() : this(default!, default!, default!) { }
    }
}
