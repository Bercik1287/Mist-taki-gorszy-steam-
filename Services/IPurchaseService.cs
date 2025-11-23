using mist.Models;

namespace mist.Services
{
    public interface IPurchaseService
    {
        Task<(bool Success, string Message, List<Purchase> Purchases)> ProcessCheckoutAsync(int userId);
        Task<List<Purchase>> GetUserPurchasesAsync(int userId);
        Task<List<Game>> GetUserOwnedGamesAsync(int userId);
    }
}