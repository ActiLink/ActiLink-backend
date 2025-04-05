using ActiLink.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ActiLink.Repositories.Extensions
{
    public static class EventRepository
    {
        public static async Task<IEnumerable<Event>> GetByOrganizerAsync(
            this IRepository<Event> repository, string organizerId)
        {
            return await repository.Query()
                .Where(e => e.Organizer.Id == organizerId)
                .OrderBy(e => e.StartTime)
                .ToListAsync();
        }

        public static async Task<IEnumerable<Event>> GetUpcomingEventsAsync(
            this IRepository<Event> repository, DateTime fromDate, int daysAhead = 7)
        {
            var toDate = fromDate.AddDays(daysAhead);
            return await repository.Query()
                .Include(e => e.Organizer)
                .Where(e => e.StartTime >= fromDate && e.StartTime <= toDate)
                .OrderBy(e => e.StartTime)
                .ToListAsync();
        }

        public static async Task<IEnumerable<Event>> GetEventsByHobbyAsync(
            this IRepository<Event> repository, Guid hobbyId)
        {
            return await repository.Query()
                .Include(e => e.RelatedHobbies)
                .Where(e => e.RelatedHobbies.Any(h => h.Id == hobbyId))
                .ToListAsync();
        }

        public static async Task<int> GetParticipantCountAsync(
            this IRepository<Event> repository, Guid eventId)
        {
            return await repository.Query()
                .Where(e => e.Id == eventId)
                .SelectMany(e => e.SignUpList)
                .CountAsync();
        }
    }
}