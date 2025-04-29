using ActiLink.Hobbies.DTOs;

namespace ActiLink.Organizers.Users.DTOs
{
    public record UserDetailsDto(string Id, string Name, string Email, IEnumerable<HobbyDto> Hobbies)
    {
        public UserDetailsDto() : this(default!, default!, default!, []) { }
    }
}
