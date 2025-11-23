using mist.Models;

namespace mist.Services
{
    public interface IWishlistService
    {
        Task<(bool Success, string Message)> AddToWishlistAsync(int userId, int gameId);
        Task<(bool Success, string Message)> RemoveFromWishlistAsync(int userId, int gameId);
        Task<List<WishlistItem>> GetUserWishlistAsync(int userId);
        Task<bool> IsInWishlistAsync(int userId, int gameId);
        Task<(bool Success, string Message)> RemoveFromWishlistIfOwnedAsync(int userId, int gameId);
    }
}