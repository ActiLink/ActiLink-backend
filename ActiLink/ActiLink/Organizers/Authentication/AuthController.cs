using Microsoft.AspNetCore.Mvc;
using ActiLink.Organizers.Authentication.Service;
using ActiLink.Organizers.DTOs;

namespace ActiLink.Organizers.Authentication
{
    // / <summary>
    // Controller for managing tokens
    // / </summary>

    [ApiController]
    [Route("[controller]")]
    public class AuthController: Controller
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;
        public AuthController(ILogger<AuthController> logger, IAuthService authService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
                var result = await _authService.RefreshTokenAsync(refreshDto.RefreshToken);

                if (!result.Succeeded)
                {
                    _logger.LogWarning("Token refresh failed: {errors}", result.Errors);
                    return BadRequest(result.Errors);
                }

                (string accessToken, string refreshToken) = result.Data!;
                _logger.LogInformation("Token refresh successful");
                return Ok(new TokenResponseDto(accessToken, refreshToken));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during token refresh");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
