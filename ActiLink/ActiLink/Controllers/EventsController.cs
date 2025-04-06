using ActiLink.DTOs;
using ActiLink.Model;
using ActiLink.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace ActiLink.Controllers
{
    /// <summary>
    /// Controller for managing events/activities.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly EventService _eventService;
        private readonly ILogger<EventsController> _logger;
        private readonly IMapper _mapper;

        public EventsController(ILogger<EventsController> logger, EventService eventService, IMapper mapper)
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
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateEventAsync([FromBody] NewEventDto newEventDto)
        {
            Event? @event = null;
            try
            {
                var createEventObject = _mapper.Map<CreateEventObject>(newEventDto);
                _logger.LogInformation("Creating event with details: {EventDetails}", createEventObject);
                var result = await _eventService.CreateEventAsync(createEventObject);
                if (!result.Succeeded)
                {
                    _logger.LogWarning("Event creation failed: {Errors}", result.Errors);
                    return BadRequest(result.Errors);
                }
                @event = result.Data!;
                _logger.LogInformation("Event {EventId} created successfully", @event.Id);
                return CreatedAtAction(nameof(GetEventByIdAsync), new { id = @event.Id }, _mapper.Map<EventDto>(@event));
            }
            catch (Exception ex)
            {
                if (@event is not null)
                    _ = await _eventService.DeleteEventAsync(@event);

                _logger.LogError(ex, "An unexpected error occurred while creating the event");
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
        [ActionName(nameof(GetEventByIdAsync))]
        [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEventByIdAsync(Guid id)
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

        /// <summary>
        /// Fetches all events.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IEnumerable{EventDto}"/> of all events.
        /// </returns>
        [HttpGet]
        public async Task<IEnumerable<EventDto>> GetAllEventsAsync()
        {
            _logger.LogInformation("Fetching all events");
            var events = await _eventService.GetAllEventsAsync();
            _logger.LogInformation("Returning {Count} events", events.Count());
            return _mapper.Map<IEnumerable<EventDto>>(events);
        }
    }
}
