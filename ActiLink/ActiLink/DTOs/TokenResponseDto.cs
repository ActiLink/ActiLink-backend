namespace ActiLink.DTOs
{
    /// <summary>
    /// Data transfer object for token response
    /// </summary>
    /// <param name="AccessToken"></param>
    /// <param name="RefreshToken"></param>
    public record TokenResponse(string AccessToken, string RefreshToken);
}
