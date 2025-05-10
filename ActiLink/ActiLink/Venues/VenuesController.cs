using System.Security.Claims;
using ActiLink.Events.DTOs;
using ActiLink.Venues.DTOs;
using ActiLink.Venues.Service;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ActiLink.Venues
{
    [Route("[controller]")]
    [ApiController]
    public class VenuesController : ControllerBase
    {
        private readonly IVenueService _venueService;
        private readonly ILogger<VenuesController> _logger;
        private readonly IMapper _mapper;

        public VenuesController(IVenueService venueService, ILogger<VenuesController> logger, IMapper mapper)
        {
            _venueService = venueService;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpPost]
        [Authorize(Roles = "BusinessClient")] // Nie działa
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateVenueAsync([FromBody] NewVenueDto newVenueDto)
        {
            Venue? venue = null;
            try
            {
                var userIdFromToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdFromToken is null)
                {
                    _logger.LogWarning("User ID not found in token.");
                    return Unauthorized();
                }

                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (userRole is null)
                {
                    _logger.LogWarning("User role not found in token.");
                    return Unauthorized();
                }

                if (userRole != "BusinessClient")
                {
                    _logger.LogWarning("User is not authorized to create a venue.");
                    return Forbid();
                }

                var createVenueObject = _mapper.Map<CreateVenueObject>(newVenueDto, opts =>
                {
                    opts.Items["OwnerId"] = userIdFromToken;
                });

                _logger.LogInformation("Creating venue with details: {VenueDetails}", createVenueObject);
                var result = await _venueService.CreateVenueAsync(createVenueObject);

                if (!result.Succeeded)
                {
                    _logger.LogError("Failed to create venue: {Errors}", result.Errors);
                    return BadRequest(result.Errors);
                }

                venue = result.Data!;
                _logger.LogInformation("Venue {VenueId} created successfully.", venue.Id);
                return CreatedAtAction(nameof(GetVenueByIdAsync), new { id = venue.Id }, _mapper.Map<VenueDto>(venue));
            }
            catch (Exception ex)
            {
                if (venue is not null)
                    _ = await _venueService.DeleteVenueAsync(venue);

                _logger.LogError(ex, "An error occurred while creating the venue.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Gets the venue by ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Authorize]
        [ActionName(nameof(GetVenueByIdAsync))]
        [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetVenueByIdAsync(Guid id)
        {
            throw new NotImplementedException("This method is not implemented yet.");
        }
    }
}
