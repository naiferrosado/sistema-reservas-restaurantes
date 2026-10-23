using System.ComponentModel.DataAnnotations;

namespace SistemaReservas.DTOs
{
    public class CreateMenuItemDto
    {
        [Required]
        public int RestaurantId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Range(0.01, 10000)]
        public decimal Price { get; set; }

        public string Category { get; set; } = "Platos Fuertes";
        public string ImageUrl { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty;
        public bool IsAvailable { get; set; } = true;
    }
}
