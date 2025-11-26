using System;

namespace SnookerGameManagementSystem.Models
{
    public class Customer
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Computed property for display (not mapped to DB)
        public decimal Balance { get; set; } = 0;
        
        // Navigation properties
        public virtual ICollection<FrameParticipant> FrameParticipants { get; set; } = new List<FrameParticipant>();
        public virtual ICollection<LedgerCharge> LedgerCharges { get; set; } = new List<LedgerCharge>();
        public virtual ICollection<LedgerPayment> LedgerPayments { get; set; } = new List<LedgerPayment>();
    }
}
