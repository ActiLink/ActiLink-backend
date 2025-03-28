using ActiLink.DTOs;
using ActiLink.Exceptions;
using ActiLink.Model;
using ActiLink.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ActiLink.Services
{
    public class UserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<Organizer> _userManager;
        public UserService(IUnitOfWork unitOfWork, UserManager<Organizer> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        /// <summary>
        /// Creates a new user
        /// </summary>
        /// <param name="username"></param>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns>The created user</returns>
        /// <exception cref="UserRegistrationException"></exception>
        public async Task<User> CreateUserAsync(string username, string email, string password)
        {
            var user = new User(username, email);
            var result = await _userManager.CreateAsync(user, password);

            return result.Succeeded ? user : throw new UserRegistrationException("User registration failed", result.Errors);
        }

        /// <summary>
        /// Gets all users
        /// </summary>
        /// <returns>An IEnumerable of all users</returns>
        public async Task<IEnumerable<User>> GetUsersAsync()
        {
            return await _unitOfWork.UserRepository.GetAllAsync();
        }

        /// <summary>
        /// Gets a user by their ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns>The user if found, null otherwise</returns>
        public async Task<User?> GetUserByIdAsync(string id)
        {
            return await _userManager.FindByIdAsync(id) as User;
        }

        /// <summary>
        /// Deletes a user
        /// </summary>
        /// <param name="user"></param>
        /// <returns>True if the user was deleted, false otherwise</returns>
        public async Task<bool> DeleteUserAsync(User user)
        {
            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }

    }
}
