namespace mist.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public int CartId { get; set; }
        public int GameId { get; set; }
        public decimal Price { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.Now;

        // Nawigacja
        public virtual Cart Cart { get; set; }
        public virtual Game Game { get; set; }
    }
}