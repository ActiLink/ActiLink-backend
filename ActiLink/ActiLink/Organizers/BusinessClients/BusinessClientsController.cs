using ActiLink.Organizers.BusinessClients.DTOs;
using ActiLink.Organizers.BusinessClients.Service;
using ActiLink.Organizers.DTOs;
using ActiLink.Shared.ServiceUtils;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ActiLink.Organizers.BusinessClients
{
    [Route("[controller]")]
    [ApiController]
    public class BusinessClientsController : ControllerBase
    {
        private readonly IBusinessClientService _businessClientService;
        private readonly ILogger<BusinessClientsController> _logger;
        private readonly IMapper _mapper;

        public BusinessClientsController(ILogger<BusinessClientsController> logger, IBusinessClientService businessClientService, IMapper mapper)
        {
            _businessClientService = businessClientService;
            _logger = logger;
            _mapper = mapper;
        }


        /// <summary>
        /// Creates a new business client with the specified details in the request body.
        /// </summary>
        /// <param name="newBusinessClientDto">The data transfer object containing the new business client's details.</param>
        /// <returns>Returns a CreatedAtAction result with the created business client's details or an error response.</returns>
        [HttpPost("register")]
        [ProducesResponseType<BusinessClientDto>(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateBusinessClientAsync([FromBody] NewBusinessClientDto newBusinessClientDto)
        {
            BusinessClient? businessClient = null;

            try
            {
                var (username, email, password, taxId) = newBusinessClientDto;
                _logger.LogInformation("Creating business client {username} with email {email}", username, email);

                var result = await _businessClientService.CreateBusinessClientAsync(username, email, password, taxId);

                if (!result.Succeeded)
                {
                    _logger.LogWarning("Business client registration failed: {errors}", result.Errors);
                    return BadRequest(result.Errors);
                }

                businessClient = result.Data!;
                _logger.LogInformation("Business client {businessClientId} created successfully", businessClient.Id);
                return CreatedAtAction(nameof(GetBusinessClientByIdAsync), new { id = businessClient.Id }, _mapper.Map<BusinessClientDto>(businessClient));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred");

                if (businessClient is not null)
                    _logger.LogError("Business client {businessClientId} creation failed", businessClient.Id);

                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred");
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
                var result = await _businessClientService.LoginAsync(email, password);

                if (!result.Succeeded)
                {
                    _logger.LogWarning("BuisnessClient login failed: {errors}", result.Errors);
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
        /// Fetches all business clients.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing an <see cref="IActionResult"/> 
        /// with the IEnumerable of <see cref="BusinessClientDto"/> objects.
        /// </returns>
        [HttpGet]
        [Authorize]
        [ProducesResponseType<IEnumerable<BusinessClientDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetBusinessClientsAsync()
        {
            _logger.LogInformation("Fetching all business clients");
            var businessClients = await _businessClientService.GetBusinessClientsAsync();
            _logger.LogInformation("Returning {businessClientsCount} business clients", businessClients.Count());
            return Ok(_mapper.Map<IEnumerable<BusinessClientDto>>(businessClients));
        }



        /// <summary>
        /// Fetches a business client by ID.
        /// </summary>
        /// <param name="id">The ID of the business client to fetch.</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, 
        /// containing an Ok result with the <see cref="BusinessClientDto"/> object if found, or a NotFound result.
        /// </returns>
        [HttpGet("{id}")]
        [Authorize]
        [ActionName(nameof(GetBusinessClientByIdAsync))]
        [ProducesResponseType<BusinessClientDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBusinessClientByIdAsync([FromRoute] string id)
        {
            _logger.LogInformation("Fetching business client with ID {id}", id);
            var businessClient = await _businessClientService.GetBusinessClientByIdAsync(id);

            if (businessClient is null)
            {
                _logger.LogWarning("Business client with ID {id} not found", id);
                return NotFound();
            }

            _logger.LogInformation("Business client with ID {id} found", id);
            return Ok(_mapper.Map<BusinessClientDto>(businessClient));
        }
        [HttpPut("{id}")]
		[Authorize]
		[ProducesResponseType(typeof(BusinessClientDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateBusinessClientAsync([FromRoute] string id, [FromBody] UpdateBusinessClientDto updateBusinessClientDto)
		{
			try
			{
				var businessCLientIdFromToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if(businessCLientIdFromToken is null)
				{
					_logger.LogWarning("Business client ID not found in token");
					return Unauthorized("Business client ID not found in token");
				}
				if (businessCLientIdFromToken != id)
				{
					_logger.LogWarning("Business client ID from token ({idFromToken}) does not match the requested business client ID ({id})", businessCLientIdFromToken, id);
					return Forbid();
				}
				_logger.LogInformation("Updating business client with ID: {BusinessClientId}", id);
                var updateBusinessClientObject = _mapper.Map<UpdateBusinessClientObject>(updateBusinessClientDto);
				var result = await _businessClientService.UpdateBusinessClientAsync(id, updateBusinessClientObject);
				if (!result.Succeeded)
				{
					_logger.LogWarning("Business client update failed: {errors}", result.Errors);
					return result.ErrorCode switch
					{
						ErrorCode.NotFound => NotFound(result.Errors),
						ErrorCode.Forbidden => Forbid(),
						_ => BadRequest(result.Errors)
					};
				}
                return Ok(_mapper.Map<BusinessClientDto>(result.Data!));

			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An unexpected error occurred while updating the business client");
				return StatusCode(StatusCodes.Status500InternalServerError);
			}
		}
		
	}
}
