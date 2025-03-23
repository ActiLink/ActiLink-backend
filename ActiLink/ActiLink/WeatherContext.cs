using Microsoft.EntityFrameworkCore;

namespace ActiLink
{
    public class WeatherContext : DbContext
    {
        public WeatherContext(DbContextOptions<WeatherContext> options) : base(options) { }

        public DbSet<WeatherForecast> WeatherForecasts { get; set; }
    }
}
