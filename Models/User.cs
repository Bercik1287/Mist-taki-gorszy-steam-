using System.ComponentModel.DataAnnotations;

namespace mist.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nazwa użytkownika jest wymagana")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Nazwa użytkownika musi mieć 3-20 znaków")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Tylko litery, cyfry i podkreślnik")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email jest wymagany")]
        [EmailAddress(ErrorMessage = "Nieprawidłowy format email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Hasło jest wymagane")]
        public string PasswordHash { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string Role { get; set; } = "User";

        // Relacje
        public virtual ICollection<Game> OwnedGames { get; set; }
        public virtual ICollection<Purchase> Purchases { get; set; }
        public virtual ICollection<WishlistItem> WishlistItems { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
    }
}