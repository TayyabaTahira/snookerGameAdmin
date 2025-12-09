using System;

namespace SnookerGameManagementSystem.Models
{
    public enum SessionStatus
    {
        IN_PROGRESS,
        ENDED
    }

    public class Session
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid? TableId { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid? GameTypeId { get; set; }
        public DateTime StartedAt { get; set; } = DateTime.Now;
        public DateTime? EndedAt { get; set; }
        public SessionStatus Status { get; set; } = SessionStatus.IN_PROGRESS;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Navigation properties
        public virtual Table? Table { get; set; }
        public virtual GameType? GameType { get; set; }
        public virtual ICollection<Frame> Frames { get; set; } = new List<Frame>();
    }
}
