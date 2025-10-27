namespace mist.Models
{
    public class Purchase
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int GameId { get; set; }
        public decimal PricePaid { get; set; }
        public DateTime PurchaseDate { get; set; } = DateTime.Now;

        // Nawigacja
        public virtual User User { get; set; }
        public virtual Game Game { get; set; }
    }
}