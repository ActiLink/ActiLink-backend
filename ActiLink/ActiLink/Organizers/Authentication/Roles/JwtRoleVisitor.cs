using System.Security.Claims;
using ActiLink.Configuration;
using ActiLink.Organizers.BusinessClients;
using ActiLink.Organizers.Users;

namespace ActiLink.Organizers.Authentication.Roles
{
    public class JwtRoleVisitor: IRoleVisitor
    {
        private readonly JwtSettings _jwtSettings;
        public JwtRoleVisitor(JwtSettings jwtSettings)
        {
            _jwtSettings = jwtSettings;
        }
        public void VisitBuisnessClient(BusinessClient businessClient, List<Claim> claims)
        {
            claims.Add(new Claim(ClaimTypes.Role, _jwtSettings.Roles.BusinessClientRole));
        }
        public void VisitUser(User user, List<Claim> claims)
        {
            claims.Add(new Claim(ClaimTypes.Role, _jwtSettings.Roles.UserRole));
        }
    }
}
