using mist.Models;

namespace mist.Services
{
    public interface ICartService
    {
        Task<Cart> GetOrCreateCartAsync(int userId);
        Task<(bool Success, string Message)> AddToCartAsync(int userId, int gameId);
        Task<(bool Success, string Message)> RemoveFromCartAsync(int userId, int gameId);
        Task<(bool Success, string Message)> ClearCartAsync(int userId);
        Task<int> GetCartItemCountAsync(int userId);
        Task<bool> IsGameInCartAsync(int userId, int gameId);
        Task<bool> HasUserPurchasedGameAsync(int userId, int gameId);
    }
}