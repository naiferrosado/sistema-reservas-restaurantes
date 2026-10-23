using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaReservas.Data;
using SistemaReservas.Models;
using SistemaReservas.Services;
using SistemaReservas.ViewModels;

namespace SistemaReservas.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            ApplicationDbContext context,
            IEmailService emailService,
            ILogger<DashboardController> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        // GET /Dashboard/Index?restaurantId=1&tab=reservations
        [HttpGet]
        public async Task<IActionResult> Index(int? restaurantId, string? tab, string? filterStatus, string? filterSearch, DateTime? filterDate)
        {
            var restaurants = await _context.Restaurants.OrderBy(r => r.Id).ToListAsync();

            if (!restaurants.Any())
            {
                return View(new DashboardViewModel());
            }

            var selectedId = restaurantId ?? restaurants.First().Id;
            var selectedRestaurant = restaurants.FirstOrDefault(r => r.Id == selectedId) ?? restaurants.First();

            var today = DateTime.UtcNow.Date;

            // KPIs
            var todayReservations = await _context.Reservations
                .Where(r => r.RestaurantId == selectedId && r.ReservationDate.Date == today)
                .ToListAsync();

            var allTables = await _context.Tables
                .Where(t => t.RestaurantId == selectedId)
                .OrderBy(t => t.Location)
                .ThenBy(t => t.TableNumber)
                .ToListAsync();

            int totalTables = allTables.Count;
            int occupiedTables = allTables.Count(t => t.Status == "Ocupada" || t.Status == "Reservada");
            double occupancyRate = totalTables > 0
                ? Math.Round(((double)occupiedTables / totalTables) * 100, 1)
                : 0.0;

            // Tabla de reservas con filtros
            var resQuery = _context.Reservations
                .Include(r => r.Restaurant)
                .Include(r => r.Table)
                .Where(r => r.RestaurantId == selectedId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filterStatus) && filterStatus != "Todos")
                resQuery = resQuery.Where(r => r.Status == filterStatus);

            if (filterDate.HasValue)
            {
                var fd = filterDate.Value.Date;
                resQuery = resQuery.Where(r => r.ReservationDate.Date == fd);
            }

            if (!string.IsNullOrWhiteSpace(filterSearch))
            {
                var s = filterSearch.ToLower();
                resQuery = resQuery.Where(r =>
                    r.CustomerName.ToLower().Contains(s) ||
                    r.CustomerEmail.ToLower().Contains(s) ||
                    r.ReservationCode.ToLower().Contains(s));
            }

            var reservations = await resQuery
                .OrderByDescending(r => r.ReservationDate)
                .ThenBy(r => r.ReservationTime)
                .ToListAsync();

            // Mesas agrupadas por zona para plano
            var tablesByZone = allTables
                .GroupBy(t => t.Location)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Bandeja de correos
            var notifications = await _context.Notifications
                .Where(n => n.ReservationId != null)
                .OrderByDescending(n => n.SentAt)
                .Take(50)
                .ToListAsync();

            // Menú
            var menuItems = await _context.MenuItems
                .Where(m => m.RestaurantId == selectedId)
                .OrderBy(m => m.Category)
                .ThenBy(m => m.Name)
                .ToListAsync();

            var menuByCategory = menuItems
                .GroupBy(m => m.Category)
                .ToDictionary(g => g.Key, g => g.ToList());

            var vm = new DashboardViewModel
            {
                SelectedRestaurantId = selectedId,
                Restaurants = restaurants,
                SelectedRestaurant = selectedRestaurant,
                ActiveTab = tab ?? "reservations",
                TotalReservationsToday = todayReservations.Count,
                ExpectedGuestsToday = todayReservations
                    .Where(r => r.Status == "Aprobada" || r.Status == "Confirmada" || r.Status == "Pendiente")
                    .Sum(r => r.PartySize),
                OccupancyRate = occupancyRate,
                PendingCount = todayReservations.Count(r => r.Status == "Pendiente"),
                ConfirmedCount = todayReservations.Count(r => r.Status == "Aprobada" || r.Status == "Confirmada"),
                RejectedCount = todayReservations.Count(r => r.Status == "Rechazada"),
                TotalTables = totalTables,
                OccupiedTables = occupiedTables,
                Reservations = reservations,
                FilterStatus = filterStatus,
                FilterSearch = filterSearch,
                FilterDate = filterDate,
                TablesByZone = tablesByZone,
                Notifications = notifications,
                MenuItems = menuItems,
                MenuByCategory = menuByCategory
            };

            return View(vm);
        }

        // POST /Dashboard/ApproveReservation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveReservation(int id, int restaurantId)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Restaurant)
                .Include(r => r.Table)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
                return NotFound();

            var previousStatus = reservation.Status;
            reservation.Status = "Aprobada";
            reservation.UpdatedAt = DateTime.UtcNow;

            // Actualizar estado de mesa si la reserva es para hoy
            if (reservation.Table != null && reservation.ReservationDate.Date == DateTime.UtcNow.Date)
            {
                reservation.Table.Status = "Reservada";
            }

            await _context.SaveChangesAsync();

            // Enviar correo de confirmación
            if (previousStatus != "Aprobada" && previousStatus != "Confirmada")
            {
                try
                {
                    await _emailService.SendReservationConfirmationAsync(reservation);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al enviar correo de aprobación para reserva {Id}", id);
                }
            }

            TempData["SuccessMessage"] = $"Reserva {reservation.ReservationCode} aprobada correctamente.";
            return RedirectToAction(nameof(Index), new { restaurantId, tab = "reservations" });
        }

        // POST /Dashboard/RejectReservation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectReservation(int id, int restaurantId, string? reason)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Restaurant)
                .Include(r => r.Table)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
                return NotFound();

            var previousStatus = reservation.Status;
            reservation.Status = "Rechazada";
            reservation.AdminNotes = reason ?? "Sin motivo especificado.";
            reservation.UpdatedAt = DateTime.UtcNow;

            // Liberar la mesa si fue asignada
            if (reservation.Table != null)
            {
                var hasOtherActiveReservations = await _context.Reservations.AnyAsync(r =>
                    r.TableId == reservation.TableId &&
                    r.Id != reservation.Id &&
                    (r.Status == "Aprobada" || r.Status == "Confirmada" || r.Status == "Pendiente"));

                if (!hasOtherActiveReservations)
                {
                    reservation.Table.Status = "Disponible";
                }
            }

            await _context.SaveChangesAsync();

            if (previousStatus != "Rechazada")
            {
                try
                {
                    await _emailService.SendReservationRejectionAsync(reservation, reason);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al enviar correo de rechazo para reserva {Id}", id);
                }
            }

            TempData["SuccessMessage"] = $"Reserva {reservation.ReservationCode} rechazada.";
            return RedirectToAction(nameof(Index), new { restaurantId, tab = "reservations" });
        }

        // ===========================
        // ENDPOINT API (para Swagger)
        // ===========================

        [AllowAnonymous]
        [HttpGet]
        [Route("api/dashboard/stats")]
        public async Task<IActionResult> ApiGetStats([FromQuery] int? restaurantId)
        {
            var today = DateTime.UtcNow.Date;

            var resQuery = _context.Reservations.AsQueryable();
            var tablesQuery = _context.Tables.AsQueryable();

            if (restaurantId.HasValue)
            {
                resQuery = resQuery.Where(r => r.RestaurantId == restaurantId.Value);
                tablesQuery = tablesQuery.Where(t => t.RestaurantId == restaurantId.Value);
            }

            var todayReservations = await resQuery
                .Where(r => r.ReservationDate.Date == today)
                .ToListAsync();

            var allTables = await tablesQuery.ToListAsync();

            int totalTables = allTables.Count;
            int occupiedTables = allTables.Count(t => t.Status == "Ocupada" || t.Status == "Reservada");
            double occupancyRate = totalTables > 0
                ? Math.Round(((double)occupiedTables / totalTables) * 100, 1)
                : 0.0;

            return Json(new
            {
                TotalReservationsToday = todayReservations.Count,
                ExpectedGuestsToday = todayReservations
                    .Where(r => r.Status == "Aprobada" || r.Status == "Confirmada" || r.Status == "Pendiente")
                    .Sum(r => r.PartySize),
                PendingCount = todayReservations.Count(r => r.Status == "Pendiente"),
                ConfirmedCount = todayReservations.Count(r => r.Status == "Aprobada" || r.Status == "Confirmada"),
                RejectedCount = todayReservations.Count(r => r.Status == "Rechazada"),
                TotalTables = totalTables,
                OccupiedTables = occupiedTables,
                OccupancyRate = occupancyRate
            });
        }
    }
}
