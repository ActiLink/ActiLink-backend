namespace ActiLink.DTOs
{
    /// <summary>
    /// Data transfer object for user login
    /// </summary>
    /// <param name="Email"></param>
    /// <param name="Password"></param>
    public record LoginDto(string Email, string Password);

}
