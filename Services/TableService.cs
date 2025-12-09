using Microsoft.EntityFrameworkCore;
using SnookerGameManagementSystem.Data;
using SnookerGameManagementSystem.Models;

namespace SnookerGameManagementSystem.Services
{
    public class TableService
    {
        private readonly SnookerDbContext _context;

        public TableService(SnookerDbContext context)
        {
            _context = context;
        }

        public async Task<List<Table>> GetAllTablesAsync()
        {
            return await _context.Tables
                .OrderBy(t => t.DisplayOrder)
                .ToListAsync();
        }

        public async Task<List<Table>> GetActiveTablesAsync()
        {
            return await _context.Tables
                .Where(t => t.IsActive)
                .OrderBy(t => t.DisplayOrder)
                .ToListAsync();
        }

        public async Task<Table?> GetTableByIdAsync(Guid id)
        {
            return await _context.Tables
                .Include(t => t.Sessions)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Table> CreateTableAsync(string name, int displayOrder)
        {
            // Check if name already exists
            var existing = await _context.Tables
                .FirstOrDefaultAsync(t => t.Name == name);
            
            if (existing != null)
            {
                throw new InvalidOperationException($"A table with the name '{name}' already exists.");
            }

            var table = new Table
            {
                Name = name,
                DisplayOrder = displayOrder,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Tables.Add(table);
            await _context.SaveChangesAsync();

            return table;
        }

        public async Task<Table> UpdateTableAsync(Guid id, string name, int displayOrder, bool isActive)
        {
            var table = await _context.Tables.FindAsync(id);
            if (table == null)
            {
                throw new InvalidOperationException("Table not found.");
            }

            // Check if name conflicts with another table
            var existing = await _context.Tables
                .FirstOrDefaultAsync(t => t.Name == name && t.Id != id);
            
            if (existing != null)
            {
                throw new InvalidOperationException($"A table with the name '{name}' already exists.");
            }

            // Check if table has active session
            if (!isActive)
            {
                var hasActiveSession = await _context.Sessions
                    .AnyAsync(s => s.TableId == id && s.Status == SessionStatus.IN_PROGRESS);
                
                if (hasActiveSession)
                {
                    throw new InvalidOperationException("Cannot deactivate a table with an active session.");
                }
            }

            table.Name = name;
            table.DisplayOrder = displayOrder;
            table.IsActive = isActive;

            await _context.SaveChangesAsync();
            return table;
        }

        public async Task DeleteTableAsync(Guid id)
        {
            var table = await _context.Tables.FindAsync(id);
            if (table == null)
            {
                throw new InvalidOperationException("Table not found.");
            }

            // Check if table has active session
            var hasActiveSession = await _context.Sessions
                .AnyAsync(s => s.TableId == id && s.Status == SessionStatus.IN_PROGRESS);
            
            if (hasActiveSession)
            {
                throw new InvalidOperationException("Cannot delete a table with an active session. Please end the session first.");
            }

            // Soft delete - set IsActive to false
            table.IsActive = false;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasActiveSessionAsync(Guid tableId)
        {
            return await _context.Sessions
                .AnyAsync(s => s.TableId == tableId && s.Status == SessionStatus.IN_PROGRESS);
        }

        public async Task<Session?> GetActiveSessionForTableAsync(Guid tableId)
        {
            return await _context.Sessions
                .Include(s => s.GameType)
                .Include(s => s.Frames)
                    .ThenInclude(f => f.Participants)
                    .ThenInclude(p => p.Customer)
                .FirstOrDefaultAsync(s => s.TableId == tableId && s.Status == SessionStatus.IN_PROGRESS);
        }

        public async Task UpdateDisplayOrdersAsync(List<(Guid Id, int Order)> orders)
        {
            foreach (var (id, order) in orders)
            {
                var table = await _context.Tables.FindAsync(id);
                if (table != null)
                {
                    table.DisplayOrder = order;
                }
            }
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null)
        {
            var query = _context.Tables.Where(t => t.Name == name);
            if (excludeId.HasValue)
            {
                query = query.Where(t => t.Id != excludeId.Value);
            }
            return !await query.AnyAsync();
        }

        public async Task<int> GetNextDisplayOrderAsync()
        {
            var maxOrder = await _context.Tables
                .OrderByDescending(t => t.DisplayOrder)
                .Select(t => t.DisplayOrder)
                .FirstOrDefaultAsync();
            
            return maxOrder + 1;
        }
    }
}
