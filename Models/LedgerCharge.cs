using System;

namespace SnookerGameManagementSystem.Models
{
    public class LedgerCharge
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CustomerId { get; set; }
        public Guid? FrameId { get; set; }
        public string Description { get; set; } = string.Empty;
        
        // Database mapped field
        public decimal AmountPk { get; set; }
        
        // Convenience properties
        public decimal Amount 
        { 
            get => AmountPk; 
            set => AmountPk = value; 
        }
        
        public DateTime ChargedAt 
        { 
            get => CreatedAt; 
            set => CreatedAt = value; 
        }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Navigation properties
        public virtual Customer? Customer { get; set; }
        public virtual Frame? Frame { get; set; }
        public virtual ICollection<PaymentAllocation> PaymentAllocations { get; set; } = new List<PaymentAllocation>();
    }
}
