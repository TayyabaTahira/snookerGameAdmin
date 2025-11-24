using System;

namespace SnookerGameManagementSystem.Models
{
    public class PaymentAllocation
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid PaymentId { get; set; }
        public Guid ChargeId { get; set; }
        public decimal AllocatedAmountPk { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Navigation properties
        public virtual LedgerPayment? Payment { get; set; }
        public virtual LedgerCharge? Charge { get; set; }
    }
}
