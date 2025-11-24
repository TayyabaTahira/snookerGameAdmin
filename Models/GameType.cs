using System;

namespace SnookerGameManagementSystem.Models
{
    public class GameType
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        
        // Navigation properties
        public virtual ICollection<GameRule> GameRules { get; set; } = new List<GameRule>();
        public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
    }
}
