using Microsoft.EntityFrameworkCore;
using SnookerGameManagementSystem.Data;
using SnookerGameManagementSystem.Models;

namespace SnookerGameManagementSystem.Services
{
    public class GameTypeService
    {
        private readonly SnookerDbContext _context;

        public GameTypeService(SnookerDbContext context)
        {
            _context = context;
        }

        public async Task<List<GameType>> GetAllGameTypesAsync()
        {
            return await _context.GameTypes
                .Include(gt => gt.GameRules)
                .OrderBy(gt => gt.Name)
                .ToListAsync();
        }

        public async Task<GameType?> GetGameTypeByIdAsync(Guid id)
        {
            return await _context.GameTypes
                .Include(gt => gt.GameRules)
                .FirstOrDefaultAsync(gt => gt.Id == id);
        }

        public async Task<GameType> CreateGameTypeAsync(string name)
        {
            var gameType = new GameType
            {
                Name = name
            };

            _context.GameTypes.Add(gameType);
            await _context.SaveChangesAsync();

            return gameType;
        }

        public async Task<GameType> UpdateGameTypeAsync(GameType gameType)
        {
            _context.GameTypes.Update(gameType);
            await _context.SaveChangesAsync();
            return gameType;
        }

        public async Task DeleteGameTypeAsync(Guid id)
        {
            var gameType = await _context.GameTypes.FindAsync(id);
            if (gameType != null)
            {
                _context.GameTypes.Remove(gameType);
                await _context.SaveChangesAsync();
            }
        }
    }
}
