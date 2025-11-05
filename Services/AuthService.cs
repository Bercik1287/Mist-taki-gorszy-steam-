using mist.Data;
using mist.Models;
using mist.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace mist.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;

        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string Message, User User)> RegisterAsync(RegisterViewModel model)
        {
            // Sprawdź czy username istnieje
            if (await _context.Users.AnyAsync(u => u.Username == model.Username))
            {
                return (false, "Nazwa użytkownika jest już zajęta", null);
            }

            // Sprawdź czy email istnieje
            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                return (false, "Email jest już zarejestrowany", null);
            }

            // Sprawdź czy to pierwszy użytkownik (automatyczny admin)
            var isFirstUser = !await _context.Users.AnyAsync();

            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                PasswordHash = HashPassword(model.Password),
                CreatedAt = DateTime.Now,
                Role = isFirstUser ? "Admin" : "User"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var message = isFirstUser 
                ? "Rejestracja zakończona sukcesem! Jako pierwszy użytkownik otrzymałeś uprawnienia administratora." 
                : "Rejestracja zakończona sukcesem";

            return (true, message, user);
        }

        public async Task<(bool Success, string Message, User User)> LoginAsync(LoginViewModel model)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == model.UsernameOrEmail || u.Email == model.UsernameOrEmail);

            if (user == null)
            {
                return (false, "Nieprawidłowy login lub hasło", null);
            }

            if (!VerifyPassword(model.Password, user.PasswordHash))
            {
                return (false, "Nieprawidłowy login lub hasło", null);
            }

            return (true, "Logowanie pomyślne", user);
        }

        public string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var saltedPassword = password + "mistSalt2024";
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
                return Convert.ToBase64String(bytes);
            }
        }

        public bool VerifyPassword(string password, string hash)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == hash;
        }
    }
}