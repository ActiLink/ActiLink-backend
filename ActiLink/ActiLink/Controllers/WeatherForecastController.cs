using ActiLink.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ActiLink.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    //private static readonly string[] Summaries = new[]
    //{
    //    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    //};

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<ActionResult<IEnumerable<WeatherForecast>>> Get()
    {
        var forecasts = await _unitOfWork.WeatherForecastRepository.GetAllAsync();
        return Ok(forecasts);
    }
}
