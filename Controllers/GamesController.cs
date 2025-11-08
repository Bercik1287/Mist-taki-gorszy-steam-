using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mist.Data;
using mist.Models;
using mist.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace mist.Controllers
{
    public class GamesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GamesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Games
        public async Task<IActionResult> Index(GameSearchViewModel searchModel)
        {
            var query = _context.Games
                .Include(g => g.Promotions)
                .Where(g => g.IsActive)
                .AsQueryable();

            // Filtrowanie po wyszukiwanej frazie
            if (!string.IsNullOrWhiteSpace(searchModel.SearchTerm))
            {
                var searchTerm = searchModel.SearchTerm.ToLower();
                query = query.Where(g => 
                    g.Title.ToLower().Contains(searchTerm) ||
                    g.Description.ToLower().Contains(searchTerm) ||
                    g.Developer.ToLower().Contains(searchTerm) ||
                    g.Publisher.ToLower().Contains(searchTerm)
                );
            }

            // Filtrowanie po gatunku
            if (!string.IsNullOrWhiteSpace(searchModel.Genre))
            {
                query = query.Where(g => g.Genre == searchModel.Genre);
            }

            // Filtrowanie po deweloperze
            if (!string.IsNullOrWhiteSpace(searchModel.Developer))
            {
                query = query.Where(g => g.Developer == searchModel.Developer);
            }

            // Pobierz gry do listy, aby móc użyć metody GetCurrentPrice()
            var games = await query.ToListAsync();

            // Filtrowanie po cenie minimalnej (z uwzględnieniem promocji)
            if (searchModel.MinPrice.HasValue)
            {
                games = games.Where(g => g.GetCurrentPrice() >= searchModel.MinPrice.Value).ToList();
            }

            // Filtrowanie po cenie maksymalnej (z uwzględnieniem promocji)
            if (searchModel.MaxPrice.HasValue)
            {
                games = games.Where(g => g.GetCurrentPrice() <= searchModel.MaxPrice.Value).ToList();
            }

            // Sortowanie po cenie (z uwzględnieniem promocji)
            games = searchModel.SortBy switch
            {
                "price-asc" => games.OrderBy(g => g.GetCurrentPrice()).ToList(),
                "price-desc" => games.OrderByDescending(g => g.GetCurrentPrice()).ToList(),
                "name" => games.OrderBy(g => g.Title).ToList(),
                _ => games // już posortowane przez query (newest)
            };

            // Filtrowanie - tylko z promocjami (przed konwersją do listy)
            if (searchModel.OnlyWithPromotions)
            {
                var now = DateTime.Now;
                query = query.Where(g => g.Promotions.Any(p => 
                    p.IsActive && p.StartDate <= now && p.EndDate >= now
                ));
            }

            // Sortowanie (przed filtrami cenowymi)
            query = searchModel.SortBy switch
            {
                "name" => query.OrderBy(g => g.Title),
                _ => query.OrderByDescending(g => g.CreatedAt) // newest (default)
            };


            // Pobierz dostępne gatunki i deweloperów dla filtrów
            ViewBag.Genres = await _context.Games
                .Where(g => g.IsActive)
                .Select(g => g.Genre)
                .Distinct()
                .OrderBy(g => g)
                .ToListAsync();

            ViewBag.Developers = await _context.Games
                .Where(g => g.IsActive)
                .Select(g => g.Developer)
                .Distinct()
                .OrderBy(d => d)
                .ToListAsync();

            ViewBag.SearchModel = searchModel;

            return View(games);
        }

        // GET: Games/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var game = await _context.Games
                .Include(g => g.Promotions)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (game == null)
            {
                return NotFound();
            }

            return View(game);
        }

        // GET: Games/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Games/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Game game)
        {
            ModelState.Remove("CreatedAt");
            ModelState.Remove("Owners");
            ModelState.Remove("Purchases");
            ModelState.Remove("Promotions");
            
            if (ModelState.IsValid)
            {
                game.CreatedAt = DateTime.UtcNow;
                _context.Add(game);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Gra została dodana pomyślnie!";
                return RedirectToAction(nameof(Index));
            }
            
            return View(game);
        }

        // GET: Games/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var game = await _context.Games.FindAsync(id);
            if (game == null)
            {
                return NotFound();
            }
            return View(game);
        }

        // POST: Games/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Game game)
        {
            if (id != game.Id)
            {
                return NotFound();
            }

            ModelState.Remove("Owners");
            ModelState.Remove("Purchases");
            ModelState.Remove("Promotions");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(game);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Gra została zaktualizowana!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GameExists(game.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(game);
        }

        // GET: Games/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var game = await _context.Games
                .FirstOrDefaultAsync(m => m.Id == id);

            if (game == null)
            {
                return NotFound();
            }

            return View(game);
        }

        // POST: Games/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var game = await _context.Games.FindAsync(id);
            if (game != null)
            {
                _context.Games.Remove(game);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Gra została usunięta!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool GameExists(int id)
        {
            return _context.Games.Any(e => e.Id == id);
        }
    }
}