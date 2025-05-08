using ActiLink.Organizers.BusinessClients;
using ActiLink.Organizers.Users;
using ActiLink.Venues.DTOs;
using ActiLink.Venues.Service;
using AutoMapper;

namespace ActiLink.Venues.Infrastructure
{
    public class VenueProfile : Profile
    {
        public VenueProfile()
        {
            CreateMap<NewVenueDto, CreateVenueObject>()
                .ForMember(dest => dest.OwnerId, opt => opt.MapFrom((src, dest, _, context) =>
                {
                    if(!context.Items.TryGetValue("OwnerId", out var ownerId))
                        throw new InvalidOperationException("OwnerId must be provided in context items");

                    return ownerId as string
                        ?? throw new InvalidOperationException("OwnerId must be of type string");
                }));

            CreateMap<CreateVenueObject, Venue>()
                .ForMember(dest => dest.Owner, opt => opt.MapFrom((src, dest, _, context) =>
                {
                    if(!context.Items.TryGetValue("Owner", out var owner))
                        throw new InvalidOperationException("Owner must be provided in context items");

                    return owner as BusinessClient
                        ?? throw new InvalidOperationException("Owner must be of type BusinessClient");
                }))
                .ForMember(dest => dest.Events, opt => opt.Ignore());

            CreateMap<Venue, VenueDto>();

            CreateMap<UpdateVenueDto, UpdateVenueObject>();
            CreateMap<UpdateVenueObject, Venue>()
                .ForMember(dest => dest.Owner, opt => opt.Ignore())
                .ForMember(dest => dest.Events, opt => opt.Ignore());
        }
    }
}
