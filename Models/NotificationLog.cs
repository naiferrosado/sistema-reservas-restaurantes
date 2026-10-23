namespace SistemaReservas.Models
{
    public class NotificationLog
    {
        public int Id { get; set; }
        public int? ReservationId { get; set; }
        public string RecipientEmail { get; set; } = string.Empty;
        public string RecipientName { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string BodyHtml { get; set; } = string.Empty;
        public string Status { get; set; } = "Enviado"; // Enviado, Simulado, Fallido
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}
