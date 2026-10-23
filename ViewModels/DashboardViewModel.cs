using SistemaReservas.Models;

namespace SistemaReservas.ViewModels
{
    public class DashboardViewModel
    {
        // Restaurante activo
        public int SelectedRestaurantId { get; set; } = 1;
        public List<Restaurant> Restaurants { get; set; } = new();
        public Restaurant? SelectedRestaurant { get; set; }

        // Pestaña activa: "reservations" | "floorplan" | "inbox" | "menu"
        public string ActiveTab { get; set; } = "reservations";

        // KPIs en tiempo real
        public int TotalReservationsToday { get; set; }
        public int ExpectedGuestsToday { get; set; }
        public double OccupancyRate { get; set; }
        public int PendingCount { get; set; }
        public int ConfirmedCount { get; set; }
        public int RejectedCount { get; set; }
        public int TotalTables { get; set; }
        public int OccupiedTables { get; set; }

        // Gestión de Reservas
        public List<Reservation> Reservations { get; set; } = new();
        public string? FilterStatus { get; set; }
        public string? FilterSearch { get; set; }
        public DateTime? FilterDate { get; set; }

        // Plano de Mesas (agrupado por zona)
        public Dictionary<string, List<RestaurantTable>> TablesByZone { get; set; } = new();

        // Bandeja de Correos
        public List<NotificationLog> Notifications { get; set; } = new();

        // Gestión de Menú
        public List<MenuItem> MenuItems { get; set; } = new();
        public Dictionary<string, List<MenuItem>> MenuByCategory { get; set; } = new();
    }
}
