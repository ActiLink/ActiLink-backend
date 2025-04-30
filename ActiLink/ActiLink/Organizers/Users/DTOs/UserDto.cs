namespace ActiLink.Organizers.Users.DTOs
{
    /// <summary>
    /// Data transfer object for a user
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="Name"></param>
    /// <param name="Email"></param>
    public record UserDto(string Id, string Name)
    {
        private UserDto() : this(default!, default!) { }
    }
}
