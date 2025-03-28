using ActiLink.DTOs;
using ActiLink.Exceptions;
using ActiLink.Model;
using ActiLink.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace ActiLink.Controllers
{
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

        [HttpPost("register")]
        public async Task<IActionResult> CreateUserAsync([FromBody] NewUserDto newUserDto)
        {
            User? user = null;
            try
            {
                var (username, email, password) = newUserDto;
                _logger.LogInformation("Creating user {username} with email {email}", username, email);

                user = await _userService.CreateUserAsync(username, email, password);

                _logger.LogInformation("User {userId} created successfully", user.Id);
                return CreatedAtAction("GetUserById", new {id = user.Id}, _mapper.Map<UserDto>(user));
            }
            catch (UserRegistrationException ex)
            {
                _logger.LogError(ex, "User registration failed");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred");

                if(user is not null)
                    _ = await _userService.DeleteUserAsync(user);

                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet]
        public async Task<IEnumerable<UserDto>> GetUsersAsync()
        {
            _logger.LogInformation("Fetching all users");
            var users = await _userService.GetUsersAsync();
            _logger.LogInformation("Returning {userCount} users", users.Count());
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        [HttpGet("{id}", Name = "GetUserById")]
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
