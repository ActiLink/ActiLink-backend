using ActiLink.Organizers.BusinessClients;
using ActiLink.Organizers.BusinessClients.DTOs;
using ActiLink.Organizers.BusinessClients.Infrastructure;
using ActiLink.Shared.Model;
using ActiLink.Venues;
using ActiLink.Venues.DTOs;
using ActiLink.Venues.Infrastructure;
using ActiLink.Venues.Service;
using AutoMapper;

namespace ActiLink.UnitTests.VenueTests
{
    [TestClass]
    public class VenueMapperTests
    {
        private Mapper _mapper = null!;

        private const string username = "testuser";
        private const string email = "testuser@email.com";
        private const string taxId = "106-00-00-062";
        private const string ownerId = "030B4A82-1B7C-11CF-9D53-00AA003C9CB6";
        private readonly Guid venueId = new("15F7F2EE-0AF5-40A4-B89B-27F807033204");
        private const string venueName = "Test Venue";
        private const string venueDescription = "Test Description";
        private readonly Location venueLocation = new(52.23196613562989, 21.00593062698127);
        private const string venueAddress = "123 Test St";

        [TestInitialize]
        public void Setup()
        {
            _mapper = new Mapper(new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new VenueProfile());
                cfg.AddProfile(new BusinessClientProfile());
            }
                ));
        }


        [TestMethod]
        public void MapNewVenueDtoToCreateVenueObject_ShouldMapCorrectly()
        {
            // Given
            var newVenueDto = new NewVenueDto(venueName, venueDescription, venueLocation, venueAddress);
            var expectedCreateVenueObject = new CreateVenueObject(ownerId, venueName, venueDescription, venueLocation, venueAddress);

            // When
            var createVenueObject = _mapper.Map<CreateVenueObject>(newVenueDto, opt => opt.Items["OwnerId"] = ownerId);

            // Then
            Assert.IsNotNull(createVenueObject);
            Assert.AreEqual(expectedCreateVenueObject, createVenueObject);
        }

        [TestMethod]
        public void MapUpdateVenueDtoToUpdateVenueObject_ShouldMapCorrectly()
        {
            // Given
            var updateVenueDto = new UpdateVenueDto(venueName, venueDescription, venueLocation, venueAddress);
            var expectedUpdateVenueObject = new UpdateVenueObject(venueName, venueDescription, venueLocation, venueAddress);

            // When
            var updateVenueObject = _mapper.Map<UpdateVenueObject>(updateVenueDto);

            // Then
            Assert.IsNotNull(updateVenueObject);
            Assert.AreEqual(expectedUpdateVenueObject, updateVenueObject);
        }

        [TestMethod]
        public void MapCreateVenueObjectToVenue_ShouldMapCorrectly()
        {
            // Given
            var createVenueObject = new CreateVenueObject(ownerId, venueName, venueDescription, venueLocation, venueAddress);
            var owner = new BusinessClient(username, email, taxId);
            var expectedVenue = new Venue(owner, venueName, venueDescription, venueLocation, venueAddress);

            // When
            var mappedVenue = _mapper.Map<Venue>(createVenueObject, opts => opts.Items["Owner"] = owner);

            // Then
            Assert.IsNotNull(mappedVenue);
            Assert.AreEqual(expectedVenue.Owner, mappedVenue.Owner);
            Assert.AreEqual(expectedVenue.Name, mappedVenue.Name);
            Assert.AreEqual(expectedVenue.Description, mappedVenue.Description);
            Assert.AreEqual(expectedVenue.Location, mappedVenue.Location);
            Assert.AreEqual(expectedVenue.Address, mappedVenue.Address);
            Assert.AreEqual(0, mappedVenue.Events.Count);
        }


        [TestMethod]
        public void MapUpdateVenueObjectToVenue_ShouldMapCorrectly()
        {
            // Given
            var newVenueName = "Updated Venue";
            var newVenueDescription = "Updated Description";
            var newVenueLocation = new Location(52.23196613562989, 21.00593062698127);
            var newVenueAddress = "456 Updated St";

            var updateVenueObject = new UpdateVenueObject(newVenueName, newVenueDescription, newVenueLocation, newVenueAddress);
            var owner = new BusinessClient(username, email, taxId);
            var venue = new Venue(owner, venueName, venueDescription, venueLocation, venueAddress);

            // When
            _mapper.Map(updateVenueObject, venue);

            // Then
            Assert.IsNotNull(venue);
            Assert.AreEqual(newVenueName, venue.Name);
            Assert.AreEqual(newVenueDescription, venue.Description);
            Assert.AreEqual(newVenueLocation, venue.Location);
            Assert.AreEqual(newVenueAddress, venue.Address);
            Assert.AreEqual(owner, venue.Owner);
            Assert.AreEqual(0, venue.Events.Count);
        }

        [TestMethod]
        public void MapVenueToVenueDto_ShouldMapCorrectly()
        {
            // Given
            var owner = new BusinessClient(username, email, taxId) { Id = ownerId };
            var ownerDto = new VenueOwnerDto(ownerId, username);
            var venue = new Venue(owner, venueName, venueDescription, venueLocation, venueAddress);
            Utils.SetupVenueId(venue, venueId);

            var expectedVenueDto = new VenueDto(venueId, venueName, venueDescription, venueLocation, venueAddress, ownerDto, []);

            // When
            var mappedVenueDto = _mapper.Map<VenueDto>(venue);

            // Then
            Assert.IsNotNull(mappedVenueDto);
            Assert.AreEqual(expectedVenueDto.Id, mappedVenueDto.Id);
            Assert.AreEqual(expectedVenueDto.Name, mappedVenueDto.Name);
            Assert.AreEqual(expectedVenueDto.Description, mappedVenueDto.Description);
            Assert.AreEqual(expectedVenueDto.Location.Longitude, mappedVenueDto.Location.Longitude);
            Assert.AreEqual(expectedVenueDto.Location.Latitude, mappedVenueDto.Location.Latitude);
            Assert.AreEqual(expectedVenueDto.Address, mappedVenueDto.Address);
            Assert.AreEqual(expectedVenueDto.Owner, mappedVenueDto.Owner);
            CollectionAssert.AreEqual(expectedVenueDto.Events, mappedVenueDto.Events);
        }
    }
}
