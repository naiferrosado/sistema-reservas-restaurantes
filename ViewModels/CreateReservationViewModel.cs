using System.ComponentModel.DataAnnotations;
using SistemaReservas.Models;

namespace SistemaReservas.ViewModels
{
    public class CreateReservationViewModel
    {
        [Required]
        public int RestaurantId { get; set; }

        public int? TableId { get; set; }

        [Required(ErrorMessage = "El nombre completo es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        [Display(Name = "Nombre completo")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido")]
        [Display(Name = "Correo electrónico")]
        public string CustomerEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono de contacto es obligatorio")]
        [Phone(ErrorMessage = "Ingrese un número de teléfono válido")]
        [Display(Name = "Teléfono de contacto")]
        public string CustomerPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de reserva es obligatoria")]
        [Display(Name = "Fecha de reserva")]
        public DateTime ReservationDate { get; set; } = DateTime.Today.AddDays(1);

        [Required(ErrorMessage = "La hora es obligatoria")]
        [Display(Name = "Hora")]
        public string ReservationTime { get; set; } = "20:00";

        [Range(1, 20, ErrorMessage = "El número de comensales debe ser entre 1 y 20")]
        [Display(Name = "Número de comensales")]
        public int PartySize { get; set; } = 2;

        [StringLength(500, ErrorMessage = "Las peticiones especiales no pueden exceder 500 caracteres")]
        [Display(Name = "Peticiones especiales")]
        public string SpecialRequests { get; set; } = string.Empty;

        // Datos para el formulario — no se validan en POST
        public Restaurant? Restaurant { get; set; }
        public List<Restaurant> Restaurants { get; set; } = new();
        public List<RestaurantTable> AvailableTables { get; set; } = new();

        // Franjas horarias disponibles
        public List<string> TimeSlots { get; set; } = new()
        {
            "12:00", "12:30", "13:00", "13:30", "14:00", "14:30",
            "19:00", "19:30", "20:00", "20:30", "21:00", "21:30", "22:00"
        };
    }
}
