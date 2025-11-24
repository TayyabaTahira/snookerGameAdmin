using System;

namespace SnookerGameManagementSystem.Models
{
    public class LedgerPayment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CustomerId { get; set; }
        public decimal AmountPk { get; set; }
        public string? Method { get; set; } = "cash";
        public DateTime ReceivedAt { get; set; } = DateTime.Now;
        
        // Navigation properties
        public virtual Customer? Customer { get; set; }
        public virtual ICollection<PaymentAllocation> PaymentAllocations { get; set; } = new List<PaymentAllocation>();
    }
}
