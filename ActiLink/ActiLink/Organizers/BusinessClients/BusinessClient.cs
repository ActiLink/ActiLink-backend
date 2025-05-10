using System.ComponentModel.DataAnnotations;
using ActiLink.Organizers.Authentication.Roles;
using ActiLink.Venues;

namespace ActiLink.Organizers.BusinessClients
{
    public class BusinessClient : Organizer
    {
        public BusinessClient(string userName, string email, string taxId) : base(userName, email)
        {
            TaxId = taxId;
        }

        [MaxLength(20)]
        public string TaxId { get; set; } = string.Empty;

        public ICollection<Venue> Venues { get; private set; } = [];
        public override void AcceptRoleVisitor(IRoleVisitor visitor)
        {
            visitor.VisitBuisnessClient(this);
        }

    }

}
