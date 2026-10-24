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

            var subject = $"Reserva Aprobada en {restaurantName} — Codigo: {reservation.ReservationCode}";

            var bodyHtml = $@"<!DOCTYPE html>
<html lang='es'>
<head>
  <meta charset='utf-8'/>
  <meta name='viewport' content='width=device-width, initial-scale=1.0'/>
  <style>
    body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background-color: #F8FAFC; margin: 0; padding: 24px; color: #0F172A; }}
    .wrapper {{ max-width: 600px; margin: 0 auto; }}
    .card {{ background: #FFFFFF; border-radius: 12px; overflow: hidden; border: 1px solid #E2E8F0; box-shadow: 0 1px 3px rgba(15, 23, 42, 0.06); }}
    .header {{ background: #1B365D; padding: 32px 28px; text-align: center; color: #FFFFFF; }}
    .header-restaurant {{ font-size: 13px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.08em; opacity: 0.75; margin-bottom: 8px; }}
    .header-title {{ font-size: 22px; font-weight: 800; margin: 0; letter-spacing: -0.01em; }}
    .header-subtitle {{ font-size: 14px; opacity: 0.85; margin-top: 6px; }}
    .status-badge {{ display: inline-block; background: rgba(255,255,255,0.15); border: 1px solid rgba(255,255,255,0.3); padding: 5px 16px; border-radius: 99px; font-weight: 700; font-size: 13px; margin-top: 14px; }}
    .content {{ padding: 32px 28px; }}
    .greeting {{ font-size: 17px; font-weight: 700; color: #0F172A; margin: 0 0 10px 0; }}
    .intro {{ font-size: 14px; color: #475569; line-height: 1.65; margin-bottom: 24px; }}
    .code-box {{ text-align: center; border: 2px dashed #CBD5E1; border-radius: 10px; padding: 20px; margin-bottom: 24px; background: #F8FAFC; }}
    .code-label {{ font-size: 11px; font-weight: 700; color: #64748B; text-transform: uppercase; letter-spacing: 0.06em; margin-bottom: 8px; }}
    .code-value {{ font-size: 26px; font-weight: 800; color: #1B365D; letter-spacing: 0.08em; font-family: 'Courier New', monospace; }}
    .details-box {{ background: #F8FAFC; border: 1px solid #E2E8F0; border-radius: 8px; overflow: hidden; margin-bottom: 24px; }}
    .detail-row {{ display: flex; justify-content: space-between; padding: 12px 16px; border-bottom: 1px solid #E2E8F0; }}
    .detail-row:last-child {{ border-bottom: none; }}
    .detail-label {{ color: #64748B; font-size: 13px; font-weight: 500; }}
    .detail-value {{ color: #0F172A; font-size: 13.5px; font-weight: 600; text-align: right; max-width: 60%; }}
    .notice {{ background: #FFFBEB; border: 1px solid #FDE68A; border-left: 4px solid #D97706; border-radius: 6px; padding: 14px 16px; font-size: 13.5px; color: #92400E; line-height: 1.5; margin-bottom: 4px; }}
    .footer {{ background: #F8FAFC; border-top: 1px solid #E2E8F0; padding: 20px 28px; text-align: center; font-size: 12.5px; color: #94A3B8; }}
  </style>
</head>
<body>
  <div class='wrapper'>
    <div class='card'>
      <div class='header'>
        <div class='header-restaurant'>{restaurantName}</div>
        <h1 class='header-title'>Reserva Confirmada</h1>
        <p class='header-subtitle'>Tu solicitud ha sido aprobada por el equipo del restaurante</p>
        <div class='status-badge'>Estado: Aprobada</div>
      </div>
      <div class='content'>
        <p class='greeting'>Hola, {reservation.CustomerName}</p>
        <p class='intro'>Nos complace informarte que tu solicitud de reserva ha sido <strong>confirmada y aprobada</strong>. Tu mesa estara lista para tu llegada en la fecha y hora indicadas a continuacion.</p>

        <div class='code-box'>
          <div class='code-label'>Codigo de Reserva</div>
          <div class='code-value'>{reservation.ReservationCode}</div>
        </div>

        <div class='details-box'>
          <div class='detail-row'>
            <span class='detail-label'>Restaurante</span>
            <span class='detail-value'>{restaurantName}</span>
          </div>
          <div class='detail-row'>
            <span class='detail-label'>Fecha</span>
            <span class='detail-value'>{reservation.ReservationDate.ToString("dddd, dd 'de' MMMM 'de' yyyy", new System.Globalization.CultureInfo("es-DO"))}</span>
          </div>
          <div class='detail-row'>
            <span class='detail-label'>Hora</span>
            <span class='detail-value'>{reservation.ReservationTime} hrs</span>
          </div>
          <div class='detail-row'>
            <span class='detail-label'>Comensales</span>
            <span class='detail-value'>{reservation.PartySize} {(reservation.PartySize == 1 ? "persona" : "personas")}</span>
          </div>
          <div class='detail-row'>
            <span class='detail-label'>Mesa asignada</span>
            <span class='detail-value'>{tableText}</span>
          </div>
          <div class='detail-row'>
            <span class='detail-label'>Direccion</span>
            <span class='detail-value'>{reservation.Restaurant?.Address ?? "Ver en nuestra web"}</span>
          </div>
          {(string.IsNullOrWhiteSpace(reservation.SpecialRequests) ? "" : $@"
          <div class='detail-row'>
            <span class='detail-label'>Peticiones especiales</span>
            <span class='detail-value'>{reservation.SpecialRequests}</span>
          </div>")}
        </div>

        <div class='notice'>
          <strong>Informacion importante:</strong> Te solicitamos presentarte con al menos 10 minutos de antelacion. Si necesitas modificar o cancelar tu reserva, contacta al restaurante indicando tu codigo de reserva.
        </div>
      </div>
      <div class='footer'>
        <p>Sistema Automatizado de Reservas &bull; {restaurantName}</p>
        <p>{reservation.Restaurant?.Address ?? ""} | Tel: {reservation.Restaurant?.Phone ?? "N/A"}</p>
      </div>
    </div>
  </div>
</body>
</html>";

            await RecordAndOptionallySendAsync(reservation, subject, bodyHtml);
        }

        public async Task SendReservationRejectionAsync(Reservation reservation, string? reason)
        {
            var restaurantName = reservation.Restaurant?.Name ?? "Restaurante";
            var subject = $"Actualizacion sobre tu solicitud en {restaurantName} — {reservation.ReservationCode}";

            var bodyHtml = $@"<!DOCTYPE html>
<html lang='es'>
<head>
  <meta charset='utf-8'/>
  <meta name='viewport' content='width=device-width, initial-scale=1.0'/>
  <style>
    body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background-color: #F8FAFC; margin: 0; padding: 24px; color: #0F172A; }}
    .wrapper {{ max-width: 600px; margin: 0 auto; }}
    .card {{ background: #FFFFFF; border-radius: 12px; overflow: hidden; border: 1px solid #E2E8F0; }}
    .header {{ background: #1B365D; padding: 28px; text-align: center; color: #FFFFFF; }}
    .header-restaurant {{ font-size: 13px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.08em; opacity: 0.75; margin-bottom: 6px; }}
    .header-title {{ font-size: 20px; font-weight: 800; margin: 0; }}
    .header-code {{ font-size: 13px; opacity: 0.85; margin-top: 6px; font-family: 'Courier New', monospace; letter-spacing: 0.04em; }}
    .content {{ padding: 28px; }}
    .reason-box {{ background: #FEF2F2; border: 1px solid #FECACA; border-left: 4px solid #DC2626; border-radius: 6px; padding: 14px 16px; margin: 16px 0; }}
    .reason-label {{ font-size: 11px; font-weight: 700; color: #DC2626; text-transform: uppercase; letter-spacing: 0.05em; margin-bottom: 6px; }}
    .reason-text {{ font-size: 13.5px; color: #991B1B; line-height: 1.5; }}
    .details-box {{ background: #F8FAFC; border: 1px solid #E2E8F0; border-radius: 8px; overflow: hidden; margin: 16px 0; }}
    .detail-row {{ display: flex; justify-content: space-between; padding: 10px 14px; border-bottom: 1px solid #E2E8F0; }}
    .detail-row:last-child {{ border-bottom: none; }}
    .detail-label {{ color: #64748B; font-size: 13px; }}
    .detail-value {{ color: #0F172A; font-size: 13px; font-weight: 600; text-align: right; }}
    .footer {{ background: #F8FAFC; border-top: 1px solid #E2E8F0; padding: 18px 28px; text-align: center; font-size: 12.5px; color: #94A3B8; }}
  </style>
</head>
<body>
  <div class='wrapper'>
    <div class='card'>
      <div class='header'>
        <div class='header-restaurant'>{restaurantName}</div>
        <h1 class='header-title'>Actualizacion de Reserva</h1>
        <div class='header-code'>{reservation.ReservationCode}</div>
      </div>
      <div class='content'>
        <p style='font-size:14px; color:#0F172A;'>Estimado/a <strong>{reservation.CustomerName}</strong>,</p>
        <p style='font-size:14px; color:#475569; line-height:1.6;'>Lamentamos informarte que, en esta ocasion, no ha sido posible confirmar tu solicitud de reserva para el <strong>{reservation.ReservationDate:dd/MM/yyyy}</strong> a las <strong>{reservation.ReservationTime}</strong> hrs.</p>

        <div class='reason-box'>
          <div class='reason-label'>Motivo</div>
          <div class='reason-text'>{reason ?? "Aforo completo para el horario solicitado."}</div>
        </div>

        <div class='details-box'>
          <div class='detail-row'>
            <span class='detail-label'>Codigo de solicitud</span>
            <span class='detail-value' style='font-family:monospace;'>{reservation.ReservationCode}</span>
          </div>
          <div class='detail-row'>
            <span class='detail-label'>Fecha solicitada</span>
            <span class='detail-value'>{reservation.ReservationDate:dd/MM/yyyy} a las {reservation.ReservationTime}</span>
          </div>
          <div class='detail-row'>
            <span class='detail-label'>Comensales</span>
            <span class='detail-value'>{reservation.PartySize} personas</span>
          </div>
        </div>

        <p style='font-size:14px; color:#475569; line-height:1.6;'>Te invitamos a elegir otro horario o fecha disponible a traves de nuestra plataforma. Puedes contactarnos directamente en <strong>{reservation.Restaurant?.Phone ?? "N/A"}</strong>.</p>
      </div>
      <div class='footer'>
        <p>Sistema Automatizado de Reservas &bull; {restaurantName}</p>
        <p>{reservation.Restaurant?.Address ?? ""}</p>
      </div>
    </div>
  </div>
</body>
</html>";

            await RecordAndOptionallySendAsync(reservation, subject, bodyHtml);
        }

        private async Task RecordAndOptionallySendAsync(Reservation reservation, string subject, string bodyHtml)
        {
            var status = "Enviado (Simulacion)";

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
                        From = new MailAddress(smtpFrom, reservation.Restaurant?.Name ?? "SistemaReservas"),
                        Subject = subject,
                        Body = bodyHtml,
                        IsBodyHtml = true
                    };
                    mail.To.Add(reservation.CustomerEmail);

                    await client.SendMailAsync(mail);
                    status = "Enviado via SMTP";
                    _logger.LogInformation("Correo enviado via SMTP a {Email}", reservation.CustomerEmail);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error al enviar correo SMTP. Registrando en bandeja de notificaciones.");
                    status = "Error SMTP (Guardado en bandeja)";
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
