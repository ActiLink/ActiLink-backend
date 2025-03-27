using ActiLink.Model;

namespace ActiLink.Repositories
{
    public interface IUnitOfWork
    {
        IRepository<Organizer> OrganizerRepository { get; }
        Task<int> SaveChangesAsync();
    }

}
