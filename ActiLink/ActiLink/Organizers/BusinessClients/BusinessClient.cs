using ActiLink.Organizers.Authentication.Roles;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

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
        public override void AcceptRoleVisitor(IRoleVisitor visitor, List<Claim> claims)
        {
            visitor.VisitBuisnessClient(this, claims);
        }

    }

}
