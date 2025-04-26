namespace ActiLink.Configuration
{
    public class JwtSettings
    {
        public int AccessTokenExpiryMinutes { get; set; }
        public int RefreshTokenExpiryDays { get; set; }
        public RoleNames Roles { get; set; } = new();

        public class RoleNames
        {
            public string UserRole { get; set; } = string.Empty;
            public string BusinessClientRole { get; set; } = string.Empty;
        }
    }
}
