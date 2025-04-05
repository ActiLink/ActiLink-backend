using System;

namespace ActiLink.DTOs
{
    /// <summary>
    /// Data transfer object for Hobby (read operations)
    /// </summary>
    /// <param name="Id">Unique identifier of the hobby</param>
    /// <param name="Name">Name of the hobby (max 50 characters)</param>
    public record HobbyDto(
        Guid Id,
        string Name
    );
}