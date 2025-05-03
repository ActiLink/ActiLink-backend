namespace ActiLink.Hobbies.DTOs
{
    /// <summary>
    /// Data transfer object for Hobby (read operations)
    /// </summary>
    /// <param name="Name">Name of the hobby (max 50 characters)</param>
    public record HobbyDto(
        string Name
    );
}