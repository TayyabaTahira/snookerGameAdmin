using Microsoft.EntityFrameworkCore;
using SnookerGameManagementSystem.Data;
using SnookerGameManagementSystem.Models;

namespace SnookerGameManagementSystem.Services
{
    public class LedgerService
    {
        private readonly SnookerDbContext _context;

        public LedgerService(SnookerDbContext context)
        {
            _context = context;
        }

        public async Task<List<LedgerChargeWithBalance>> GetUnpaidChargesAsync(Guid customerId)
        {
            var charges = await _context.LedgerCharges
                .Where(c => c.CustomerId == customerId)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            var result = new List<LedgerChargeWithBalance>();

            foreach (var charge in charges)
            {
                var alreadyPaid = await _context.PaymentAllocations
                    .Where(pa => pa.ChargeId == charge.Id)
                    .SumAsync(pa => pa.AllocatedAmountPk);

                var remaining = charge.AmountPk - alreadyPaid;

                if (remaining > 0)
                {
                    result.Add(new LedgerChargeWithBalance
                    {
                        Charge = charge,
                        AlreadyPaid = alreadyPaid,
                        RemainingAmount = remaining
                    });
                }
            }

            return result;
        }

        public async Task<bool> ProcessPaymentAsync(Guid customerId, decimal amount, string method)
        {
            System.Diagnostics.Debug.WriteLine($"[LedgerService] ========== ProcessPaymentAsync START ==========");
            System.Diagnostics.Debug.WriteLine($"[LedgerService] Customer ID: {customerId}");
            System.Diagnostics.Debug.WriteLine($"[LedgerService] Amount: {amount}");
            System.Diagnostics.Debug.WriteLine($"[LedgerService] Method: {method}");
            
            // Use execution strategy to wrap the transaction
            var strategy = _context.Database.CreateExecutionStrategy();
            
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // 1. Create payment record
                    System.Diagnostics.Debug.WriteLine($"[LedgerService] Creating payment record...");
                    var payment = new LedgerPayment
                    {
                        Id = Guid.NewGuid(),
                        CustomerId = customerId,
                        AmountPk = amount,
                        Method = method,
                        ReceivedAt = DateTime.Now
                    };
                    _context.LedgerPayments.Add(payment);
                    await _context.SaveChangesAsync();
                    System.Diagnostics.Debug.WriteLine($"[LedgerService] Payment record created with ID: {payment.Id}");

                    // 2. Get unpaid charges (FIFO)
                    System.Diagnostics.Debug.WriteLine($"[LedgerService] Getting unpaid charges...");
                    var unpaidCharges = await GetUnpaidChargesAsync(customerId);
                    System.Diagnostics.Debug.WriteLine($"[LedgerService] Found {unpaidCharges.Count} unpaid charges");

                    // 3. Allocate payment FIFO
                    decimal remainingPayment = amount;
                    int allocationCount = 0;

                    foreach (var chargeInfo in unpaidCharges)
                    {
                        if (remainingPayment <= 0) break;

                        var chargeRemaining = chargeInfo.RemainingAmount;
                        System.Diagnostics.Debug.WriteLine($"[LedgerService] Processing charge {chargeInfo.Charge.Id}, remaining: {chargeRemaining}");

                        if (chargeRemaining <= 0) continue;

                        // Allocate what we can to this charge
                        var toAllocate = Math.Min(remainingPayment, chargeRemaining);

                        var allocation = new PaymentAllocation
                        {
                            Id = Guid.NewGuid(),
                            PaymentId = payment.Id,
                            ChargeId = chargeInfo.Charge.Id,
                            AllocatedAmountPk = toAllocate,
                            CreatedAt = DateTime.Now
                        };
                        _context.PaymentAllocations.Add(allocation);
                        allocationCount++;
                        System.Diagnostics.Debug.WriteLine($"[LedgerService] Allocated {toAllocate} to charge {chargeInfo.Charge.Id}");

                        remainingPayment -= toAllocate;

                        // Update frame pay status
                        await UpdateFramePayStatusAsync(chargeInfo.Charge.FrameId);
                    }

                    System.Diagnostics.Debug.WriteLine($"[LedgerService] Created {allocationCount} allocations");
                    System.Diagnostics.Debug.WriteLine($"[LedgerService] Saving changes...");
                    await _context.SaveChangesAsync();
                    System.Diagnostics.Debug.WriteLine($"[LedgerService] Committing transaction...");
                    await transaction.CommitAsync();
                    System.Diagnostics.Debug.WriteLine($"[LedgerService] ========== ProcessPaymentAsync SUCCESS ==========");
                    return true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[LedgerService] ========== ProcessPaymentAsync FAILED ==========");
                    System.Diagnostics.Debug.WriteLine($"[LedgerService] Error: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"[LedgerService] Stack trace: {ex.StackTrace}");
                    if (ex.InnerException != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"[LedgerService] Inner exception: {ex.InnerException.Message}");
                        System.Diagnostics.Debug.WriteLine($"[LedgerService] Inner stack: {ex.InnerException.StackTrace}");
                    }
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        private async Task UpdateFramePayStatusAsync(Guid? frameId)
        {
            if (frameId == null) return;

            var frame = await _context.Frames
                .Include(f => f.LedgerCharges)
                    .ThenInclude(c => c.PaymentAllocations)
                .FirstOrDefaultAsync(f => f.Id == frameId);

            if (frame == null) return;

            // Calculate total charges and total paid for this frame
            decimal totalCharges = frame.LedgerCharges.Sum(c => c.AmountPk);
            decimal totalPaid = frame.LedgerCharges
                .SelectMany(c => c.PaymentAllocations)
                .Sum(pa => pa.AllocatedAmountPk);

            if (totalPaid >= totalCharges)
            {
                frame.PayStatus = PayStatus.PAID;
            }
            else if (totalPaid > 0)
            {
                frame.PayStatus = PayStatus.PARTIAL;
            }
            else
            {
                frame.PayStatus = PayStatus.UNPAID;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<decimal> GetCustomerBalanceAsync(Guid customerId)
        {
            var charges = await _context.LedgerCharges
                .Where(c => c.CustomerId == customerId)
                .SumAsync(c => c.AmountPk);

            var payments = await _context.LedgerPayments
                .Where(p => p.CustomerId == customerId)
                .SumAsync(p => p.AmountPk);

            return charges - payments;
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.LedgerPayments
                .Where(p => p.ReceivedAt >= startDate && p.ReceivedAt < endDate)
                .SumAsync(p => p.AmountPk);
        }

        public async Task<decimal> GetTotalOutstandingCreditAsync()
        {
            var totalCharges = await _context.LedgerCharges.SumAsync(c => c.AmountPk);
            var totalPayments = await _context.LedgerPayments.SumAsync(p => p.AmountPk);
            return Math.Max(0, totalCharges - totalPayments);
        }
    }

    public class LedgerChargeWithBalance
    {
        public LedgerCharge Charge { get; set; } = null!;
        public decimal AlreadyPaid { get; set; }
        public decimal RemainingAmount { get; set; }
    }
}
