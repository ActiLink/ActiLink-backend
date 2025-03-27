
using ActiLink.Model;

namespace ActiLink.Repositories
{
    internal class UnitOfWork : IUnitOfWork
    {
        private readonly ApiContext _context;
        private IRepository<Organizer>? _organizerRepository;

        public UnitOfWork(ApiContext context)
        {
            _context = context;
        }
        public IRepository<Organizer> OrganizerRepository => _organizerRepository ??= new Repository<Organizer>(_context);  


        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
