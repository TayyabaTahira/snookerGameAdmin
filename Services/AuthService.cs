using Microsoft.EntityFrameworkCore;
using SnookerGameManagementSystem.Data;
using SnookerGameManagementSystem.Models;
using System.Diagnostics;

namespace SnookerGameManagementSystem.Services
{
    public class AuthService
    {
        private readonly SnookerDbContext _context;

        public AuthService(SnookerDbContext context)
        {
            _context = context;
        }

        public async Task<AppUser?> AuthenticateAsync(string username, string password)
        {
            try
            {
                Debug.WriteLine($"[AuthService] Attempting authentication for user: {username}");
                
                var user = await _context.AppUsers
                    .FirstOrDefaultAsync(u => u.Username == username);

                if (user == null)
                {
                    Debug.WriteLine($"[AuthService] User '{username}' not found in database");
                    return null;
                }

                Debug.WriteLine($"[AuthService] User found. Hash: {user.PasswordHash.Substring(0, 20)}...");

                // Verify password using BCrypt
                bool isValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
                
                Debug.WriteLine($"[AuthService] Password verification result: {isValid}");
                
                return isValid ? user : null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AuthService] ERROR: {ex.Message}");
                Debug.WriteLine($"[AuthService] Stack: {ex.StackTrace}");
                throw;
            }
        }

        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, 12);
        }
    }
}
