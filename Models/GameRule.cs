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
        
        // Display/description field - computed from rule properties
        public string Description 
        { 
            get
            {
                // Generate description based on payer mode
                var payerDesc = DefaultPayerMode switch
                {
                    PayerMode.LOSER => "Loser pays",
                    PayerMode.SPLIT => "Split between both players",
                    PayerMode.EACH => "Each player pays their own",
                    PayerMode.CUSTOM => "Custom payment allocation",
                    _ => "Payment mode not set"
                };

                // Add time limit info if available
                if (TimeLimitMinutes.HasValue && TimeLimitMinutes.Value > 0)
                {
                    payerDesc += $" • {TimeLimitMinutes.Value} min limit";
                    
                    // Add overtime info
                    if (OvertimeMode == OvertimeMode.PER_MINUTE && OvertimeRatePkMin.HasValue && OvertimeRatePkMin.Value > 0)
                    {
                        payerDesc += $" • PKR {OvertimeRatePkMin.Value:N2}/min overtime";
                    }
                    else if (OvertimeMode == OvertimeMode.LUMP_SUM && OvertimeLumpSumPk.HasValue && OvertimeLumpSumPk.Value > 0)
                    {
                        payerDesc += $" • PKR {OvertimeLumpSumPk.Value:N2} lump sum";
                    }
                }

                return payerDesc;
            }
        }
        
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
