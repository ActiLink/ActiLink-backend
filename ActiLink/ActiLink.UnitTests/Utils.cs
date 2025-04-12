﻿using ActiLink.Events;
using ActiLink.Hobbies;

namespace ActiLink.UnitTests
{
    internal class Utils
    {
        internal static void SetupEventGuid(Event existingEvent, Guid eventId)
        {
            existingEvent.GetType()
                .GetProperty("Id")?
                .SetValue(existingEvent, eventId);
        }
        internal static void SetupHobbyGuid(Hobby existingHobby, Guid hobbyId)
        {
            existingHobby.GetType()
                .GetProperty("Id")?
                .SetValue(existingHobby, hobbyId);
        }
    }
}
