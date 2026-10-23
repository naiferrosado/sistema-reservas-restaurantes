using System.Net;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using SistemaReservas.Data;
using SistemaReservas.Models;

namespace SistemaReservas.Services
{
    public class EmailService : IEmailService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(ApplicationDbContext context, IConfiguration configuration, ILogger<EmailService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendReservationConfirmationAsync(Reservation reservation)
        {
            var restaurantName = reservation.Restaurant?.Name ?? "Restaurante";
            var tableText = reservation.Table != null 
                ? $"Mesa {reservation.Table.TableNumber} ({reservation.Table.Location})" 
                : "Mesa asignada al llegar";

            var subject = $"✨ ¡Reserva Confirmada en {restaurantName}! - Código: {reservation.ReservationCode}";
            
            var bodyHtml = $@"
<!DOCTYPE html>
<html>
<head>
  <meta charset='utf-8'/>
  <style>
    body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background-color: #f1f5f9; margin: 0; padding: 24px; color: #1e293b; }}
    .card {{ max-width: 600px; margin: 0 auto; background: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 10px 25px -5px rgba(0, 0, 0, 0.05); border: 1px solid #e2e8f0; }}
    .header {{ background: linear-gradient(135deg, #4f46e5 0%, #7c3aed 100%); padding: 36px 30px; text-align: center; color: #ffffff; }}
    .header h1 {{ margin: 0 0 8px 0; font-size: 26px; font-weight: 700; }}
    .header p {{ margin: 0; opacity: 0.9; font-size: 15px; }}
    .badge {{ display: inline-block; background: rgba(255,255,255,0.2); padding: 6px 14px; border-radius: 9999px; font-weight: 600; font-size: 14px; margin-top: 12px; }}
    .content {{ padding: 32px 30px; }}
    .greeting {{ font-size: 18px; margin-top: 0; font-weight: 600; color: #0f172a; }}
    .details-box {{ background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 12px; padding: 20px; margin: 24px 0; }}
    .detail-row {{ display: flex; justify-content: space-between; padding: 10px 0; border-bottom: 1px solid #edf2f7; }}
    .detail-row:last-child {{ border-bottom: none; }}
    .label {{ color: #64748b; font-size: 14px; font-weight: 500; }}
    .value {{ color: #0f172a; font-size: 15px; font-weight: 600; text-align: right; }}
    .code-box {{ text-align: center; background: #eff6ff; border: 2px dashed #3b82f6; border-radius: 12px; padding: 18px; margin: 20px 0; }}
    .code-title {{ font-size: 12px; font-weight: 700; color: #3b82f6; text-transform: uppercase; letter-spacing: 0.05em; }}
    .code-number {{ font-size: 26px; font-weight: 800; color: #1e3a8a; letter-spacing: 0.1em; margin-top: 4px; }}
    .note {{ background: #fffbeb; border-left: 4px solid #f59e0b; padding: 12px 16px; border-radius: 4px; font-size: 14px; color: #92400e; margin: 20px 0; }}
    .footer {{ background: #f8fafc; border-top: 1px solid #e2e8f0; padding: 20px 30px; text-align: center; font-size: 13px; color: #94a3b8; }}
  </style>
</head>
<body>
  <div class='card'>
    <div class='header'>
      <h1>{restaurantName}</h1>
      <p>Confirmación Oficial de Reserva</p>
      <div class='badge'>✓ Estado: Aprobada</div>
    </div>
    <div class='content'>
      <p class='greeting'>¡Hola, {reservation.CustomerName}!</p>
      <p>Nos complace informarte que tu solicitud de reserva ha sido <strong>confirmada y aprobada</strong> por el restaurante. Tu mesa estará lista y reservada para tu llegada.</p>
      
      <div class='code-box'>
        <div class='code-title'>Código de Reserva</div>
        <div class='code-number'>{reservation.ReservationCode}</div>
      </div>

      <div class='details-box'>
        <div class='detail-row'>
          <span class='label'>📅 Fecha:</span>
          <span class='value'>{reservation.ReservationDate:dddd, dd 'de' MMMM 'de' yyyy}</span>
        </div>
        <div class='detail-row'>
          <span class='label'>⏰ Hora:</span>
          <span class='value'>{reservation.ReservationTime} hrs</span>
        </div>
        <div class='detail-row'>
          <span class='label'>👥 Comensales:</span>
          <span class='value'>{reservation.PartySize} personas</span>
        </div>
        <div class='detail-row'>
          <span class='label'>🍽️ Ubicación / Mesa:</span>
          <span class='value'>{tableText}</span>
        </div>
        {(string.IsNullOrWhiteSpace(reservation.SpecialRequests) ? "" : $@"
        <div class='detail-row'>
          <span class='label'>💬 Peticiones Especiales:</span>
          <span class='value'>{reservation.SpecialRequests}</span>
        </div>")}
      </div>

      <div class='note'>
        <strong>Aviso:</strong> Agradecemos presentarte con 10 minutos de antelación. Si necesitas modificar o cancelar la reserva, comunícate al restaurante con tu código de reserva.
      </div>
    </div>
    <div class='footer'>
      <p>Sistema Automatizado de Reservas &bull; {restaurantName}</p>
      <p>{reservation.Restaurant?.Address ?? "Restaurante"} | Tel: {reservation.Restaurant?.Phone ?? "N/A"}</p>
    </div>
  </div>
</body>
</html>";

            await RecordAndOptionallySendAsync(reservation, subject, bodyHtml);
        }

        public async Task SendReservationRejectionAsync(Reservation reservation, string? reason)
        {
            var restaurantName = reservation.Restaurant?.Name ?? "Restaurante";
            var subject = $"Aviso sobre tu solicitud de reserva en {restaurantName}";

            var bodyHtml = $@"
<!DOCTYPE html>
<html>
<head>
  <meta charset='utf-8'/>
  <style>
    body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; background-color: #f1f5f9; padding: 24px; color: #1e293b; }}
    .card {{ max-width: 600px; margin: 0 auto; background: #ffffff; border-radius: 16px; overflow: hidden; border: 1px solid #e2e8f0; }}
    .header {{ background: #dc2626; padding: 28px 24px; text-align: center; color: #ffffff; }}
    .content {{ padding: 28px 24px; }}
    .reason-box {{ background: #fef2f2; border: 1px solid #fecaca; padding: 16px; border-radius: 8px; color: #991b1b; margin: 16px 0; }}
  </style>
</head>
<body>
  <div class='card'>
    <div class='header'>
      <h2 style='margin:0;'>{restaurantName}</h2>
      <p style='margin:4px 0 0 0;'>Actualización de Reserva: {reservation.ReservationCode}</p>
    </div>
    <div class='content'>
      <p>Estimado/a <strong>{reservation.CustomerName}</strong>,</p>
      <p>Lamentamos informarte que en esta ocasión no ha sido posible confirmar tu solicitud de reserva para el <strong>{reservation.ReservationDate:dd/MM/yyyy}</strong> a las <strong>{reservation.ReservationTime}</strong>.</p>
      <div class='reason-box'>
        <strong>Motivo:</strong> {reason ?? "Aforo completo para el horario solicitado."}
      </div>
      <p>Te invitamos cordialmente a elegir otro horario o fecha disponible a través de nuestra web.</p>
    </div>
  </div>
</body>
</html>";

            await RecordAndOptionallySendAsync(reservation, subject, bodyHtml);
        }

        private async Task RecordAndOptionallySendAsync(Reservation reservation, string subject, string bodyHtml)
        {
            var status = "Enviado (Simulación)";

            // Intento de envío SMTP si está configurado en appsettings.json
            var smtpHost = _configuration["Smtp:Host"];
            var smtpPort = _configuration.GetValue<int?>("Smtp:Port");
            var smtpUser = _configuration["Smtp:User"];
            var smtpPass = _configuration["Smtp:Password"];
            var smtpFrom = _configuration["Smtp:From"] ?? "notificaciones@sistemareservas.local";

            if (!string.IsNullOrEmpty(smtpHost) && smtpPort.HasValue && !string.IsNullOrEmpty(smtpUser))
            {
                try
                {
                    using var client = new SmtpClient(smtpHost, smtpPort.Value)
                    {
                        Credentials = new NetworkCredential(smtpUser, smtpPass),
                        EnableSsl = _configuration.GetValue("Smtp:EnableSsl", true)
                    };

                    using var mail = new MailMessage
                    {
                        From = new MailAddress(smtpFrom, reservation.Restaurant?.Name ?? "Sistema Reservas"),
                        Subject = subject,
                        Body = bodyHtml,
                        IsBodyHtml = true
                    };
                    mail.To.Add(reservation.CustomerEmail);

                    await client.SendMailAsync(mail);
                    status = "Enviado vía SMTP";
                    _logger.LogInformation("Correo enviado vía SMTP a {Email}", reservation.CustomerEmail);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error al enviar correo SMTP real. Registrando en buzón de notificaciones.");
                    status = "Enviado (Simulación en buzón)";
                }
            }

            var notification = new NotificationLog
            {
                ReservationId = reservation.Id,
                RecipientEmail = reservation.CustomerEmail,
                RecipientName = reservation.CustomerName,
                Subject = subject,
                BodyHtml = bodyHtml,
                Status = status,
                SentAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<List<NotificationLog>> GetRecentNotificationsAsync(int? restaurantId = null)
        {
            var query = _context.Notifications.AsQueryable();
            return await query.OrderByDescending(n => n.SentAt).Take(50).ToListAsync();
        }
    }
}
