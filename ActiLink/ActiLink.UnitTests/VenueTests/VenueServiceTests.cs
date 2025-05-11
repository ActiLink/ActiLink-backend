using ActiLink.Organizers.BusinessClients;
using ActiLink.Shared.Model;
using ActiLink.Shared.Repositories;
using ActiLink.Shared.ServiceUtils;
using ActiLink.UnitTests.EventTests;
using ActiLink.Venues;
using ActiLink.Venues.Service;
using AutoMapper;
using Moq;

namespace ActiLink.UnitTests.VenueTests
{
    [TestClass]
    public class VenueServiceTests
    {
        private Mock<IUnitOfWork> _unitOfWorkMock = null!;
        private Mock<IRepository<Venue>> _venueRepositoryMock = null!;
        private Mock<IRepository<BusinessClient>> _businessClientRepositoryMock = null!;
        private Mock<IMapper> _mapperMock = null!;
        private VenueService _venueService = null!;


        const string businessClientId = "TestBCId";
        readonly Guid venueId = new("030B4A82-1B7C-11CF-9D53-00AA003C9CB6");
        const string venueName = "Test Venue";
        const string venueDescription = "Test Description";
        const string venueAddress = "Test Address";
        readonly Location venueLocation = new(1.0, 2.0);


        [TestInitialize]
        public void Setup()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _venueRepositoryMock = new Mock<IRepository<Venue>>();
            _businessClientRepositoryMock = new Mock<IRepository<BusinessClient>>();
            _mapperMock = new Mock<IMapper>();
            _unitOfWorkMock.Setup(uow => uow.VenueRepository).Returns(_venueRepositoryMock.Object);
            _unitOfWorkMock.Setup(uow => uow.BusinessClientRepository).Returns(_businessClientRepositoryMock.Object);
            _venueService = new VenueService(_unitOfWorkMock.Object, _mapperMock.Object);
        }

        [TestMethod]
        public async Task CreateVenueByBusinessClientAsync_Success_ReturnsSuccessResult()
        {
            // Given
            var owner = new BusinessClient("Test Owner", "testowner@email.com", "PL1234567890") { Id = businessClientId };
            var createVenueObject = new CreateVenueObject(businessClientId, venueName, venueDescription, venueLocation, venueAddress);
            var venue = new Venue(owner, venueName, venueDescription, venueLocation, venueAddress);
            Utils.SetupVenueId(venue, venueId);

            _businessClientRepositoryMock.Setup(repo => repo.GetByIdAsync(businessClientId)).ReturnsAsync(owner);
            _mapperMock.Setup(m => m.Map(createVenueObject, It.IsAny<Action<IMappingOperationOptions<object, Venue>>>()))
                .Returns(venue);

            _venueRepositoryMock.Setup(repo => repo.AddAsync(venue)).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync()).ReturnsAsync(1);

            // When
            var result = await _venueService.CreateVenueAsync(createVenueObject);

            // Then
            Assert.IsTrue(result.Succeeded);
            var createdVenue = result.Data;
            Assert.IsNotNull(createdVenue);
            Assert.AreEqual(venueId, createdVenue.Id);
            Assert.AreEqual(venueName, createdVenue.Name);
            Assert.AreEqual(venueDescription, createdVenue.Description);
            Assert.AreEqual(venueLocation, createdVenue.Location);
            Assert.AreEqual(venueAddress, createdVenue.Address);
            Assert.AreEqual(owner, createdVenue.Owner);
            Assert.IsTrue(createdVenue.Events.Count == 0);
            _unitOfWorkMock.Verify(uow => uow.VenueRepository.AddAsync(It.IsAny<Venue>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once);
            _businessClientRepositoryMock.Verify(repo => repo.GetByIdAsync(businessClientId), Times.Once);
            _mapperMock.Verify(m => m.Map(createVenueObject, It.IsAny<Action<IMappingOperationOptions<object, Venue>>>()), Times.Once);
        }

        [TestMethod]
        public async Task CreateVenueByBusinessClientAsync_Failure_UserNotFound()
        {
            // Given
            var createVenueObject = new CreateVenueObject(businessClientId, venueName, venueDescription, venueLocation, venueAddress);
            _businessClientRepositoryMock.Setup(repo => repo.GetByIdAsync(businessClientId)).ReturnsAsync((BusinessClient?)null);

            // When
            var result = await _venueService.CreateVenueAsync(createVenueObject);

            // Then
            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual(ErrorCode.NotFound, result.ErrorCode);
            Assert.AreEqual("User not found", result.Errors.First());
            _unitOfWorkMock.Verify(uow => uow.BusinessClientRepository.GetByIdAsync(businessClientId), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.VenueRepository.AddAsync(It.IsAny<Venue>()), Times.Never);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Never);
        }

        [TestMethod]
        public async Task GetVenueById_Success_ReturnsVenue()
        {
            // Given
            var owner = new BusinessClient("Test Owner", "testowner@email.com", "PL1234567890") { Id = businessClientId };
            var venue = new Venue(owner, venueName, venueDescription, venueLocation, venueAddress);
            Utils.SetupVenueId(venue, venueId);

            _venueRepositoryMock
                .Setup(repo => repo.Query())
                .Returns(new TestAsyncEnumerable<Venue>([venue]));

            // When
            var result = await _venueService.GetVenueByIdAsync(venueId);

            // Then
            Assert.IsNotNull(result);
            Assert.AreEqual(venue, result);
            Assert.AreEqual(venueId, result.Id);
            Assert.AreEqual(venueName, result.Name);
            Assert.AreEqual(venueDescription, result.Description);
            Assert.AreEqual(venueLocation, result.Location);
            Assert.AreEqual(venueAddress, result.Address);
            Assert.AreEqual(owner, result.Owner);
            Assert.IsTrue(result.Events.Count == 0);
            _unitOfWorkMock.Verify(uow => uow.VenueRepository.Query(), Times.Once);
        }

        [TestMethod]
        public async Task GetVenueById_Failure_VenueNotFound()
        {
            // Given
            _venueRepositoryMock
                .Setup(repo => repo.Query())
                .Returns(new TestAsyncEnumerable<Venue>([]));

            // When
            var result = await _venueService.GetVenueByIdAsync(venueId);

            // Then
            Assert.IsNull(result);
            _unitOfWorkMock.Verify(uow => uow.VenueRepository.Query(), Times.Once);
        }

        [TestMethod]
        public async Task GetAllVenues_Success_ReturnsListOfVenues()
        {
            // Given
            var venue1Id = venueId;
            var venue2Id = new Guid("030B4A82-1B7C-11CF-9D53-00AA003C9CB7");
            var owner = new BusinessClient("Test Owner", "testowner@email.com", "PL1234567890") { Id = businessClientId };
            var venue1 = new Venue(owner, venueName, venueDescription, venueLocation, venueAddress);
            var venue2 = new Venue(owner, "Another Venue", "Another Description", new Location(3.0, 4.0), "Another Address");
            Utils.SetupVenueId(venue1, venue1Id);
            Utils.SetupVenueId(venue2, venue2Id);

            List<Venue> venues = [venue1, venue2];
            _venueRepositoryMock
                .Setup(repo => repo.Query())
                .Returns(new TestAsyncEnumerable<Venue>(venues));

            // When
            var result = await _venueService.GetAllVenuesAsync();
            var venuesList = result.ToList();

            // Then
            CollectionAssert.AreEqual(venues, venuesList);
        }

        [TestMethod]
        public async Task GetAllVenues_Failure_NoVenuesFound()
        {
            // Given
            _venueRepositoryMock
                .Setup(repo => repo.Query())
                .Returns(new TestAsyncEnumerable<Venue>([]));

            // When
            var result = await _venueService.GetAllVenuesAsync();
            var venuesList = result.ToList();

            // Then
            Assert.AreEqual(0, venuesList.Count);
            _unitOfWorkMock.Verify(uow => uow.VenueRepository.Query(), Times.Once);
        }
    }
}
