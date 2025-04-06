using ActiLink.DTOs;
using ActiLink.Model;
using ActiLink.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace ActiLink.Controllers
{
    /// <summary>
    /// Controller for managing users
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class UsersController : Controller
    {
        private readonly UserService _userService;
        private readonly ILogger<UsersController> _logger;
        private readonly IMapper _mapper;

        public UsersController(ILogger<UsersController> logger, UserService userService, IMapper mapper)
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
        /// Refreshes JWT access token using a valid refresh token.
        /// </summary>
        /// <param name="refreshDto">DTO containing refresh token</param>
        /// <returns>New access token and refresh token</returns>
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(TokenResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RefreshTokenAsync([FromBody] RefreshTokenDto refreshDto)
        {
            try
            {
                var result = await _userService.RefreshTokenAsync(refreshDto.RefreshToken);

                if (!result.Succeeded)
                {
                    _logger.LogWarning("Token refresh failed: {errors}", result.Errors);
                    return BadRequest(result.Errors);
                }

                (string accessToken, string refreshToken) = result.Data!;
                return Ok(new TokenResponseDto(accessToken, refreshToken));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during token refresh");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Fetches all users.
        /// </summary>
        /// <returns>
        /// Returns a <see cref="IEnumerable{T}"/> of all users.
        /// </returns>
        [HttpGet]
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
        [ActionName(nameof(GetUserByIdAsync))]
        [ProducesResponseType<UserDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserByIdAsync([FromRoute] string id)
        {
            _logger.LogInformation("Fetching user with ID: {UserId}", id);
            var user = await _userService.GetUserByIdAsync(id);

            if (user is null)
            {
                _logger.LogWarning("User with ID {UserId} not found", id);
                return NotFound();
            }

            return Ok(_mapper.Map<UserDto>(user));
        }
    }
}
