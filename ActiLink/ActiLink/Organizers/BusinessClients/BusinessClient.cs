namespace ActiLink.Organizers.BusinessClients
{
    public class BusinessClient : Organizer
    {
        public BusinessClient(string username, string email, string taxId) : base(username, email)
        {
            TaxId = taxId;
        }

        public string TaxId { get; set; }

    }

}
