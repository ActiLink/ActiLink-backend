﻿using System.Security.Claims;
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
        [Authorize(Roles = "BusinessClient")]
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
            _logger.LogInformation("Getting venue by ID: {VenueId}", id);
            var venue = await _venueService.GetVenueByIdAsync(id);
            if (venue is null)
            {
                _logger.LogWarning("Venue with ID {VenueId} not found.", id);
                return NotFound();
            }

            _logger.LogInformation("Venue with ID {VenueId} found.", id);
            _logger.LogInformation("Returning venue details: {VenueDetails}", venue);
            return Ok(_mapper.Map<VenueDto>(venue));
        }

        /// <summary>
        /// Gets all venues.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing a collection of <see cref="VenueDto"/> objects.
        /// </returns>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<VenueDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAllVenuesAsync()
        {
            _logger.LogInformation("Getting all venues.");
            var venues = await _venueService.GetAllVenuesAsync();
            _logger.LogInformation("Found {VenueCount} venues.", venues.Count());
            return Ok(_mapper.Map<IEnumerable<VenueDto>>(venues));
        }
    }
}
