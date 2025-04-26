using System.Security.Claims;
using ActiLink.Configuration;
using ActiLink.Organizers.BusinessClients;
using ActiLink.Organizers.Users;

namespace ActiLink.Organizers.Authentication.Roles
{
    public class JwtRoleVisitor: IRoleVisitor
    {
        private readonly JwtSettings _jwtSettings;
        public List<Claim> Claims { get; } 
        public JwtRoleVisitor(JwtSettings jwtSettings, List<Claim> claims)
        {
            _jwtSettings = jwtSettings;
            Claims = claims;
        }
        public void VisitBuisnessClient(BusinessClient businessClient)
        {
            Claims.Add(new Claim(ClaimTypes.Role, _jwtSettings.Roles.BusinessClientRole));
        }
        public void VisitUser(User user)
        {
            Claims.Add(new Claim(ClaimTypes.Role, _jwtSettings.Roles.UserRole));
        }
    }
}
