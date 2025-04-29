using ActiLink.Events;
using ActiLink.Hobbies;
using ActiLink.Organizers.Users.DTOs;
using ActiLink.Organizers.Users.Service;
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

            CreateMap<User, UserDetailsDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.UserName));

            CreateMap<UpdateUserDto, UpdateUserObject>()
                .ForMember(dest => dest.HobbyNames, opt => opt.MapFrom(src => src.Hobbies.Select(h => h.Name).ToList()));
        }
    }
}
