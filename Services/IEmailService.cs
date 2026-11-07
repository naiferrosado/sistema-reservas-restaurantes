using SistemaReservas.Models;

namespace SistemaReservas.Services
{
    public interface IEmailService
    {
        Task SendReservationReceivedAsync(Reservation reservation);
        Task SendReservationConfirmationAsync(Reservation reservation);
        Task SendReservationRejectionAsync(Reservation reservation, string? reason);
        Task<List<NotificationLog>> GetRecentNotificationsAsync(int? restaurantId = null);
    }
}
