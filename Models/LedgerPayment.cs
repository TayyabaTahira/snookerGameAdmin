using System;

namespace SnookerGameManagementSystem.Models
{
    public class LedgerPayment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CustomerId { get; set; }
        public decimal AmountPk { get; set; }
        public string? Method { get; set; } = "Cash";
        public DateTime ReceivedAt { get; set; } = DateTime.Now;
        
        // Navigation properties
        public virtual Customer? Customer { get; set; }
        public virtual ICollection<PaymentAllocation> PaymentAllocations { get; set; } = new List<PaymentAllocation>();
        
        // Available payment methods
        public static readonly List<string> AvailablePaymentMethods = new()
        {
            "Cash",
            "Bank Transfer",
            "EasyPaisa",
            "JazzCash",
            "Card",
            "Other"
        };
    }
}
