using ActiLink.Organizers.Users;
using ActiLink.Organizers.Users.DTOs;
using AutoMapper;

namespace ActiLink.Organizers.Users.Infrastructure
{
    /// <summary>
    /// Profile for mapping user objects
    /// </summary>
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<NewUserDto, User>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Name));

            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.UserName));
        }
    }
}
