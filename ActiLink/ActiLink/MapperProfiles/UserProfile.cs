using ActiLink.DTOs;
using ActiLink.Model;
using AutoMapper;

namespace ActiLink.MapperProfiles
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
