using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IO;
using SnookerGameManagementSystem.Data;
using SnookerGameManagementSystem.Services;

// Quick test program
var config = new ConfigurationBuilder()
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
    .AddJsonFile("appsettings.json")
    .Build();

var connectionString = config.GetConnectionString("SnookerDb");

Console.WriteLine("Testing Database Connection and Authentication");
Console.WriteLine("==============================================\n");
Console.WriteLine($"Connection String: {connectionString}\n");

var optionsBuilder = new DbContextOptionsBuilder<SnookerDbContext>();
optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

using var context = new SnookerDbContext(optionsBuilder.Options);

// Test connection
Console.Write("Testing connection... ");
var canConnect = await context.Database.CanConnectAsync();
Console.WriteLine(canConnect ? "? SUCCESS" : "? FAILED");

if (!canConnect)
{
    Console.WriteLine("Cannot connect to database!");
    return;
}

// Count users
Console.Write("Counting users... ");
var userCount = await context.AppUsers.CountAsync();
Console.WriteLine($"{userCount} user(s) found");

// Get admin user
Console.Write("Finding admin user... ");
var adminUser = await context.AppUsers.FirstOrDefaultAsync(u => u.Username == "admin");
if (adminUser != null)
{
    Console.WriteLine("? FOUND");
    Console.WriteLine($"  Username: {adminUser.Username}");
    Console.WriteLine($"  Hash: {adminUser.PasswordHash.Substring(0, 30)}...");
}
else
{
    Console.WriteLine("? NOT FOUND");
    return;
}

// Test authentication
Console.WriteLine("\nTesting Authentication:");
var authService = new AuthService(context);

Console.Write("  Attempting login with admin/admin123... ");
var result = await authService.AuthenticateAsync("admin", "admin123");

if (result != null)
{
    Console.WriteLine("? SUCCESS!");
    Console.WriteLine($"  Authenticated as: {result.Username}");
    Console.WriteLine($"  User ID: {result.Id}");
}
else
{
    Console.WriteLine("? FAILED!");
    Console.WriteLine("  Password verification returned null");
    
    // Try verifying the hash manually
    Console.WriteLine("\n  Manual BCrypt test:");
    var isValid = BCrypt.Net.BCrypt.Verify("admin123", adminUser.PasswordHash);
    Console.WriteLine($"  BCrypt.Verify result: {isValid}");
}

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();
