
using ActiLink.Model;

namespace ActiLink.Repositories
{
    /// <summary>
    /// Implementation of the Unit of Work pattern. This class is used to manage the repositories.
    /// </summary>
    internal class UnitOfWork : IUnitOfWork
    {
        private readonly ApiContext _context;
        private IRepository<User>? _userRepository;

        public UnitOfWork(ApiContext context)
        {
            _context = context;
        }
        public IRepository<User> UserRepository => _userRepository ??= new Repository<User>(_context);  


        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
