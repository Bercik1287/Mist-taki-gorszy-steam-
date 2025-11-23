using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mist.Services;
using System.Security.Claims;

namespace mist.Controllers
{
    [Authorize]
    public class WishlistController : Controller
    {
        private readonly IWishlistService _wishlistService;

        public WishlistController(IWishlistService wishlistService)
        {
            _wishlistService = wishlistService;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        // GET: Wishlist
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var wishlist = await _wishlistService.GetUserWishlistAsync(userId);
            return View(wishlist);
        }

        // POST: Wishlist/AddToWishlist
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToWishlist(int gameId)
        {
            var userId = GetUserId();
            var result = await _wishlistService.AddToWishlistAsync(userId, gameId);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction("Details", "Games", new { id = gameId });
        }

        // POST: Wishlist/RemoveFromWishlist
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromWishlist(int gameId)
        {
            var userId = GetUserId();
            var result = await _wishlistService.RemoveFromWishlistAsync(userId, gameId);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}