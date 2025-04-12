﻿using ActiLink.Hobbies.DTOs;
using ActiLink.Organizers.Users.DTOs;
using ActiLink.Shared.Model;

namespace ActiLink.Events.DTOs
{
    /// <summary>
    /// Data transfer object for an event.
    /// </summary>
    /// <param name="Id">Unique identifier of the event.</param>
    /// <param name="OrganizerId">Unique identifier of the organizer.</param>
    /// <param name="Title"> Title of the event (max 100 characters).</param>
    /// <param name="Description"> Description of the event.</param>
    /// <param name="StartTime">Start time of the event.</param>
    /// <param name="EndTime">End time of the event.</param>
    /// <param name="Location"> Location of the event</param>
    /// <param name="Price">Price of the event.</param>
    /// <param name="MaxUsers">Maximum number of participants.</param>
    /// <param name="MinUsers">Minimum number of participants.</param>
    /// <param name="Participants">List of Participants.</param>
    /// <param name="Hobbies">List of Hobbies related to this event.</param>
    public record EventDto(
        Guid Id,
        string OrganizerId,
        string Title,
        string Description,
        DateTime StartTime,
        DateTime EndTime,
        Location Location,
        decimal Price,
        int MinUsers,
        int MaxUsers,
        List<UserDto> Participants,
        List<HobbyDto> Hobbies
    )
    {
        public EventDto()
            : this(
                default,
                string.Empty,
                string.Empty,
                string.Empty,
                default,
                default,
                new Location(0, 0),
                default,
                default,
                default,
                [],
                [])
        { }
    }

}