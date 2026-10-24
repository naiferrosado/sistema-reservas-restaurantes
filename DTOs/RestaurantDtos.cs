using System.ComponentModel.DataAnnotations;

namespace SistemaReservas.DTOs
{
    public class CreateRestaurantDto
    {
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [MaxLength(100)]
        public string CuisineType { get; set; } = "Alta Cocina Dominicana";

        public string OpeningTime { get; set; } = "12:00";

        public string ClosingTime { get; set; } = "23:00";

        public string ImageUrl { get; set; } = string.Empty;

        [Range(1.0, 5.0)]
        public double Rating { get; set; } = 4.8;

        public string PriceRange { get; set; } = "$$$";

        [Range(1, 1000)]
        public int TotalCapacity { get; set; } = 40;
    }
}
