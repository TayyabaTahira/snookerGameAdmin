using System;

namespace SnookerGameManagementSystem.Models
{
    public class GameType
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public int? MinPlayers { get; set; } = 2;
        public int? MaxPlayers { get; set; } = 4;
        
        // Navigation properties
        public virtual ICollection<GameRule> GameRules { get; set; } = new List<GameRule>();
        public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
    }
}
