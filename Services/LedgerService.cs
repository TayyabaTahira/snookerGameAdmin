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
            try
            {
                // 1. Create payment record
                var payment = new LedgerPayment
                {
                    CustomerId = customerId,
                    AmountPk = amount,
                    Method = method,
                    ReceivedAt = DateTime.Now
                };
                _context.LedgerPayments.Add(payment);
                await _context.SaveChangesAsync();

                // 2. Get unpaid charges (FIFO)
                var unpaidCharges = await GetUnpaidChargesAsync(customerId);

                // 3. Allocate payment FIFO
                decimal remainingPayment = amount;

                foreach (var chargeInfo in unpaidCharges)
                {
                    if (remainingPayment <= 0) break;

                    var chargeRemaining = chargeInfo.RemainingAmount;

                    if (chargeRemaining <= 0) continue;

                    // Allocate what we can to this charge
                    var toAllocate = Math.Min(remainingPayment, chargeRemaining);

                    var allocation = new PaymentAllocation
                    {
                        PaymentId = payment.Id,
                        ChargeId = chargeInfo.Charge.Id,
                        AllocatedAmountPk = toAllocate,
                        CreatedAt = DateTime.Now
                    };
                    _context.PaymentAllocations.Add(allocation);

                    remainingPayment -= toAllocate;

                    // Update frame pay status
                    await UpdateFramePayStatusAsync(chargeInfo.Charge.FrameId);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
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
