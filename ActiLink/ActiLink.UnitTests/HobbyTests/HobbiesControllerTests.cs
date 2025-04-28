using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ActiLink.Hobbies;
using ActiLink.Hobbies.DTOs;
using ActiLink.Hobbies.Service;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace ActiLink.UnitTests.HobbyTests
{
    [TestClass]
    public class HobbiesControllerTests
    {
        private Mock<IHobbyService> _mockHobbyService = null!;
        private Mock<IMapper> _mockMapper = null!;
        private HobbiesController _hobbiesController = null!;


        [TestInitialize]
        public void Setup()
        {
            // Initialize the mock unit of work and the hobby service
            _mockHobbyService = new Mock<IHobbyService>();
            _mockMapper = new Mock<IMapper>();

            // Initialize the BusinessClientController with mocked dependencies
            _hobbiesController = new HobbiesController(Mock.Of<ILogger<HobbiesController>>(), _mockHobbyService.Object, _mockMapper.Object);
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

            var expectedHobbyDtos = new List<HobbyDto>()
            {
                new(name1),
                new(name2)
            };

            _mockHobbyService.Setup(uow => uow.GetHobbiesAsync())
                .ReturnsAsync(hobbies);

            _mockMapper.Setup(m => m.Map<IEnumerable<HobbyDto>>(hobbies))
                .Returns(expectedHobbyDtos);


            // When
            var result = await _hobbiesController.GetHobbiesAsync();

            // Then
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            var hobbyDtos = okResult.Value as IEnumerable<HobbyDto>;
            Assert.IsNotNull(hobbyDtos);
            CollectionAssert.AreEqual(expectedHobbyDtos.ToList(), hobbyDtos.ToList());
            _mockHobbyService.Verify(uow => uow.GetHobbiesAsync(), Times.Once);
            _mockMapper.Verify(m => m.Map<IEnumerable<HobbyDto>>(hobbies), Times.Once);

        }

        [TestMethod]
        public async Task GetHobbiesAsync_ShouldReturnEmptyList()
        {
            // Given
            _mockHobbyService.Setup(uow => uow.GetHobbiesAsync())
                .ReturnsAsync([]);

            // When
            var result = await _hobbiesController.GetHobbiesAsync();

            // Then
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            var hobbyDtos = okResult.Value as IEnumerable<HobbyDto>;
            Assert.IsNotNull(hobbyDtos);
            Assert.AreEqual(0, hobbyDtos.Count());
        }
    }
}
