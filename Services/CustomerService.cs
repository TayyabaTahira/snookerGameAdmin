using Microsoft.EntityFrameworkCore;
using SnookerGameManagementSystem.Data;
using SnookerGameManagementSystem.Models;

namespace SnookerGameManagementSystem.Services
{
    public class CustomerService
    {
        private readonly SnookerDbContext _context;

        public CustomerService(SnookerDbContext context)
        {
            _context = context;
        }

        public async Task<List<Customer>> GetAllCustomersAsync()
        {
            return await _context.Customers
                .OrderBy(c => c.FullName)
                .ToListAsync();
        }

        public async Task<Customer?> GetCustomerByIdAsync(Guid customerId)
        {
            return await _context.Customers.FindAsync(customerId);
        }

        public async Task<Customer?> FindCustomerByNameAsync(string name)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.FullName.ToLower() == name.ToLower());
        }

        public async Task<Customer> CreateCustomerAsync(string fullName, string? phone = null, decimal initialCredit = 0)
        {
            var customer = new Customer
            {
                FullName = fullName,
                Phone = phone,
                InitialCreditPk = initialCredit
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return customer;
        }

        public async Task<Customer> UpdateCustomerAsync(Customer customer)
        {
            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        public async Task DeleteCustomerAsync(Guid customerId)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Customer> GetOrCreateCustomerAsync(string fullName, string? phone = null)
        {
            var existing = await FindCustomerByNameAsync(fullName);
            if (existing != null)
                return existing;

            return await CreateCustomerAsync(fullName, phone);
        }

        public async Task<decimal> GetCustomerBalanceAsync(Guid customerId)
        {
            // Get customer's initial credit (old outstanding balance)
            var customer = await _context.Customers.FindAsync(customerId);
            var initialCredit = customer?.InitialCreditPk ?? 0;
            
            // Get total payments made by this customer
            var totalPayments = await _context.LedgerPayments
                .Where(p => p.CustomerId == customerId)
                .SumAsync(p => (decimal?)p.AmountPk) ?? 0;

            // Get total amount allocated to charges (game charges, not initial credit)
            var totalAllocatedToCharges = await _context.PaymentAllocations
                .Where(pa => pa.Charge.CustomerId == customerId)
                .SumAsync(pa => (decimal?)pa.AllocatedAmountPk) ?? 0;

            // Amount paid towards initial credit = total payments - amount allocated to charges
            var paidToInitialCredit = Math.Max(0, totalPayments - totalAllocatedToCharges);
            
            // Remaining initial credit
            var remainingInitialCredit = Math.Max(0, initialCredit - paidToInitialCredit);

            // Get all charges for this customer
            var charges = await _context.LedgerCharges
                .Where(c => c.CustomerId == customerId)
                .ToListAsync();

            decimal totalUnpaidCharges = 0;

            // For each charge, calculate how much is still unpaid
            foreach (var charge in charges)
            {
                var alreadyPaid = await _context.PaymentAllocations
                    .Where(pa => pa.ChargeId == charge.Id)
                    .SumAsync(pa => (decimal?)pa.AllocatedAmountPk) ?? 0;

                var outstanding = charge.AmountPk - alreadyPaid;
                if (outstanding > 0)
                {
                    totalUnpaidCharges += outstanding;
                }
            }

            // Total balance = remaining initial credit + unpaid charges
            return remainingInitialCredit + totalUnpaidCharges;
        }
    }
}
