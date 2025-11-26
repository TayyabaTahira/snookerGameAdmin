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
        EACH,
        CUSTOM
    }

    public class GameRule
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid GameTypeId { get; set; }
        
        // Display/description fields (not mapped to DB)
        public string Description { get; set; } = string.Empty;
        
        // Database mapped fields
        public decimal BaseRatePk { get; set; }
        
        // Convenience properties that map to DB fields
        public decimal BaseRate 
        { 
            get => BaseRatePk; 
            set => BaseRatePk = value; 
        }
        
        public decimal OvertimeRate 
        { 
            get => OvertimeRatePkMin ?? 0; 
            set => OvertimeRatePkMin = value; 
        }
        
        public int? TimeLimitMinutes { get; set; }
        public OvertimeMode OvertimeMode { get; set; } = OvertimeMode.NONE;
        public decimal? OvertimeRatePkMin { get; set; }
        public decimal? OvertimeLumpSumPk { get; set; }
        public PayerMode DefaultPayerMode { get; set; } = PayerMode.LOSER;
        
        // Navigation properties
        public virtual GameType? GameType { get; set; }
    }
}
