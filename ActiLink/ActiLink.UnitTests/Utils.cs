using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ActiLink.Model;

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
    }
}
