using ActiLink.Hobbies.DTOs;
using ActiLink.Hobbies.Service;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ActiLink.Hobbies
{
    /// <summary>
    /// Controller for managing hobbies.        
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class HobbiesController : ControllerBase
    {
        private readonly ILogger<HobbiesController> _logger;
        private readonly IHobbyService _hobbyService;
        private readonly IMapper _mapper;
        public HobbiesController(ILogger<HobbiesController> logger, IHobbyService hobbyService, IMapper mapper)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _hobbyService = hobbyService ?? throw new ArgumentNullException(nameof(hobbyService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Fetches a list of all hobbies.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IActionResult"/> 
        /// with an <see cref="IEnumerable{T}"/> of <see cref="HobbyDto"/> objects.
        /// </returns>
        [HttpGet]
        [Authorize]
        [ProducesResponseType<IEnumerable<HobbyDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetHobbiesAsync()
        {
            var hobbies = await _hobbyService.GetHobbiesAsync();
            return Ok(_mapper.Map<IEnumerable<HobbyDto>>(hobbies));
        }
    }
}
