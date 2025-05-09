using System.Security.Claims;
using ActiLink.Events.DTOs;
using ActiLink.Events.Service;
using ActiLink.Shared.ServiceUtils;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ActiLink.Events
{
    /// <summary>
    /// Controller for managing events/activities.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;
        private readonly ILogger<EventsController> _logger;
        private readonly IMapper _mapper;

        public EventsController(ILogger<EventsController> logger, IEventService eventService, IMapper mapper)
        {
            _eventService = eventService;
            _logger = logger;
            _mapper = mapper;
        }
        /// <summary>
        /// Creates a new event with the specified details in the request body.
        /// </summary>
        /// <param name="newEventDto"></param>
        /// <returns>
        /// Returns a CreatedAtAction result with the created event's details or an error response.
        /// </returns>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateEventAsync([FromBody] NewEventDto newEventDto)
        {
            Event? newEvent = null;
            try
            {
                var userIdFromToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdFromToken is null)
                {
                    _logger.LogWarning("User ID not found in token");
                    return Unauthorized("User ID not found in token");
                }

                var createEventObject = _mapper.Map<CreateEventObject>(
                    newEventDto,
                    opts => opts.Items["OrganizerId"] = userIdFromToken);
                _logger.LogInformation("Creating event with details: {EventDetails}", createEventObject);
                var result = await _eventService.CreateEventAsync(createEventObject);
                if (!result.Succeeded)
                {
                    _logger.LogWarning("Event creation failed: {Errors}", result.Errors);
                    return BadRequest(result.Errors);
                }
                newEvent = result.Data!;
                _logger.LogInformation("Event {EventId} created successfully", newEvent.Id);
                return CreatedAtAction(nameof(GetEventByIdAsync), new { id = newEvent.Id }, _mapper.Map<EventDto>(newEvent));
            }
            catch (Exception ex)
            {
                if (newEvent is not null)
                    _ = await _eventService.DeleteEventAsync(newEvent);

                _logger.LogError(ex, "An unexpected error occurred while creating the event");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred");
            }
        }


        /// <summary>
        /// Updates an existing event with the specified ID and details in the request body.
        /// </summary>
        /// <returns>
        /// Returns Ok if the update was successful, or an error response if the event was not found or the user is not authorized.
        /// </returns>
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateEventAsync(Guid id, [FromBody] UpdateEventDto updateEventDto)
        {
            try
            {
                var userIdFromToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdFromToken is null)
                {
                    _logger.LogWarning("User ID not found in token");
                    return Unauthorized("User ID not found in token");
                }

                var result = await _eventService.UpdateEventAsync(id,
                                                                  _mapper.Map<UpdateEventObject>(updateEventDto),
                                                                  userIdFromToken);

                if (!result.Succeeded)
                {
                    return result.ErrorCode switch
                    {
                        ErrorCode.Forbidden => Forbid(),
                        ErrorCode.NotFound => NotFound(),
                        _ => BadRequest(result.Errors),
                    };
                }

                return Ok(_mapper.Map<EventDto>(result.Data!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while updating the event");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred");
            }
        }

        /// <summary>
        /// Fetches the event with the specified ID.
        /// </summary>
        /// <param name="id">The event ID to fetch.</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IActionResult"/> of the operation
        /// with the <see cref="EventDto"/> object or an error response if the event was not found.
        /// </returns>
        [HttpGet("{id}")]
        [Authorize]
        [ActionName(nameof(GetEventByIdAsync))]
        [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEventByIdAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Fetching event with ID {EventId}", id);
                var @event = await _eventService.GetEventByIdAsync(id);
                if (@event is null)
                {
                    _logger.LogWarning("Event with ID {EventId} not found", id);
                    return NotFound();
                }

                _logger.LogInformation("Event with ID {EventId} found", id);
                return Ok(_mapper.Map<EventDto>(@event));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while fetching the event {id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred");
            }
        }

        /// <summary>
        /// Fetches all events.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IEnumerable{EventDto}"/> of all events.
        /// </returns>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<EventDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IEnumerable<EventDto>> GetAllEventsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all events");
                var events = await _eventService.GetAllEventsAsync();
                _logger.LogInformation("Returning {Count} events", events.Count());
                return _mapper.Map<IEnumerable<EventDto>>(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while fetching all events");
                return (IEnumerable<EventDto>)StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred");
            }
        }
        /// <summary>
        /// Deletes the event with the specified ID.
        /// </summary>
        /// 
        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteEventAsync(Guid id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId is null)
                    return Unauthorized("User ID not found in token");

                var result = await _eventService.DeleteEventByIdAsync(id, userId);

                if (!result.Succeeded)
                {
                    return result.ErrorCode switch
                    {
                        ErrorCode.Forbidden => Forbid(),
                        ErrorCode.NotFound => NotFound(),
                        _ => BadRequest(result.Errors),
                    };
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while deleting the event {id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred");
            }
        }

        /// <summary>
        /// Enrolls the user in the event with the specified ID.
        /// </summary>
        /// <param name="id">The ID of the event to enroll in.</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IActionResult"/> of the operation
        /// with the <see cref="EventDto"/> object or an error response.
        /// </returns>
        [HttpPost("{id}/enroll")]
        [Authorize(Roles = "User")] // Nie działa z claimsasmi, trzeba by użyć user managera do dodawania roli
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SignUpForEventAsync(Guid id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId is null)
                {
                    _logger.LogWarning("User ID not found in token");
                    return Unauthorized("User ID not found in token");
                }

                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                if (userRole is null)
                {
                    _logger.LogWarning("User role not found in token");
                    return Unauthorized("User role not found in token");
                }

                if (userRole != "User")
                {
                    _logger.LogWarning("User is not authorized to enroll in events");
                    return Forbid("User is not authorized to enroll in events");
                }

                var result = await _eventService.SignUpForEventAsync(id, userId);
                if (!result.Succeeded)
                {
                    return result.ErrorCode switch
                    {
                        ErrorCode.Forbidden => Forbid(),
                        ErrorCode.NotFound => NotFound(),
                        _ => BadRequest(result.Errors),
                    };
                }

                return Ok(_mapper.Map<EventDto>(result.Data!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while enrolling for the event {id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred");
            }
        }

        /// <summary>
        /// Withdraws the user from the event with the specified ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IActionResult"/> of the operation
        /// with the <see cref="EventDto"/> object or an error response.
        /// </returns>
        [HttpPost("{id}/withdraw")]
        [Authorize(Roles = "User")] // Nie działa z claimsasmi, trzeba by użyć user managera do dodawania roli
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UnsignFromEventAsync(Guid id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId is null)
                {
                    _logger.LogWarning("User ID not found in token");
                    return Unauthorized("User ID not found in token");
                }

                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                if (userRole is null)
                {
                    _logger.LogWarning("User role not found in token");
                    return Unauthorized("User role not found in token");
                }

                if (userRole != "User")
                {
                    _logger.LogWarning("User is not authorized to enroll in events");
                    return Forbid("User is not authorized to enroll in events");
                }

                var result = await _eventService.UnsignFromEventAsync(id, userId);
                if (!result.Succeeded)
                {
                    return result.ErrorCode switch
                    {
                        ErrorCode.Forbidden => Forbid(),
                        ErrorCode.NotFound => NotFound(),
                        _ => BadRequest(result.Errors),
                    };
                }
                return Ok(_mapper.Map<EventDto>(result.Data!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while unenrolling from the event {id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred");
            }
        }
    }
}
