using System;

namespace SnookerGameManagementSystem.Models
{
    public enum OvertimeMode
    {
        PER_MINUTE,
        LUMP_SUM,
        NONE
    }

    public enum PayerMode
    {
        LOSER,
        SPLIT,
        CUSTOM
    }

    public class GameRule
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid GameTypeId { get; set; }
        public decimal BaseRatePk { get; set; }
        public int? TimeLimitMinutes { get; set; }
        public OvertimeMode OvertimeMode { get; set; } = OvertimeMode.NONE;
        public decimal? OvertimeRatePkMin { get; set; }
        public decimal? OvertimeLumpSumPk { get; set; }
        public PayerMode DefaultPayerMode { get; set; } = PayerMode.LOSER;
        
        // Navigation properties
        public virtual GameType? GameType { get; set; }
    }
}
