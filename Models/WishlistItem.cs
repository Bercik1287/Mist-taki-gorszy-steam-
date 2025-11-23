namespace mist.Models
{
    public class WishlistItem
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int GameId { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.Now;

        // Nawigacja
        public virtual User User { get; set; }
        public virtual Game Game { get; set; }
    }
}