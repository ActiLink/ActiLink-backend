namespace ActiLink.Organizers.DTOs
{
    /// <summary>
    /// Data transfer object for requesting access token refresh
    /// </summary>
    /// <param name="RefreshToken"></param>
    public record RefreshTokenDto(string RefreshToken);
}
