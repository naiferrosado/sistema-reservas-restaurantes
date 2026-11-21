using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaReservas.Models;
using SistemaReservas.Services;

namespace SistemaReservas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador,Gerente / Encargado,Anfitrión / Recepcionista")]
    public class NotificationsController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public NotificationsController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NotificationLog>>> GetNotifications([FromQuery] int? restaurantId)
        {
            var logs = await _emailService.GetRecentNotificationsAsync(restaurantId);
            return Ok(logs);
        }
    }
}
