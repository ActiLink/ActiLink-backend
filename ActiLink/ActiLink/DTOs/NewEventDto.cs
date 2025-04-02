namespace ActiLink.DTOs
{
    /// <summary>
    /// Data transfer object for creating a new event.
    /// </summary>
    /// <param name="OrganizerId">Unique identifier of the organizer.</param>
    /// <param name="StartTime">Start time of the event.</param>
    /// <param name="EndTime">End time of the event.</param>
    /// <param name="Price">Price of the event.</param>
    /// <param name="MaxUsers">Maximum number of participants.</param>
    /// <param name="MinUsers">Minimum number of participants.</param>
    public record NewEventDto(
        Guid OrganizerId,
        DateTime StartTime,
        DateTime EndTime,
        int Height,
        int Width,
        decimal Price,
        int MaxUsers,
        int MinUsers
    );
}