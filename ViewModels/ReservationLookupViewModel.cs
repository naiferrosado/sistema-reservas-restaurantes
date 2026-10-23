using SistemaReservas.Models;

namespace SistemaReservas.ViewModels
{
    public class ReservationLookupViewModel
    {
        // Campos de búsqueda
        public string? SearchCode { get; set; }
        public string? SearchEmail { get; set; }

        // Resultados
        public List<Reservation> Results { get; set; } = new();

        // Control de estado de la búsqueda
        public bool HasSearched { get; set; } = false;
        public string? ErrorMessage { get; set; }
    }
}
