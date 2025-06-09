using System.Security.Claims;
using ActiLink.Organizers.DTOs;
using ActiLink.Organizers.Users.DTOs;
using ActiLink.Organizers.Users.Service;
using ActiLink.Shared.ServiceUtils;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ActiLink.Organizers.Users
{
    /// <summary>
    /// Controller for managing users
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class UsersController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;
        private readonly IMapper _mapper;

        public UsersController(ILogger<UsersController> logger, IUserService userService, IMapper mapper)
        {
            _userService = userService;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Creates a new user with the specified details in the request body.
        /// </summary>
        /// <param name="newUserDto">The data transfer object containing the new user's details.</param>
        /// <returns>Returns a CreatedAtAction result with the created user's details or an error response.</returns>
        [HttpPost("register")]
        [ProducesResponseType<UserDto>(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateUserAsync([FromBody] NewUserDto newUserDto)
        {
            User? user = null;
            try
            {
                var (username, email, password) = newUserDto;
                _logger.LogInformation("Creating user {username} with email {email}", username, email);

                var result = await _userService.CreateUserAsync(username, email, password);

                if (!result.Succeeded)
                {
                    _logger.LogWarning("User registration failed: {errors}", result.Errors);
                    return BadRequest(result.Errors);
                }

                user = result.Data!;
                _logger.LogInformation("User {userId} created successfully", user.Id);
                return CreatedAtAction(nameof(GetUserByIdAsync), new { id = user.Id }, _mapper.Map<UserDto>(user));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred");

                if (user is not null)
                    _ = await _userService.DeleteUserAsync(user);

                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
        /// <summary>
        /// Logs in a user with the specified email and password.
        /// </summary>
        /// <param name="loginDto"></param>
        /// <returns>Returns a JWT token which can be used for authentication on other endpoints</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(TokenResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> LoginAsync([FromBody] LoginDto loginDto)
        {
            try
            {
                var (email, password) = loginDto;
                _logger.LogInformation("Attempting login for email: {Email}", loginDto.Email);
                var result = await _userService.LoginAsync(email, password);

                if (!result.Succeeded)
                {
                    _logger.LogWarning("User login failed: {errors}", result.Errors);
                    return BadRequest(result.Errors);
                }

                var (accessToken, refreshToken) = result.Data!;
                _logger.LogInformation("Login successful for email: {Email}", loginDto.Email);
                return Ok(new TokenResponseDto(accessToken, refreshToken));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during login");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Fetches all users.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing an <see cref="IEnumerable{UserDto}"/> of all users.
        /// </returns>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IEnumerable<UserDto>> GetUsersAsync()
        {
            _logger.LogInformation("Fetching all users");
            var users = await _userService.GetUsersAsync();
            _logger.LogInformation("Returning {userCount} users", users.Count());
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        /// <summary>
        /// Fetches a user by their ID.
        /// </summary>
        /// <param name="id">The ID of the user to fetch</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IActionResult"/> of the operation with the <see cref="UserDto"/> if the user exists.  
        /// </returns>
        [HttpGet("{id}")]
        [Authorize]
        [ActionName(nameof(GetUserByIdAsync))]
        [ProducesResponseType<UserDetailsDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserByIdAsync([FromRoute] string id)
        {
            var userIdFromToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdFromToken is null)
            {
                _logger.LogWarning("User ID not found in token");
                return Unauthorized("User ID not found in token");
            }

            if (userIdFromToken != id)
            {
                _logger.LogWarning("User ID from token ({idFromToken}) does not match the requested user ID ({id})", userIdFromToken, id);
                return Forbid();
            }

            _logger.LogInformation("Fetching user with ID: {UserId}", id);
            var user = await _userService.GetUserWithHobbiesByIdAsync(id);

            if (user is null)
            {
                _logger.LogWarning("User with ID {UserId} not found", id);
                return NotFound();
            }

            _logger.LogInformation("User with ID {UserId} found", id);
            return Ok(_mapper.Map<UserDetailsDto>(user));
        }


        /// <summary>
        /// Updates a user with the specified ID and details in the request body.
        /// </summary>
        /// <param name="id">The ID of the user to update</param>
        /// <param name="updateUserDto">The data transfer object containing the updated user's details.</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IActionResult"/> of the operation
        /// with the updated <see cref="UserDetailsDto"/> object or an error response if the update failed.
        /// </returns>
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(UserDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateUserAsync([FromRoute] string id, [FromBody] UpdateUserDto updateUserDto)
        {
            try
            {
                var userIdFromToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdFromToken is null)
                {
                    _logger.LogWarning("User ID not found in token");
                    return Unauthorized("User ID not found in token");
                }

                if (userIdFromToken != id)
                {
                    _logger.LogWarning("User ID from token ({idFromToken}) does not match the requested user ID ({id})", userIdFromToken, id);
                    return Forbid();
                }

                _logger.LogInformation("Updating user with ID: {UserId}", id);
                var updateUserObject = _mapper.Map<UpdateUserObject>(updateUserDto);
                var result = await _userService.UpdateUserAsync(id, updateUserObject);
                if (!result.Succeeded)
                {
                    _logger.LogWarning("User update failed: {errors}", result.Errors);
                    return result.ErrorCode switch
                    {
                        ErrorCode.NotFound => NotFound(result.Errors),
                        ErrorCode.Forbidden => Forbid(),
                        _ => BadRequest(result.Errors)
                    };
                }
                return Ok(_mapper.Map<UserDetailsDto>(result.Data!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while updating the user");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType<UserDetailsDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMeAsync()
        {
            var userIdFromToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdFromToken is null)
            {
                _logger.LogWarning("User ID not found in token");
                return Unauthorized("User ID not found in token");
            }

            var user = await _userService.GetUserByIdAsync(userIdFromToken);

            if (user is null)
            {
                _logger.LogWarning("User with ID {UserId} not found", userIdFromToken);
                return NotFound();
            }

            _logger.LogInformation("User with ID {UserId} found", userIdFromToken);
            return Ok(_mapper.Map<UserDetailsDto>(user));
        }
    }
}
