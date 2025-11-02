using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mist.Services;
using System.Security.Claims;

namespace mist.Controllers
{
    [Authorize]
    public class LibraryController : Controller
    {
        private readonly IPurchaseService _purchaseService;

        public LibraryController(IPurchaseService purchaseService)
        {
            _purchaseService = purchaseService;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        // GET: Library
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var games = await _purchaseService.GetUserOwnedGamesAsync(userId);
            return View(games);
        }

        // GET: Library/Purchases
        public async Task<IActionResult> Purchases()
        {
            var userId = GetUserId();
            var purchases = await _purchaseService.GetUserPurchasesAsync(userId);
            return View(purchases);
        }
    }
}