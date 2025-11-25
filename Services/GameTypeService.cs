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
                .OrderBy(gt => gt.Name)
                .ToListAsync();
        }

        public async Task<GameType?> GetGameTypeByIdAsync(Guid id)
        {
            return await _context.GameTypes
                .Include(gt => gt.GameRules)
                .FirstOrDefaultAsync(gt => gt.Id == id);
        }
    }
}
