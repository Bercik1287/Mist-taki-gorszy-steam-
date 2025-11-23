using mist.Models;

namespace mist.Services
{
    public interface IReviewService
    {
        Task<(bool Success, string Message, Review Review)> AddReviewAsync(int userId, int gameId, int rating, string title, string content);
        Task<(bool Success, string Message)> UpdateReviewAsync(int reviewId, int userId, int rating, string title, string content);
        Task<(bool Success, string Message)> DeleteReviewAsync(int reviewId, int userId);
        Task<List<Review>> GetGameReviewsAsync(int gameId);
        Task<Review> GetUserReviewAsync(int userId, int gameId);
        Task<bool> CanUserReviewAsync(int userId, int gameId);
    }
}