
namespace ActiLink.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly WeatherContext _context;
        private IRepository<WeatherForecast>? _weatherForecastRepository;

        public UnitOfWork(WeatherContext context)
        {
            _context = context;
        }

        public IRepository<WeatherForecast> WeatherForecastRepository
        {
            get
            {
                return _weatherForecastRepository ??= new Repository<WeatherForecast>(_context);
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
