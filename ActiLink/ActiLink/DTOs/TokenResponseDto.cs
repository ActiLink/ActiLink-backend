namespace ActiLink.DTOs
{
    /// <summary>
    /// Data transfer object for token response
    /// </summary>
    /// <param name="AccessToken"></param>
    /// <param name="RefreshToken"></param>
    public record TokenResponseDto(string AccessToken, string RefreshToken);
}
