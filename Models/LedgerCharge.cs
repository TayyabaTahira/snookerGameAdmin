using System;

namespace SnookerGameManagementSystem.Models
{
    public class LedgerCharge
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CustomerId { get; set; }
        public Guid? FrameId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal AmountPk { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Navigation properties
        public virtual Customer? Customer { get; set; }
        public virtual Frame? Frame { get; set; }
        public virtual ICollection<PaymentAllocation> PaymentAllocations { get; set; } = new List<PaymentAllocation>();
    }
}
