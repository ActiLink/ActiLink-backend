
using ActiLink.Model;

namespace ActiLink.Repositories
{
    internal class UnitOfWork : IUnitOfWork
    {
        private readonly ApiContext _context;
        private IRepository<User>? _organizerRepository;

        public UnitOfWork(ApiContext context)
        {
            _context = context;
        }
        public IRepository<User> UserRepository => _organizerRepository ??= new Repository<User>(_context);  


        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
