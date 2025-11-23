using Microsoft.EntityFrameworkCore;
using mist.Data;
using mist.Models;

namespace mist.Services
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;

        public CartService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Cart> GetOrCreateCartAsync(int userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Game)
                .ThenInclude(g => g.Promotions)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        public async Task<(bool Success, string Message)> AddToCartAsync(int userId, int gameId)
        {
            // Sprawdź czy gra istnieje
            var game = await _context.Games
                .Include(g => g.Promotions)
                .FirstOrDefaultAsync(g => g.Id == gameId);
                
            if (game == null)
            {
                return (false, "Gra nie została znaleziona");
            }

            if (!game.IsActive)
            {
                return (false, "Ta gra nie jest już dostępna");
            }

            // Sprawdź czy użytkownik już kupił tę grę
            if (await HasUserPurchasedGameAsync(userId, gameId))
            {
                return (false, "Już posiadasz tę grę");
            }

            var cart = await GetOrCreateCartAsync(userId);

            // Sprawdź czy gra już jest w koszyku
            if (await IsGameInCartAsync(userId, gameId))
            {
                return (false, "Gra jest już w koszyku");
            }

            // Użyj aktualnej ceny (z uwzględnieniem promocji)
            var currentPrice = game.GetCurrentPrice();

            var cartItem = new CartItem
            {
                CartId = cart.Id,
                GameId = gameId,
                Price = currentPrice
            };

            _context.CartItems.Add(cartItem);
            cart.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return (true, "Dodano do koszyka");
        }

        public async Task<(bool Success, string Message)> RemoveFromCartAsync(int userId, int gameId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.GameId == gameId);

            if (cartItem == null)
            {
                return (false, "Nie znaleziono produktu w koszyku");
            }

            _context.CartItems.Remove(cartItem);
            cart.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return (true, "Usunięto z koszyka");
        }

        public async Task<(bool Success, string Message)> ClearCartAsync(int userId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            
            if (!cart.CartItems.Any())
            {
                return (false, "Koszyk jest już pusty");
            }

            _context.CartItems.RemoveRange(cart.CartItems);
            await _context.SaveChangesAsync();

            return (true, "Koszyk został wyczyszczony");
        }

        public async Task<int> GetCartItemCountAsync(int userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            return cart?.ItemCount ?? 0;
        }

        public async Task<bool> IsGameInCartAsync(int userId, int gameId)
        {
            return await _context.CartItems
                .AnyAsync(ci => ci.Cart.UserId == userId && ci.GameId == gameId);
        }

        public async Task<bool> HasUserPurchasedGameAsync(int userId, int gameId)
        {
            return await _context.Purchases
                .AnyAsync(p => p.UserId == userId && p.GameId == gameId);
        }
    }
}