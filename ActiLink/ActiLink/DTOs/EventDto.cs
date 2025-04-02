using System;

namespace ActiLink.DTOs
{
    /// <summary>
    /// Data transfer object for an event.
    /// </summary>
    /// <param name="Id">Unique identifier of the event.</param>
    /// <param name="OrganizerId">Unique identifier of the organizer.</param>
    /// <param name="StartTime">Start time of the event.</param>
    /// <param name="EndTime">End time of the event.</param>
    /// <param name="Price">Price of the event.</param>
    /// <param name="MaxUsers">Maximum number of participants.</param>
    /// <param name="MinUsers">Minimum number of participants.</param>
    /// <param name="Participants">List of Participants.</param>
    /// <param name="Hobbies">List of Hobbies related to this event.</param>
    public record EventDto(
        Guid Id,
        Guid OrganizerId,
        DateTime StartTime,
        DateTime EndTime,
        int Height,
        int Width,
        decimal Price,
        int MaxUsers,
        int MinUsers,
        List<UserDto> Participants,
        List<HobbyDto> Hobbies
    )
    {
        public EventDto() 
            : this(
                default, 
                default, 
                default, 
                default, 
                default, 
                default, 
                default, 
                default, 
                default,
                new List<UserDto>(),
                new List<HobbyDto>()) 
        { }
    }

}