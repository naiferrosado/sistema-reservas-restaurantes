using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaReservas.Data;
using SistemaReservas.DTOs;
using SistemaReservas.Models;
using SistemaReservas.Services;

namespace SistemaReservas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<ReservationsController> _logger;

        public ReservationsController(
            ApplicationDbContext context, 
            IEmailService emailService,
            ILogger<ReservationsController> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Reservation>>> GetReservations(
            [FromQuery] int? restaurantId,
            [FromQuery] string? status,
            [FromQuery] DateTime? date,
            [FromQuery] string? search)
        {
            var query = _context.Reservations
                .Include(r => r.Restaurant)
                .Include(r => r.Table)
                .AsQueryable();

            if (restaurantId.HasValue)
            {
                query = query.Where(r => r.RestaurantId == restaurantId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "Todos")
            {
                query = query.Where(r => r.Status == status);
            }

            if (date.HasValue)
            {
                var targetDate = date.Value.Date;
                query = query.Where(r => r.ReservationDate.Date == targetDate);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                query = query.Where(r => r.CustomerName.ToLower().Contains(s) ||
                                         r.CustomerEmail.ToLower().Contains(s) ||
                                         r.ReservationCode.ToLower().Contains(s) ||
                                         r.CustomerPhone.Contains(s));
            }

            var reservations = await query
                .OrderByDescending(r => r.ReservationDate)
                .ThenByDescending(r => r.ReservationTime)
                .ToListAsync();

            return Ok(reservations);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Reservation>> GetById(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Restaurant)
                .Include(r => r.Table)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
            {
                return NotFound(new { message = "Reserva no encontrada" });
            }

            return Ok(reservation);
        }

        [HttpGet("lookup")]
        public async Task<ActionResult<IEnumerable<Reservation>>> Lookup(
            [FromQuery] string? code, 
            [FromQuery] string? email)
        {
            if (string.IsNullOrWhiteSpace(code) && string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new { message = "Se requiere código de reserva o correo electrónico" });
            }

            var query = _context.Reservations
                .Include(r => r.Restaurant)
                .Include(r => r.Table)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(code))
            {
                query = query.Where(r => r.ReservationCode == code.Trim().ToUpper());
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                query = query.Where(r => r.CustomerEmail.ToLower() == email.Trim().ToLower());
            }

            var results = await query
                .OrderByDescending(r => r.ReservationDate)
                .ToListAsync();

            return Ok(results);
        }

        [HttpPost]
        public async Task<ActionResult<Reservation>> CreateReservation([FromBody] CreateReservationDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var restaurant = await _context.Restaurants
                .Include(r => r.Tables)
                .FirstOrDefaultAsync(r => r.Id == dto.RestaurantId);

            if (restaurant == null)
            {
                return BadRequest(new { message = "Restaurante no existe" });
            }

            var targetDate = DateTime.SpecifyKind(dto.ReservationDate.Date, DateTimeKind.Utc);

            // Validar mesa si fue seleccionada
            RestaurantTable? selectedTable = null;
            if (dto.TableId.HasValue)
            {
                selectedTable = await _context.Tables.FirstOrDefaultAsync(t => t.Id == dto.TableId.Value && t.RestaurantId == dto.RestaurantId);
                if (selectedTable == null)
                {
                    return BadRequest(new { message = "La mesa seleccionada no pertenece a este restaurante." });
                }

                // Verificar si la mesa ya tiene una reserva confirmada en la misma fecha y franja horaria
                var isOccupied = await _context.Reservations.AnyAsync(r =>
                    r.TableId == dto.TableId.Value &&
                    r.ReservationDate.Date == targetDate &&
                    r.ReservationTime == dto.ReservationTime &&
                    (r.Status == "Confirmada" || r.Status == "Pendiente"));

                if (isOccupied)
                {
                    return Conflict(new { message = $"La Mesa {selectedTable.TableNumber} ya tiene una reserva para la fecha {dto.ReservationDate:dd/MM/yyyy} a las {dto.ReservationTime}." });
                }
            }
            else
            {
                // Autoasignar una mesa disponible con capacidad adecuada
                var busyTableIds = await _context.Reservations
                    .Where(r => r.RestaurantId == dto.RestaurantId &&
                                r.ReservationDate.Date == targetDate &&
                                r.ReservationTime == dto.ReservationTime &&
                                (r.Status == "Confirmada" || r.Status == "Pendiente") &&
                                r.TableId.HasValue)
                    .Select(r => r.TableId!.Value)
                    .ToListAsync();

                selectedTable = restaurant.Tables
                    .Where(t => !busyTableIds.Contains(t.Id) && t.Capacity >= dto.PartySize && t.Status != "Mantenimiento")
                    .OrderBy(t => t.Capacity)
                    .FirstOrDefault();
            }

            var code = $"RES-{DateTime.UtcNow:MMdd}-{Random.Shared.Next(1000, 9999)}";

            var reservation = new Reservation
            {
                ReservationCode = code,
                RestaurantId = dto.RestaurantId,
                TableId = selectedTable?.Id,
                CustomerName = dto.CustomerName.Trim(),
                CustomerEmail = dto.CustomerEmail.Trim().ToLower(),
                CustomerPhone = dto.CustomerPhone.Trim(),
                ReservationDate = targetDate,
                ReservationTime = dto.ReservationTime.Trim(),
                PartySize = dto.PartySize,
                SpecialRequests = dto.SpecialRequests?.Trim() ?? string.Empty,
                Status = "Pendiente",
                CreatedAt = DateTime.UtcNow
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            // Cargar datos completos para la respuesta
            reservation.Restaurant = restaurant;
            reservation.Table = selectedTable;

            return CreatedAtAction(nameof(GetById), new { id = reservation.Id }, reservation);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateReservationStatusDto dto)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Restaurant)
                .Include(r => r.Table)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
            {
                return NotFound(new { message = "Reserva no encontrada" });
            }

            var previousStatus = reservation.Status;
            reservation.Status = dto.Status;
            reservation.UpdatedAt = DateTime.UtcNow;

            if (dto.TableId.HasValue)
            {
                reservation.TableId = dto.TableId.Value;
                reservation.Table = await _context.Tables.FindAsync(dto.TableId.Value);
            }

            if (!string.IsNullOrEmpty(dto.AdminNotes))
            {
                reservation.AdminNotes = dto.AdminNotes;
            }

            // Si se aprueba/confirma, sincronizar estado de mesa si es para hoy
            if (dto.Status == "Confirmada" && reservation.Table != null)
            {
                if (reservation.ReservationDate.Date == DateTime.UtcNow.Date)
                {
                    reservation.Table.Status = "Reservada";
                }
            }

            await _context.SaveChangesAsync();

            // Enviar correo de confirmación o rechazo según la consigna
            if (dto.Status == "Confirmada" && previousStatus != "Confirmada")
            {
                try
                {
                    await _emailService.SendReservationConfirmationAsync(reservation);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al enviar notificación de confirmación de reserva {Id}", reservation.Id);
                }
            }
            else if (dto.Status == "Rechazada" && previousStatus != "Rechazada")
            {
                try
                {
                    await _emailService.SendReservationRejectionAsync(reservation, dto.AdminNotes);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al enviar notificación de rechazo de reserva {Id}", reservation.Id);
                }
            }

            return Ok(reservation);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelReservation(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound(new { message = "Reserva no encontrada" });
            }

            reservation.Status = "Cancelada";
            reservation.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Reserva cancelada exitosamente", reservation });
        }
    }
}
