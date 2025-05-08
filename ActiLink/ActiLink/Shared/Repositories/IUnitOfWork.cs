using ActiLink.Events;
using ActiLink.Hobbies;
using ActiLink.Organizers;
using ActiLink.Organizers.Authentication;
using ActiLink.Organizers.BusinessClients;
using ActiLink.Organizers.Users;
using ActiLink.Venues;

namespace ActiLink.Shared.Repositories
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
        IRepository<BusinessClient> BusinessClientRepository { get; }
        IRepository<Organizer> OrganizerRepository { get; }
        IRepository<Event> EventRepository { get; }
        IRepository<Hobby> HobbyRepository { get; }
        IRepository<Venue> VenueRepository { get; }
        IRepository<RefreshToken> RefreshTokenRepository { get; }

        /// <summary>
        /// Saves the changes to the database
        /// </summary>
        /// <returns>The number of affected rows</returns>
        Task<int> SaveChangesAsync();
    }

}
