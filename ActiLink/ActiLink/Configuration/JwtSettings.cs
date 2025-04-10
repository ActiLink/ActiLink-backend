namespace ActiLink.Configuration
{
    public class JwtSettings
    {
        public int AccessTokenExpiryMinutes { get; set; }
        public int RefreshTokenExpiryDays { get; set; }
    }
}
