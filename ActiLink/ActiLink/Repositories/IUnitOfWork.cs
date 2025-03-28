using ActiLink.Model;

namespace ActiLink.Repositories
{
    public interface IUnitOfWork
    {
        IRepository<User> UserRepository { get; }
        Task<int> SaveChangesAsync();
    }

}
