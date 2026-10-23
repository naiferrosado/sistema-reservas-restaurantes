using System.Text.Json.Serialization;

namespace SistemaReservas.Models
{
    public class MenuItem
    {
        public int Id { get; set; }
        public int RestaurantId { get; set; }
        
        [JsonIgnore]
        public Restaurant? Restaurant { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Category { get; set; } = "Platos Fuertes"; // Entrantes, Platos Fuertes, Postres, Bebidas
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsAvailable { get; set; } = true;
        public string Tags { get; set; } = string.Empty; // e.g. "Vegetariano, Sin Gluten"
    }
}
