using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mist.Data;
using mist.Models;
using mist.Services;
using Microsoft.AspNetCore.Authorization;

namespace mist.Controllers
{
    public class GamesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileUploadService _fileUploadService;

        public GamesController(ApplicationDbContext context, IFileUploadService fileUploadService)
        {
            _context = context;
            _fileUploadService = fileUploadService;
        }

        // GET: Games
        public async Task<IActionResult> Index()
        {
            var games = await _context.Games
                .Include(g => g.Promotions)
                .Where(g => g.IsActive)
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync();
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
        public async Task<IActionResult> Create(Game game, IFormFile imageFile)
        {
            // Usuń walidację dla CreatedAt jeśli jest
            ModelState.Remove("CreatedAt");
            ModelState.Remove("Owners");
            ModelState.Remove("Purchases");
            ModelState.Remove("Promotions");
            ModelState.Remove("ImageUrl");
            
            if (ModelState.IsValid)
            {
                // Obsługa przesyłania pliku
                if (imageFile != null && imageFile.Length > 0)
                {
                    var uploadResult = await _fileUploadService.UploadGameImageAsync(imageFile);
                    
                    if (!uploadResult.Success)
                    {
                        ModelState.AddModelError("ImageFile", uploadResult.Message);
                        return View(game);
                    }
                    
                    game.ImageUrl = uploadResult.FilePath;
                }
                else
                {
                    // Jeśli nie przesłano pliku, ustaw domyślny obrazek
                    game.ImageUrl = "/images/default-game.jpg";
                }

                game.CreatedAt = DateTime.UtcNow;
                _context.Add(game);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Gra została dodana pomyślnie!";
                return RedirectToAction(nameof(Index));
            }
            
            // Wyświetl błędy walidacji
            foreach (var modelState in ModelState.Values)
            {
                foreach (var error in modelState.Errors)
                {
                    System.Diagnostics.Debug.WriteLine($"Validation Error: {error.ErrorMessage}");
                }
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
        public async Task<IActionResult> Edit(int id, Game game, IFormFile imageFile, bool removeImage)
        {
            if (id != game.Id)
            {
                return NotFound();
            }

            // Usuń walidację dla pól nawigacyjnych
            ModelState.Remove("Owners");
            ModelState.Remove("Purchases");
            ModelState.Remove("Promotions");
            ModelState.Remove("ImageUrl");

            if (ModelState.IsValid)
            {
                try
                {
                    var existingGame = await _context.Games.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);
                    var oldImageUrl = existingGame?.ImageUrl;

                    // Obsługa usuwania obrazka
                    if (removeImage && !string.IsNullOrEmpty(oldImageUrl))
                    {
                        await _fileUploadService.DeleteGameImageAsync(oldImageUrl);
                        game.ImageUrl = "/images/default-game.jpg";
                    }
                    // Obsługa przesyłania nowego pliku
                    else if (imageFile != null && imageFile.Length > 0)
                    {
                        var uploadResult = await _fileUploadService.UploadGameImageAsync(imageFile);
                        
                        if (!uploadResult.Success)
                        {
                            ModelState.AddModelError("ImageFile", uploadResult.Message);
                            return View(game);
                        }
                        
                        // Usuń stary obrazek jeśli istnieje
                        if (!string.IsNullOrEmpty(oldImageUrl) && oldImageUrl != "/images/default-game.jpg")
                        {
                            await _fileUploadService.DeleteGameImageAsync(oldImageUrl);
                        }
                        
                        game.ImageUrl = uploadResult.FilePath;
                    }
                    else
                    {
                        // Zachowaj stary obrazek
                        game.ImageUrl = oldImageUrl ?? "/images/default-game.jpg";
                    }

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
                // Usuń obrazek jeśli nie jest domyślny
                if (!string.IsNullOrEmpty(game.ImageUrl) && game.ImageUrl != "/images/default-game.jpg")
                {
                    await _fileUploadService.DeleteGameImageAsync(game.ImageUrl);
                }

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