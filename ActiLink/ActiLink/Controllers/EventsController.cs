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
        private readonly ILogger<UsersController> _logger;
        private readonly IMapper _mapper;

        public EventsController(ILogger<UsersController> logger, EventService eventService, IMapper mapper)
        {
            this._eventService = eventService;
            _logger = logger;
            _mapper = mapper;
        }
    }
}
