using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SnookerGameManagementSystem.Data;
using SnookerGameManagementSystem.Services;

namespace SnookerGameManagementSystem
{
    /// <summary>
    /// Simple test program to verify database and authentication
    /// </summary>
    public class DiagnosticTest
    {
        public static async Task RunDiagnostics()
        {
            Console.WriteLine("========================================");
            Console.WriteLine("DATABASE & AUTH DIAGNOSTIC TEST");
            Console.WriteLine("========================================\n");

            try
            {
                // Load configuration
                Console.WriteLine("[1/5] Loading configuration...");
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false)
                    .Build();

                var connectionString = configuration.GetConnectionString("SnookerDb");
                Console.WriteLine($"      Connection: {connectionString?.Split(";")[0]}");
                Console.WriteLine("      ? Configuration loaded\n");

                // Create DbContext
                Console.WriteLine("[2/5] Creating database context...");
                var optionsBuilder = new DbContextOptionsBuilder<SnookerDbContext>();
                optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
                
                using var context = new SnookerDbContext(optionsBuilder.Options);
                Console.WriteLine("      ? DbContext created\n");

                // Test connection
                Console.WriteLine("[3/5] Testing database connection...");
                var canConnect = await context.Database.CanConnectAsync();
                if (canConnect)
                {
                    Console.WriteLine("      ? Database connection successful\n");
                }
                else
                {
                    Console.WriteLine("      ? Cannot connect to database\n");
                    return;
                }

                // Check tables
                Console.WriteLine("[4/5] Checking database tables...");
                var userCount = await context.AppUsers.CountAsync();
                var customerCount = await context.Customers.CountAsync();
                var gameTypeCount = await context.GameTypes.CountAsync();
                
                Console.WriteLine($"      Users: {userCount}");
                Console.WriteLine($"      Customers: {customerCount}");
                Console.WriteLine($"      Game Types: {gameTypeCount}");
                Console.WriteLine("      ? Tables accessible\n");

                // Test authentication
                Console.WriteLine("[5/5] Testing authentication...");
                var authService = new AuthService(context);
                
                Console.Write("      Testing with username 'admin' and password 'admin123'...");
                var user = await authService.AuthenticateAsync("admin", "admin123");
                
                if (user != null)
                {
                    Console.WriteLine(" ?");
                    Console.WriteLine($"      Authenticated user: {user.Username}");
                    Console.WriteLine($"      User ID: {user.Id}");
                    Console.WriteLine($"      Created: {user.CreatedAt}");
                }
                else
                {
                    Console.WriteLine(" ?");
                    Console.WriteLine("      Authentication FAILED!");
                    
                    // Try to get the user
                    var existingUser = await context.AppUsers.FirstOrDefaultAsync(u => u.Username == "admin");
                    if (existingUser != null)
                    {
                        Console.WriteLine($"      User exists but password doesn't match");
                        Console.WriteLine($"      Hash in DB: {existingUser.PasswordHash.Substring(0, 30)}...");
                    }
                    else
                    {
                        Console.WriteLine("      User 'admin' does NOT exist in database!");
                    }
                }

                Console.WriteLine("\n========================================");
                Console.WriteLine("DIAGNOSTIC COMPLETE");
                Console.WriteLine("========================================");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n? ERROR: {ex.Message}");
                Console.WriteLine($"\nStack Trace:\n{ex.StackTrace}");
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
