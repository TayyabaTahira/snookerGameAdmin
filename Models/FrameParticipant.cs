using System;

namespace SnookerGameManagementSystem.Models
{
    public enum Team
    {
        A,
        B
    }

    public class FrameParticipant
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid FrameId { get; set; }
        public Guid CustomerId { get; set; }
        public Team? Team { get; set; }
        public bool IsWinner { get; set; } = false;
        public decimal? SharePk { get; set; }
        
        // Navigation properties
        public virtual Frame? Frame { get; set; }
        public virtual Customer? Customer { get; set; }
    }
}
