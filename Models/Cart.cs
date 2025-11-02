namespace mist.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Nawigacja
        public virtual User User { get; set; }
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

        // Obliczenia
        public decimal TotalAmount => CartItems?.Sum(ci => ci.Price) ?? 0;
        public int ItemCount => CartItems?.Count ?? 0;
    }
}