using System.ComponentModel.DataAnnotations;

namespace mist.Models
{
    public class Game
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tytuł gry jest wymagany")]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [StringLength(2000)]
        public string Description { get; set; }

        [Required]
        [Range(0.01, 999.99, ErrorMessage = "Cena musi być między 0.01 a 999.99")]
        public decimal Price { get; set; }

        public string ImageUrl { get; set; }
        public string Developer { get; set; }
        public string Publisher { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string Genre { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }

        // Relacje
        public virtual ICollection<User> Owners { get; set; }
        public virtual ICollection<Purchase> Purchases { get; set; }
        public virtual ICollection<Promotion> Promotions { get; set; }

        // Obliczenia - cena z uwzględnieniem aktywnej promocji
        public decimal GetCurrentPrice()
        {
            var activePromotion = Promotions?
                .FirstOrDefault(p => p.IsValidNow());

            if (activePromotion != null)
            {
                return activePromotion.CalculateDiscountedPrice(Price);
            }

            return Price;
        }

        public Promotion GetActivePromotion()
        {
            return Promotions?.FirstOrDefault(p => p.IsValidNow());
        }

        public bool HasActivePromotion()
        {
            return GetActivePromotion() != null;
        }
    }
}