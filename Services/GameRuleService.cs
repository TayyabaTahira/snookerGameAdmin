using Microsoft.EntityFrameworkCore;
using SnookerGameManagementSystem.Data;
using SnookerGameManagementSystem.Models;

namespace SnookerGameManagementSystem.Services
{
    public class GameRuleService
    {
        private readonly SnookerDbContext _context;

        public GameRuleService(SnookerDbContext context)
        {
            _context = context;
        }

        public async Task<List<GameType>> GetAllGameTypesAsync()
        {
            return await _context.GameTypes
                .Include(gt => gt.GameRules)
                .ToListAsync();
        }

        public async Task<GameRule?> GetRuleByGameTypeIdAsync(Guid gameTypeId)
        {
            return await _context.GameRules
                .Include(r => r.GameType)
                .FirstOrDefaultAsync(r => r.GameTypeId == gameTypeId);
        }
    }
}
