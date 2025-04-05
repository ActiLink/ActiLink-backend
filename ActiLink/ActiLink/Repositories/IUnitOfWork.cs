using ActiLink.Model;

namespace ActiLink.Repositories
{
    /// <summary>
    /// Interface for the Unit of Work pattern. This interface is used to manage the repositories.
    /// </summary>
    public interface IUnitOfWork
    {
        /// <summary>
        /// Repository for the User entity
        /// </summary>
        IRepository<User> UserRepository { get; }
        IRepository<Event> EventRepository { get; }

        /// <summary>
        /// Saves the changes to the database
        /// </summary>
        /// <returns>The number of affected rows</returns>
        Task<int> SaveChangesAsync();
    }

}
