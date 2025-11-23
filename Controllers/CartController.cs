using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mist.Services;
using System.Security.Claims;

namespace mist.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IPurchaseService _purchaseService;

        public CartController(ICartService cartService, IPurchaseService purchaseService)
        {
            _cartService = cartService;
            _purchaseService = purchaseService;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        // GET: Cart
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var cart = await _cartService.GetOrCreateCartAsync(userId);
            return View(cart);
        }

        // POST: Cart/AddToCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int gameId)
        {
            var userId = GetUserId();
            var result = await _cartService.AddToCartAsync(userId, gameId);

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

        // POST: Cart/RemoveFromCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int gameId)
        {
            var userId = GetUserId();
            var result = await _cartService.RemoveFromCartAsync(userId, gameId);

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

        // POST: Cart/Clear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Clear()
        {
            var userId = GetUserId();
            var result = await _cartService.ClearCartAsync(userId);

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

        // GET: Cart/Checkout
        public async Task<IActionResult> Checkout()
        {
            var userId = GetUserId();
            var cart = await _cartService.GetOrCreateCartAsync(userId);

            if (!cart.CartItems.Any())
            {
                TempData["ErrorMessage"] = "Koszyk jest pusty";
                return RedirectToAction(nameof(Index));
            }

            return View(cart);
        }

        // POST: Cart/ProcessCheckout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessCheckout()
        {
            var userId = GetUserId();
            var result = await _purchaseService.ProcessCheckoutAsync(userId);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction("OrderConfirmation", new { purchaseIds = string.Join(",", result.Purchases.Select(p => p.Id)) });
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Checkout));
            }
        }

        // GET: Cart/OrderConfirmation
        public IActionResult OrderConfirmation(string purchaseIds)
        {
            ViewBag.PurchaseIds = purchaseIds;
            return View();
        }
    }
}