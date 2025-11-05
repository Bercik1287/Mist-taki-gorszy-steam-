using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mist.Data;
using mist.Models;
using mist.ViewModels;

namespace mist.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Dashboard
        public async Task<IActionResult> Index()
        {
            var stats = new AdminDashboardViewModel
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalGames = await _context.Games.CountAsync(),
                TotalPurchases = await _context.Purchases.CountAsync(),
                TotalRevenue = await _context.Purchases.SumAsync(p => p.PricePaid),
                ActivePromotions = await _context.Promotions.CountAsync(p => p.IsActive && p.StartDate <= DateTime.Now && p.EndDate >= DateTime.Now),
                RecentPurchases = await _context.Purchases
                    .Include(p => p.User)
                    .Include(p => p.Game)
                    .OrderByDescending(p => p.PurchaseDate)
                    .Take(10)
                    .ToListAsync()
            };

            return View(stats);
        }

        // === ZARZĄDZANIE UŻYTKOWNIKAMI ===

        public async Task<IActionResult> Users()
        {
            var users = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
            return View(users);
        }

        public async Task<IActionResult> UserDetails(int id)
        {
            var user = await _context.Users
                .Include(u => u.Purchases)
                .ThenInclude(p => p.Game)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserRole(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Sprawdź czy to nie jedyny admin
            if (user.Role == "Admin")
            {
                var adminCount = await _context.Users.CountAsync(u => u.Role == "Admin");
                if (adminCount <= 1)
                {
                    TempData["ErrorMessage"] = "Nie można zmienić roli jedynego administratora!";
                    return RedirectToAction(nameof(Users));
                }
            }

            user.Role = user.Role == "Admin" ? "User" : "Admin";
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Rola użytkownika {user.Username} została zmieniona na {user.Role}";
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Sprawdź czy to nie jedyny admin
            if (user.Role == "Admin")
            {
                var adminCount = await _context.Users.CountAsync(u => u.Role == "Admin");
                if (adminCount <= 1)
                {
                    TempData["ErrorMessage"] = "Nie można usunąć jedynego administratora!";
                    return RedirectToAction(nameof(Users));
                }
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Użytkownik został usunięty";
            return RedirectToAction(nameof(Users));
        }

        // === ZARZĄDZANIE PROMOCJAMI ===

        public async Task<IActionResult> Promotions()
        {
            var promotions = await _context.Promotions
                .Include(p => p.Game)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            return View(promotions);
        }

        public async Task<IActionResult> CreatePromotion()
        {
            ViewBag.Games = await _context.Games.Where(g => g.IsActive).ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePromotion(Promotion promotion)
        {
            if (ModelState.IsValid)
            {
                // Walidacja dat
                if (promotion.EndDate <= promotion.StartDate)
                {
                    ModelState.AddModelError("EndDate", "Data zakończenia musi być późniejsza niż data rozpoczęcia");
                    ViewBag.Games = await _context.Games.Where(g => g.IsActive).ToListAsync();
                    return View(promotion);
                }

                // Walidacja wartości rabatu
                if (promotion.DiscountType == DiscountType.Percentage && promotion.DiscountValue > 100)
                {
                    ModelState.AddModelError("DiscountValue", "Rabat procentowy nie może być większy niż 100%");
                    ViewBag.Games = await _context.Games.Where(g => g.IsActive).ToListAsync();
                    return View(promotion);
                }

                var game = await _context.Games.FindAsync(promotion.GameId);
                if (promotion.DiscountType == DiscountType.FixedAmount && promotion.DiscountValue >= game.Price)
                {
                    ModelState.AddModelError("DiscountValue", "Rabat kwotowy nie może być większy lub równy cenie gry");
                    ViewBag.Games = await _context.Games.Where(g => g.IsActive).ToListAsync();
                    return View(promotion);
                }

                promotion.CreatedAt = DateTime.Now;
                _context.Promotions.Add(promotion);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Promocja została utworzona";
                return RedirectToAction(nameof(Promotions));
            }

            ViewBag.Games = await _context.Games.Where(g => g.IsActive).ToListAsync();
            return View(promotion);
        }

        public async Task<IActionResult> EditPromotion(int id)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null)
            {
                return NotFound();
            }

            ViewBag.Games = await _context.Games.Where(g => g.IsActive).ToListAsync();
            return View(promotion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPromotion(int id, Promotion promotion)
        {
            if (id != promotion.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (promotion.EndDate <= promotion.StartDate)
                {
                    ModelState.AddModelError("EndDate", "Data zakończenia musi być późniejsza niż data rozpoczęcia");
                    ViewBag.Games = await _context.Games.Where(g => g.IsActive).ToListAsync();
                    return View(promotion);
                }

                try
                {
                    _context.Update(promotion);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Promocja została zaktualizowana";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PromotionExists(promotion.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Promotions));
            }

            ViewBag.Games = await _context.Games.Where(g => g.IsActive).ToListAsync();
            return View(promotion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePromotion(int id)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null)
            {
                return NotFound();
            }

            promotion.IsActive = !promotion.IsActive;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Promocja została {(promotion.IsActive ? "aktywowana" : "dezaktywowana")}";
            return RedirectToAction(nameof(Promotions));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePromotion(int id)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null)
            {
                return NotFound();
            }

            _context.Promotions.Remove(promotion);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Promocja została usunięta";
            return RedirectToAction(nameof(Promotions));
        }

        // === STATYSTYKI ===

        public async Task<IActionResult> Statistics()
        {
            var stats = new StatisticsViewModel
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalGames = await _context.Games.CountAsync(),
                TotalPurchases = await _context.Purchases.CountAsync(),
                TotalRevenue = await _context.Purchases.SumAsync(p => p.PricePaid),
                
                // Top 5 najlepiej sprzedających się gier
                TopGames = await _context.Purchases
                    .GroupBy(p => p.Game)
                    .Select(g => new GameStatistic
                    {
                        Game = g.Key,
                        SalesCount = g.Count(),
                        Revenue = g.Sum(p => p.PricePaid)
                    })
                    .OrderByDescending(x => x.SalesCount)
                    .Take(5)
                    .ToListAsync(),

                // Sprzedaż w ostatnich 30 dniach
                Last30DaysSales = await _context.Purchases
                    .Where(p => p.PurchaseDate >= DateTime.Now.AddDays(-30))
                    .CountAsync(),

                Last30DaysRevenue = await _context.Purchases
                    .Where(p => p.PurchaseDate >= DateTime.Now.AddDays(-30))
                    .SumAsync(p => p.PricePaid)
            };

            return View(stats);
        }

        private bool PromotionExists(int id)
        {
            return _context.Promotions.Any(e => e.Id == id);
        }
    }
}