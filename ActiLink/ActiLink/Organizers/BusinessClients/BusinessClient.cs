namespace ActiLink.Organizers.BusinessClients
{
    public class BusinessClient : Organizer
    {

        //public BusinessClient(string username, string email) : base(username, email) { }
        public BusinessClient(string userName, string email, string taxId) : base(userName, email)
        {
            TaxId = taxId;
        }

        public string TaxId { get; set; } = string.Empty;

    }

}
