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
                .AfterMap((src, dest, context) =>
                {
                    var ownerId = context.Items["OwnerId"] as string
                        ?? throw new InvalidOperationException("OwnerId must be provided in context items");

                    dest.OwnerId = ownerId;
                });

            CreateMap<CreateVenueObject, Venue>()
                .ForMember(dest => dest.Owner, opt => opt.Ignore())
                .ForMember(dest => dest.Events, opt => opt.Ignore())
                .AfterMap((src, dest, context) =>
                {
                    // Owner
                    if (context.Items.TryGetValue("Owner", out var ownerObj) &&
                        ownerObj is BusinessClient owner)
                    {
                        typeof(Venue)
                            .GetProperty(nameof(Venue.Owner))?
                            .SetValue(dest, owner);
                    }
                });

            CreateMap<Venue, VenueDto>();

            CreateMap<UpdateVenueDto, UpdateVenueObject>();
            CreateMap<UpdateVenueObject, Venue>()
                .ForMember(dest => dest.Owner, opt => opt.Ignore())
                .ForMember(dest => dest.Events, opt => opt.Ignore());
        }
    }
}
