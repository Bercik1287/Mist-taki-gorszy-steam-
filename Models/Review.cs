using System.ComponentModel.DataAnnotations;

namespace mist.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int GameId { get; set; }

        [Required(ErrorMessage = "Ocena jest wymagana")]
        [Range(1, 5, ErrorMessage = "Ocena musi być między 1 a 5")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Tytuł recenzji jest wymagany")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Tytuł musi mieć 3-100 znaków")]
        public string Title { get; set; }

        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Recenzja musi mieć 10-1000 znaków")]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public bool IsVerifiedPurchase { get; set; } // Czy użytkownik posiada grę

        // Nawigacja
        public virtual User User { get; set; }
        public virtual Game Game { get; set; }
    }
}