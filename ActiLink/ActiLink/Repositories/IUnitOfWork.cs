namespace ActiLink.Repositories
{
    public interface IUnitOfWork
    {
        IRepository<WeatherForecast> WeatherForecastRepository { get; }
        Task<int> SaveChangesAsync();
    }

}
