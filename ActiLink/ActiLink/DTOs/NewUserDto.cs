namespace ActiLink.DTOs
{
    /// <summary>
    /// Data transfer object for creating a new user
    /// </summary>
    /// <param name="Name"></param>
    /// <param name="Email"></param>
    /// <param name="Password"></param>
    public record NewUserDto(string Name, string Email, string Password);
}
