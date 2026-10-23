using System.ComponentModel.DataAnnotations;

namespace SistemaReservas.DTOs
{
    public class CreateReservationDto
    {
        [Required]
        public int RestaurantId { get; set; }

        public int? TableId { get; set; }

        [Required(ErrorMessage = "El nombre del cliente es obligatorio")]
        [StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de correo no válido")]
        public string CustomerEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        public string CustomerPhone { get; set; } = string.Empty;

        [Required]
        public DateTime ReservationDate { get; set; }

        [Required(ErrorMessage = "La hora es obligatoria")]
        public string ReservationTime { get; set; } = string.Empty;

        [Range(1, 20, ErrorMessage = "El número de comensales debe ser entre 1 y 20")]
        public int PartySize { get; set; }

        public string SpecialRequests { get; set; } = string.Empty;
    }
}
