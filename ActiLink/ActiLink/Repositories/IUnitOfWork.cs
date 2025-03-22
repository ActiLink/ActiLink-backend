namespace ActiLink.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<WeatherForecast> WeatherForecastRepository { get; }
        Task<int> SaveChangesAsync();
    }

}
