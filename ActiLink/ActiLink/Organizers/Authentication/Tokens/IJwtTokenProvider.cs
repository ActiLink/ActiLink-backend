namespace ActiLink.Organizers.Authentication.Tokens
{
    public interface IJwtTokenProvider
    {
        string GenerateAccessToken(Organizer user);
        string GenerateRefreshToken(string userId);
    }
}
