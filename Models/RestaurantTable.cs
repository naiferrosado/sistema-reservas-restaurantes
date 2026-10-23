using System.Text.Json.Serialization;

namespace SistemaReservas.Models
{
    public class RestaurantTable
    {
        public int Id { get; set; }
        public int RestaurantId { get; set; }
        
        [JsonIgnore]
        public Restaurant? Restaurant { get; set; }

        public int TableNumber { get; set; }
        public int Capacity { get; set; }
        public string Location { get; set; } = "Salón Principal"; // Salón Principal, Terraza, Zona VIP, Jardín
        public string Status { get; set; } = "Disponible"; // Disponible, Reservada, Ocupada, Mantenimiento
        
        public int PosX { get; set; } = 0;
        public int PosY { get; set; } = 0;

        [JsonIgnore]
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
