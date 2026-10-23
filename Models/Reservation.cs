using System.Text.Json.Serialization;

namespace SistemaReservas.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public string ReservationCode { get; set; } = string.Empty; // e.g. RES-2026-X102

        public int RestaurantId { get; set; }
        public Restaurant? Restaurant { get; set; }

        public int? TableId { get; set; }
        public RestaurantTable? Table { get; set; }

        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;

        public DateTime ReservationDate { get; set; }
        public string ReservationTime { get; set; } = string.Empty; // e.g. "19:30"
        public int PartySize { get; set; }

        public string SpecialRequests { get; set; } = string.Empty;
        public string Status { get; set; } = "Pendiente"; // Pendiente, Confirmada, Rechazada, Cancelada, Completada
        public string? AdminNotes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
