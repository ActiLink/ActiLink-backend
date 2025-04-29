using ActiLink.Hobbies.DTOs;

namespace ActiLink.Organizers.Users.DTOs
{
    public record UpdateUserDto(string Name, string Email, ICollection<HobbyDto> Hobbies)
    {
        public UpdateUserDto() : this(default!, default!, []) { }
    }
}
