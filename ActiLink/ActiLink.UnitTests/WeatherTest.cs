using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ActiLink.Controllers;
using ActiLink.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace ActiLink.UnitTests
{
    [TestClass]
    public class WeatherForecastControllerTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<ILogger<WeatherForecastController>> _mockLogger;
        private WeatherForecastController _controller;

        [TestInitialize]
        public void Setup()
        {
            // Initialize mocks
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<WeatherForecastController>>();
            _controller = new WeatherForecastController(_mockLogger.Object, _mockUnitOfWork.Object);
        }

        [TestMethod]
        public async Task Get_ReturnsOkResult_WithWeatherForecasts()
        {
            // Arrange: Prepare mock data
            var mockForecasts = new List<WeatherForecast>
            {
                new WeatherForecast { Date = DateOnly.FromDateTime(DateTime.Now), TemperatureC = 20, Summary = "Mild" },
                new WeatherForecast { Date = DateOnly.FromDateTime(DateTime.Now.AddDays(1)), TemperatureC = 22, Summary = "Warm" }
            };

            _mockUnitOfWork.Setup(uow => uow.WeatherForecastRepository.GetAllAsync())
                           .ReturnsAsync(mockForecasts);

            // Act: Call the Get method
            var result = await _controller.Get();

            // Assert: Verify the result is an OkObjectResult and contains the correct data
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var returnValue = okResult.Value as IEnumerable<WeatherForecast>;
            Assert.IsNotNull(returnValue);
            Assert.AreEqual(2, returnValue.Count());
        }
    }
}
