namespace SistemaReservas.Services
{
    public interface IEmailService
    {
        Task SendReservationReceivedAsync(Models.Reservation reservation);
        Task SendReservationConfirmationAsync(Models.Reservation reservation);
        Task SendReservationRejectionAsync(Models.Reservation reservation, string? reason);
        Task SendPasswordResetAsync(string email, string fullName, string resetLink, int expirationMinutes = 30);
        Task<List<Models.NotificationLog>> GetRecentNotificationsAsync(int? restaurantId = null);
    }
}
