using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mist.Services;
using mist.ViewModels;
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
        public async Task<IActionResult> Index(LibrarySearchViewModel searchModel)
        {
            var userId = GetUserId();
            var games = await _purchaseService.GetUserOwnedGamesAsync(userId);

            // Konwertuj na IQueryable dla łatwiejszego filtrowania
            var query = games.AsQueryable();

            // Filtrowanie po wyszukiwanej frazie
            if (!string.IsNullOrWhiteSpace(searchModel.SearchTerm))
            {
                var searchTerm = searchModel.SearchTerm.ToLower();
                query = query.Where(g => 
                    g.Title.ToLower().Contains(searchTerm) ||
                    g.Developer.ToLower().Contains(searchTerm) ||
                    g.Genre.ToLower().Contains(searchTerm)
                );
            }

            // Filtrowanie po gatunku
            if (!string.IsNullOrWhiteSpace(searchModel.Genre))
            {
                query = query.Where(g => g.Genre == searchModel.Genre);
            }

            // Sortowanie
            query = searchModel.SortBy switch
            {
                "name" => query.OrderBy(g => g.Title),
                "genre" => query.OrderBy(g => g.Genre).ThenBy(g => g.Title),
                _ => query.OrderByDescending(g => g.CreatedAt) // recent (default)
            };

            var filteredGames = query.ToList();

            // Pobierz dostępne gatunki dla filtrów
            ViewBag.Genres = games
                .Select(g => g.Genre)
                .Distinct()
                .OrderBy(g => g)
                .ToList();

            ViewBag.SearchModel = searchModel;
            ViewBag.TotalGames = games.Count;
            ViewBag.FilteredGames = filteredGames.Count;

            return View(filteredGames);
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