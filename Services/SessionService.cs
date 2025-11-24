using Microsoft.EntityFrameworkCore;
using SnookerGameManagementSystem.Data;
using SnookerGameManagementSystem.Models;

namespace SnookerGameManagementSystem.Services
{
    public class SessionService
    {
        private readonly SnookerDbContext _context;

        public SessionService(SnookerDbContext context)
        {
            _context = context;
        }

        public async Task<List<Session>> GetActiveSessionsAsync()
        {
            return await _context.Sessions
                .Include(s => s.GameType)
                .Include(s => s.Frames)
                    .ThenInclude(f => f.Participants)
                    .ThenInclude(p => p.Customer)
                .Where(s => s.Status == SessionStatus.IN_PROGRESS)
                .OrderBy(s => s.StartedAt)
                .ToListAsync();
        }

        public async Task<Session> CreateSessionAsync(string name, Guid? gameTypeId = null)
        {
            var session = new Session
            {
                Name = name,
                GameTypeId = gameTypeId,
                StartedAt = DateTime.Now,
                Status = SessionStatus.IN_PROGRESS
            };

            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();

            return session;
        }

        public async Task<Session?> GetSessionByIdAsync(Guid sessionId)
        {
            return await _context.Sessions
                .Include(s => s.GameType)
                .Include(s => s.Frames)
                    .ThenInclude(f => f.Participants)
                    .ThenInclude(p => p.Customer)
                .FirstOrDefaultAsync(s => s.Id == sessionId);
        }

        public async Task EndSessionAsync(Guid sessionId)
        {
            var session = await _context.Sessions.FindAsync(sessionId);
            if (session != null)
            {
                session.EndedAt = DateTime.Now;
                session.Status = SessionStatus.ENDED;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetNextTableNumberAsync()
        {
            var activeSessions = await _context.Sessions
                .Where(s => s.Status == SessionStatus.IN_PROGRESS)
                .ToListAsync();

            var usedNumbers = activeSessions
                .Select(s => s.Name)
                .Where(name => name.StartsWith("Table #"))
                .Select(name =>
                {
                    var numStr = name.Replace("Table #", "");
                    return int.TryParse(numStr, out int num) ? num : 0;
                })
                .Where(num => num > 0)
                .ToList();

            if (!usedNumbers.Any())
                return 1;

            // Find first gap or return max + 1
            for (int i = 1; i <= usedNumbers.Max() + 1; i++)
            {
                if (!usedNumbers.Contains(i))
                    return i;
            }

            return 1;
        }
    }
}
