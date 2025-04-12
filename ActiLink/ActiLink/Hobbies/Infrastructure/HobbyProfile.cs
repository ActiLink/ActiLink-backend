using ActiLink.Hobbies.DTOs;
using AutoMapper;

namespace ActiLink.Hobbies.Infrastructure
{
    public class HobbyProfile : Profile
    {
        public HobbyProfile()
        {
            // Map Hobby to HobbyDto
            CreateMap<Hobby, HobbyDto>();
        }
    }
}
