using System.Text.Json.Serialization;

namespace SistemaReservas.Models
{
    public class Restaurant
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string CuisineType { get; set; } = string.Empty;
        public string OpeningTime { get; set; } = "12:00";
        public string ClosingTime { get; set; } = "23:00";
        public string ImageUrl { get; set; } = string.Empty;
        public double Rating { get; set; } = 4.8;
        public string PriceRange { get; set; } = "$$$";
        public int TotalCapacity { get; set; } = 50;

        [JsonIgnore]
        public ICollection<RestaurantTable> Tables { get; set; } = new List<RestaurantTable>();

        [JsonIgnore]
        public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();

        [JsonIgnore]
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
