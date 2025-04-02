using ActiLink.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ActiLink.Repositories
{
    public class EventRepository : Repository<Event>, IRepository<Event>
    {
        public EventRepository(IdentityDbContext<Organizer> context) : base(context)
        {
        }

        public async Task<IEnumerable<Event>> GetByOrganizerAsync(string organizerId)
        {
            return await _dbSet
                .Where(e => e.OrganizerId == organizerId)
                .OrderBy(e => e.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Event>> GetUpcomingEventsAsync(DateTime fromDate, int daysAhead = 7)
        {
            var toDate = fromDate.AddDays(daysAhead);
            return await _dbSet
                .Where(e => e.StartTime >= fromDate && e.StartTime <= toDate)
                .OrderBy(e => e.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Event>> GetEventsByHobbyAsync(Guid hobbyId)
        {
            return await _dbSet
                .Where(e => e.RelatedHobbies.Any(h => h.Id == hobbyId))
                .Include(e => e.RelatedHobbies)
                .ToListAsync();
        }

        public async Task<int> GetParticipantCountAsync(Guid eventId)
        {
            return await _dbSet
                .Where(e => e.Id == eventId)
                .SelectMany(e => e.SignUpList)
                .CountAsync();
        }
    }
}