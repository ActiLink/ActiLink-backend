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

        public async Task<User> CreateUserAsync(string username, string email, string password)
        {
            var user = new User(username, email);
            var result = await _userManager.CreateAsync(user, password);

            return result.Succeeded ? user : throw new UserRegistrationException("User registration failed", result.Errors);
        }

        public async Task<IEnumerable<User>> GetUsersAsync()
        {
            return await _unitOfWork.UserRepository.GetAllAsync();
        }

        public async Task<User?> GetUserByIdAsync(string id)
        {
            return await _userManager.FindByIdAsync(id) as User;
        }

        public async Task<bool> DeleteUserAsync(User user)
        {
            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }

    }
}
