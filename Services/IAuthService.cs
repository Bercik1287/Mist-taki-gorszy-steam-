using mist.Models;
using mist.ViewModels;

namespace mist.Services
{
    public interface IAuthService
    {
        Task<(bool Success, string Message, User User)> RegisterAsync(RegisterViewModel model);
        Task<(bool Success, string Message, User User)> LoginAsync(LoginViewModel model);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
    }
}