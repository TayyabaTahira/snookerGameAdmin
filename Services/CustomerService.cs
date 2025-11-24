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

        public async Task<Customer> CreateCustomerAsync(string fullName, string? phone = null)
        {
            var customer = new Customer
            {
                FullName = fullName,
                Phone = phone
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return customer;
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
            var charges = await _context.LedgerCharges
                .Where(c => c.CustomerId == customerId)
                .SumAsync(c => c.AmountPk);

            var payments = await _context.LedgerPayments
                .Where(p => p.CustomerId == customerId)
                .SumAsync(p => p.AmountPk);

            return charges - payments;
        }
    }
}
