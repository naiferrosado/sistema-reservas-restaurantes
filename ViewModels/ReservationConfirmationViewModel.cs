using SistemaReservas.Models;

namespace SistemaReservas.ViewModels
{
    public class ReservationConfirmationViewModel
    {
        public Reservation Reservation { get; set; } = null!;
        public Restaurant? Restaurant { get; set; }
        public RestaurantTable? Table { get; set; }
    }
}
