using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ActiLink.Hobbies;
using ActiLink.Hobbies.DTOs;
using ActiLink.Hobbies.Infrastructure;
using ActiLink.Hobbies.Service;
using ActiLink.Shared.Repositories;
using Moq;

namespace ActiLink.UnitTests.HobbyTests
{
    [TestClass]
    public class HobbyServiceTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork = null!;
        private HobbyService _hobbyService = null!;
        //private readonly Guid id = new("030B4A82-1B7C-11CF-9D53-00AA003C9CB6");

        [TestInitialize]
        public void Setup()
        {
            // Initialize the mock unit of work and the hobby service
            _mockUnitOfWork = new Mock<IUnitOfWork>();

            // Initialize the BusinessClientService with mocked dependencies
            _hobbyService = new HobbyService(_mockUnitOfWork.Object);
        }

        [TestMethod]
        public async Task GetHobbiesAsync_ShouldReturnAllHobbies()
        {
            // Given
            string name1 = "Hobby1";
            string name2 = "Hobby2";
            var hobbies = new List<Hobby>
            {
                new(name1),
                new(name2)
            };

            _mockUnitOfWork.Setup(uow => uow.HobbyRepository.GetAllAsync())
                .ReturnsAsync(hobbies);

            // When
            var result = await _hobbyService.GetHobbiesAsync();

            // Then
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual(hobbies.First(), result.First());
            Assert.AreEqual(hobbies.Last(), result.Last());
            _mockUnitOfWork.Verify(uow => uow.HobbyRepository.GetAllAsync(), Times.Once);
        }

        [TestMethod]
        public async Task GetHobbiesAsync_ShouldReturnEmptyList()
        {
            // Given
            _mockUnitOfWork.Setup(uow => uow.HobbyRepository.GetAllAsync())
                .ReturnsAsync([]);

            // When
            var result = await _hobbyService.GetHobbiesAsync();

            // Then
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
            _mockUnitOfWork.Verify(uow => uow.HobbyRepository.GetAllAsync(), Times.Once);
        }
    }
}
