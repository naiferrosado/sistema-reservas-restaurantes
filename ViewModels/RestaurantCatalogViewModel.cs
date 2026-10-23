using SistemaReservas.Models;

namespace SistemaReservas.ViewModels
{
    public class RestaurantCatalogViewModel
    {
        public List<Restaurant> Restaurants { get; set; } = new();
        public Restaurant? SelectedRestaurant { get; set; }
        public int SelectedRestaurantId { get; set; } = 1;

        // Carta del restaurante activo agrupada por categoría
        public Dictionary<string, List<MenuItem>> MenuByCategory { get; set; } = new();

        // Mesas del restaurante activo agrupadas por zona/ubicación
        public Dictionary<string, List<RestaurantTable>> TablesByZone { get; set; } = new();

        // Pestaña activa: "menu" o "tables"
        public string ActiveTab { get; set; } = "menu";

        // Filtro de categoría activo
        public string? ActiveCategory { get; set; }

        // Todas las categorías disponibles del menú
        public List<string> Categories { get; set; } = new();
    }
}
