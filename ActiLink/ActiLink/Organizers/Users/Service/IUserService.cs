﻿using ActiLink.Shared.ServiceUtils;

namespace ActiLink.Organizers.Users.Service
{
    public interface IUserService
    {
        public Task<GenericServiceResult<User>> CreateUserAsync(string username, string email, string password);
        public Task<GenericServiceResult<(string AccessToken, string RefreshToken)>> LoginAsync(string email, string password);
        public Task<IEnumerable<User>> GetUsersAsync();
        public Task<User?> GetUserByIdAsync(string id);
        public Task<User?> GetUserWithHobbiesByIdAsync(string id);
        public Task<ServiceResult> DeleteUserAsync(User user);
        public Task<GenericServiceResult<User>> UpdateUserAsync(string id, UpdateUserObject updateUserObject);
    }
}
