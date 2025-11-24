using System;

namespace SnookerGameManagementSystem.Models
{
    public enum PayStatus
    {
        UNPAID,
        PARTIAL,
        PAID
    }

    public class Frame
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid SessionId { get; set; }
        public DateTime StartedAt { get; set; } = DateTime.Now;
        public DateTime? EndedAt { get; set; }
        public Guid? WinnerCustomerId { get; set; }
        public Guid? LoserCustomerId { get; set; }
        public decimal BaseRatePk { get; set; }
        public int OvertimeMinutes { get; set; } = 0;
        public decimal OvertimeAmountPk { get; set; } = 0;
        public decimal LumpSumFinePk { get; set; } = 0;
        public decimal DiscountPk { get; set; } = 0;
        public decimal TotalAmountPk { get; set; } = 0;
        public PayerMode PayerMode { get; set; } = PayerMode.LOSER;
        public PayStatus PayStatus { get; set; } = PayStatus.UNPAID;
        
        // Navigation properties
        public virtual Session? Session { get; set; }
        public virtual Customer? WinnerCustomer { get; set; }
        public virtual Customer? LoserCustomer { get; set; }
        public virtual ICollection<FrameParticipant> Participants { get; set; } = new List<FrameParticipant>();
        public virtual ICollection<LedgerCharge> LedgerCharges { get; set; } = new List<LedgerCharge>();
    }
}
