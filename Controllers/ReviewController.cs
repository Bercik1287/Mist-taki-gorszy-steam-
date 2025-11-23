using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mist.Data;
using mist.Models;
using mist.Services;
using mist.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace mist.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly IReviewService _reviewService;
        private readonly ApplicationDbContext _context;

        public ReviewController(IReviewService reviewService, ApplicationDbContext context)
        {
            _reviewService = reviewService;
            _context = context;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        // GET: Review/Create/5
        public async Task<IActionResult> Create(int gameId)
        {
            var userId = GetUserId();
            
            // Sprawdź czy użytkownik posiada grę
            if (!await _reviewService.CanUserReviewAsync(userId, gameId))
            {
                TempData["ErrorMessage"] = "Możesz wystawiać recenzje tylko dla gier, które posiadasz";
                return RedirectToAction("Details", "Games", new { id = gameId });
            }

            // Sprawdź czy już ma recenzję
            var existingReview = await _reviewService.GetUserReviewAsync(userId, gameId);
            if (existingReview != null)
            {
                TempData["ErrorMessage"] = "Już wystawiłeś recenzję dla tej gry";
                return RedirectToAction("Edit", new { id = existingReview.Id });
            }

            var game = await _context.Games.FindAsync(gameId);
            if (game == null)
            {
                return NotFound();
            }

            var model = new CreateReviewViewModel { GameId = gameId };
            ViewBag.GameTitle = game.Title;
            return View(model);
        }

        // POST: Review/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateReviewViewModel model)
        {
            var userId = GetUserId();

            if (!ModelState.IsValid)
            {
                var game = await _context.Games.FindAsync(model.GameId);
                ViewBag.GameTitle = game?.Title;
                return View(model);
            }

            var result = await _reviewService.AddReviewAsync(userId, model.GameId, model.Rating, model.Title, model.Content);

            if (result.Success)
            {
                TempData["SuccessMessage"] = "Recenzja została dodana";
                return RedirectToAction("Details", "Games", new { id = model.GameId });
            }

            TempData["ErrorMessage"] = result.Message;
            return RedirectToAction("Details", "Games", new { id = model.GameId });
        }

        // GET: Review/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var userId = GetUserId();
            var review = await _context.Reviews
                .Include(r => r.Game)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review == null)
            {
                return NotFound();
            }

            if (review.UserId != userId)
            {
                return Forbid();
            }

            var model = new EditReviewViewModel
            {
                Id = review.Id,
                GameId = review.GameId,
                Rating = review.Rating,
                Title = review.Title,
                Content = review.Content
            };

            ViewBag.GameTitle = review.Game.Title;
            return View(model);
        }

        // POST: Review/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditReviewViewModel model)
        {
            var userId = GetUserId();
            var review = await _context.Reviews.FindAsync(id);

            if (review == null)
            {
                return NotFound();
            }

            if (review.UserId != userId)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                var game = await _context.Games.FindAsync(review.GameId);
                ViewBag.GameTitle = game?.Title;
                return View(model);
            }

            var result = await _reviewService.UpdateReviewAsync(id, userId, model.Rating, model.Title, model.Content);

            if (result.Success)
            {
                TempData["SuccessMessage"] = "Recenzja została zaktualizowana";
                return RedirectToAction("Details", "Games", new { id = review.GameId });
            }

            TempData["ErrorMessage"] = result.Message;
            return View(model);
        }

        // POST: Review/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();
            var review = await _context.Reviews.FindAsync(id);

            if (review == null)
            {
                return NotFound();
            }

            if (review.UserId != userId)
            {
                return Forbid();
            }

            var gameId = review.GameId;
            var result = await _reviewService.DeleteReviewAsync(id, userId);

            if (result.Success)
            {
                TempData["SuccessMessage"] = "Recenzja została usunięta";
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction("Details", "Games", new { id = gameId });
        }
    }
}