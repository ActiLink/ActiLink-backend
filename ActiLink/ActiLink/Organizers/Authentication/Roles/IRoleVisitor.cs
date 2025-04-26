using System.Security.Claims;
using ActiLink.Organizers.Users;
using ActiLink.Organizers.BusinessClients;

namespace ActiLink.Organizers.Authentication.Roles
{
    public interface IRoleVisitor
    {
        void VisitUser(User user);
        void VisitBuisnessClient(BusinessClient businessClient);
    }
}
