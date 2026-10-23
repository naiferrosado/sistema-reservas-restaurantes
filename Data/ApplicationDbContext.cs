using Microsoft.EntityFrameworkCore;
using SistemaReservas.Models;

namespace SistemaReservas.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Restaurant> Restaurants => Set<Restaurant>();
        public DbSet<RestaurantTable> Tables => Set<RestaurantTable>();
        public DbSet<MenuItem> MenuItems => Set<MenuItem>();
        public DbSet<Reservation> Reservations => Set<Reservation>();
        public DbSet<NotificationLog> Notifications => Set<NotificationLog>();
        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Restaurant>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
                entity.Property(e => e.CuisineType).HasMaxLength(100);
            });

            modelBuilder.Entity<RestaurantTable>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Restaurant)
                      .WithMany(r => r.Tables)
                      .HasForeignKey(e => e.RestaurantId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<MenuItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Price).HasPrecision(10, 2);
                entity.HasOne(e => e.Restaurant)
                      .WithMany(r => r.MenuItems)
                      .HasForeignKey(e => e.RestaurantId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Reservation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ReservationCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CustomerName).IsRequired().HasMaxLength(150);
                entity.Property(e => e.CustomerEmail).IsRequired().HasMaxLength(150);
                
                entity.HasOne(e => e.Restaurant)
                      .WithMany(r => r.Reservations)
                      .HasForeignKey(e => e.RestaurantId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Table)
                      .WithMany(t => t.Reservations)
                      .HasForeignKey(e => e.TableId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<NotificationLog>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(150);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
            });
        }
    }
}
