using Microsoft.EntityFrameworkCore;
using mist.Data;
using mist.Models;

namespace mist.Services
{
    public class ReviewService : IReviewService
    {
        private readonly ApplicationDbContext _context;

        public ReviewService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CanUserReviewAsync(int userId, int gameId)
        {
            // Użytkownik może wystawiać recenzje tylko jeśli posiada grę
            return await _context.Purchases
                .AnyAsync(p => p.UserId == userId && p.GameId == gameId);
        }

        public async Task<(bool Success, string Message, Review Review)> AddReviewAsync(int userId, int gameId, int rating, string title, string content)
        {
            // Sprawdź czy użytkownik posiada grę
            if (!await CanUserReviewAsync(userId, gameId))
            {
                return (false, "Możesz wystawiać recenzje tylko dla gier, które posiadasz", null);
            }

            // Sprawdź czy już ma recenzję dla tej gry
            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.UserId == userId && r.GameId == gameId);

            if (existingReview != null)
            {
                return (false, "Już wystawiłeś recenzję dla tej gry", null);
            }

            var game = await _context.Games.FindAsync(gameId);
            if (game == null)
            {
                return (false, "Gra nie została znaleziona", null);
            }

            var review = new Review
            {
                UserId = userId,
                GameId = gameId,
                Rating = rating,
                Title = title,
                Content = content,
                IsVerifiedPurchase = true,
                CreatedAt = DateTime.Now
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return (true, "Recenzja została dodana", review);
        }

        public async Task<(bool Success, string Message)> UpdateReviewAsync(int reviewId, int userId, int rating, string title, string content)
        {
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review == null)
            {
                return (false, "Recenzja nie została znaleziona");
            }

            if (review.UserId != userId)
            {
                return (false, "Nie możesz edytować cudzej recenzji");
            }

            review.Rating = rating;
            review.Title = title;
            review.Content = content;
            review.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return (true, "Recenzja została zaktualizowana");
        }

        public async Task<(bool Success, string Message)> DeleteReviewAsync(int reviewId, int userId)
        {
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review == null)
            {
                return (false, "Recenzja nie została znaleziona");
            }

            if (review.UserId != userId)
            {
                return (false, "Nie możesz usunąć cudzej recenzji");
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return (true, "Recenzja została usunięta");
        }

        public async Task<List<Review>> GetGameReviewsAsync(int gameId)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.GameId == gameId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<Review> GetUserReviewAsync(int userId, int gameId)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.UserId == userId && r.GameId == gameId);
        }
    }
}