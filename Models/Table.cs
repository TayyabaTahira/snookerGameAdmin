using System;

namespace SnookerGameManagementSystem.Models
{
    public class Table
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public int DisplayOrder { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
    }
}
