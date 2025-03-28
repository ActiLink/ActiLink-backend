namespace ActiLink.DTOs
{
    public record UserDto(string Id, string Name, string Email)
    {
        public UserDto() : this(default!, default!, default!) { }
    }
}
