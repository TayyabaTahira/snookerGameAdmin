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

        public async Task<GameRule> CreateGameRuleAsync(
            Guid gameTypeId, 
            decimal baseRate, 
            decimal overtimeRate)
        {
            var gameRule = new GameRule
            {
                GameTypeId = gameTypeId,
                BaseRate = baseRate,
                OvertimeRate = overtimeRate,
                DefaultPayerMode = PayerMode.LOSER // Default payer mode
            };

            _context.GameRules.Add(gameRule);
            await _context.SaveChangesAsync();

            return gameRule;
        }

        public async Task<GameRule> UpdateGameRuleAsync(GameRule gameRule)
        {
            _context.GameRules.Update(gameRule);
            await _context.SaveChangesAsync();
            return gameRule;
        }

        public async Task DeleteGameRuleAsync(Guid id)
        {
            var gameRule = await _context.GameRules.FindAsync(id);
            if (gameRule != null)
            {
                _context.GameRules.Remove(gameRule);
                await _context.SaveChangesAsync();
            }
        }
    }
}
