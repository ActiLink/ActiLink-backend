using ActiLink.Organizers.BusinessClients;
using ActiLink.Organizers.Users;

namespace ActiLink.Organizers.Authentication.Roles
{
    public interface IRoleVisitor
    {
        void VisitUser(User user);
        void VisitBusinessClient(BusinessClient businessClient);
    }
}
