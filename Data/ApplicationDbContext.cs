using Microsoft.EntityFrameworkCore;
using mist.Models;

namespace mist.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User - unique constraints
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Purchase relationships
            modelBuilder.Entity<Purchase>()
                .HasOne(p => p.User)
                .WithMany(u => u.Purchases)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Purchase>()
                .HasOne(p => p.Game)
                .WithMany(g => g.Purchases)
                .HasForeignKey(p => p.GameId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cart relationships
            modelBuilder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithOne()
                .HasForeignKey<Cart>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Game)
                .WithMany()
                .HasForeignKey(ci => ci.GameId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed data - użyj stałych wartości dat
            modelBuilder.Entity<Game>().HasData(
                new Game
                {
                    Id = 1,
                    Title = "Cyberpunk 2137",
                    Description = "Futurystyczna gra RPG w otwartym świecie Day City",
                    Price = 199.99m,
                    Developer = "VHS Projekt Green",
                    Publisher = "VHS Projekt",
                    Genre = "RPG",
                    ReleaseDate = new DateTime(2020, 12, 10),
                    ImageUrl = "/images/cyberpunk.jpg",
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 10, 31, 23, 6, 0, 300, DateTimeKind.Utc) // Stała wartość
                },
                new Game
                {
                    Id = 2,
                    Title = "Wichur 3 Dziki Zgon",
                    Description = "Epicka przygoda Gerwazego z Rumunii",
                    Price = 129.99m,
                    Developer = "VHS Projekt Green",
                    Publisher = "VHS Projekt",
                    Genre = "RPG",
                    ReleaseDate = new DateTime(2015, 5, 19),
                    ImageUrl = "/images/witcher3.jpg",
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 10, 31, 23, 6, 0, 306, DateTimeKind.Utc) // Stała wartość
                },
                new Game
                {
                    Id = 3,
                    Title = "Holluw Knuht",
                    Description = "Zejdź w głąb królestwa Hallownest",
                    Price = 19.99m,
                    Developer = "Team Mirabelle",
                    Publisher = "Team Mirabelle",
                    Genre = "Metroidvania",
                    ReleaseDate = new DateTime(2017, 2, 24),
                    ImageUrl = "/images/hollowknight.jpg",
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 10, 31, 23, 6, 0, 312, DateTimeKind.Utc) // Stała wartość
                }
           );
        }

        // Dodaj metodę do automatycznego ustawiania CreatedAt przy zapisie
        public override int SaveChanges()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is Game && e.State == EntityState.Added);

            foreach (var entry in entries)
            {
                ((Game)entry.Entity).CreatedAt = DateTime.UtcNow;
            }

            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is Game && e.State == EntityState.Added);

            foreach (var entry in entries)
            {
                ((Game)entry.Entity).CreatedAt = DateTime.UtcNow;
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}