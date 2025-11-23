using System.ComponentModel.DataAnnotations;

namespace mist.Models
{
    public class Promotion
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nazwa promocji jest wymagana")]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public int GameId { get; set; }

        [Required(ErrorMessage = "Typ rabatu jest wymagany")]
        public DiscountType DiscountType { get; set; }

        [Required(ErrorMessage = "Wartość rabatu jest wymagana")]
        [Range(0.01, 100, ErrorMessage = "Wartość rabatu musi być między 0.01 a 100")]
        public decimal DiscountValue { get; set; }

        [Required(ErrorMessage = "Data rozpoczęcia jest wymagana")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Data zakończenia jest wymagana")]
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Nawigacja
        public virtual Game Game { get; set; }

        // Obliczenia
        public decimal CalculateDiscountedPrice(decimal originalPrice)
        {
            if (DiscountType == DiscountType.Percentage)
            {
                return originalPrice - (originalPrice * (DiscountValue / 100));
            }
            else
            {
                return Math.Max(0, originalPrice - DiscountValue);
            }
        }

        public bool IsValidNow()
        {
            var now = DateTime.Now;
            return IsActive && now >= StartDate && now <= EndDate;
        }
    }

    public enum DiscountType
    {
        Percentage = 0,
        FixedAmount = 1
    }
}