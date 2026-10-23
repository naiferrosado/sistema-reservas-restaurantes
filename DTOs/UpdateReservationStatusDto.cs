using System.ComponentModel.DataAnnotations;

namespace SistemaReservas.DTOs
{
    public class UpdateReservationStatusDto
    {
        [Required]
        public string Status { get; set; } = string.Empty; // Confirmada, Rechazada, Cancelada, Completada

        public int? TableId { get; set; }

        public string? AdminNotes { get; set; }
    }
}
