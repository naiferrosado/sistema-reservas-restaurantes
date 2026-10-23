using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaReservas.Data;
using SistemaReservas.DTOs;
using SistemaReservas.Models;
using SistemaReservas.Services;
using SistemaReservas.ViewModels;

namespace SistemaReservas.Controllers
{
    public class ReservationsController : Controller
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

        // GET /Reservations/Create?restaurantId=1
        [HttpGet]
        public async Task<IActionResult> Create(int? restaurantId)
        {
            var restaurants = await _context.Restaurants.OrderBy(r => r.Id).ToListAsync();

            var selectedId = restaurantId ?? restaurants.FirstOrDefault()?.Id ?? 1;
            var restaurant = restaurants.FirstOrDefault(r => r.Id == selectedId);

            var availableTables = await _context.Tables
                .Where(t => t.RestaurantId == selectedId && t.Status != "Mantenimiento")
                .OrderBy(t => t.Location)
                .ThenBy(t => t.TableNumber)
                .ToListAsync();

            var vm = new CreateReservationViewModel
            {
                RestaurantId = selectedId,
                Restaurant = restaurant,
                Restaurants = restaurants,
                AvailableTables = availableTables,
                ReservationDate = DateTime.Today.AddDays(1)
            };

            return View(vm);
        }

        // POST /Reservations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateReservationViewModel vm)
        {
            // Recargar datos para re-render si hay errores
            var restaurants = await _context.Restaurants.OrderBy(r => r.Id).ToListAsync();
            vm.Restaurants = restaurants;
            vm.Restaurant = restaurants.FirstOrDefault(r => r.Id == vm.RestaurantId);
            vm.AvailableTables = await _context.Tables
                .Where(t => t.RestaurantId == vm.RestaurantId && t.Status != "Mantenimiento")
                .OrderBy(t => t.Location)
                .ThenBy(t => t.TableNumber)
                .ToListAsync();

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var restaurant = await _context.Restaurants
                .Include(r => r.Tables)
                .FirstOrDefaultAsync(r => r.Id == vm.RestaurantId);

            if (restaurant == null)
            {
                ModelState.AddModelError("", "El restaurante seleccionado no existe.");
                return View(vm);
            }

            var targetDate = DateTime.SpecifyKind(vm.ReservationDate.Date, DateTimeKind.Utc);

            RestaurantTable? selectedTable = null;

            if (vm.TableId.HasValue)
            {
                selectedTable = await _context.Tables
                    .FirstOrDefaultAsync(t => t.Id == vm.TableId.Value && t.RestaurantId == vm.RestaurantId);

                if (selectedTable == null)
                {
                    ModelState.AddModelError("TableId", "La mesa seleccionada no pertenece a este restaurante.");
                    return View(vm);
                }

                if (selectedTable.Capacity < vm.PartySize)
                {
                    ModelState.AddModelError("PartySize", $"La mesa seleccionada tiene capacidad para {selectedTable.Capacity} personas. Selecciona una mesa con mayor capacidad o reduce el número de comensales.");
                    return View(vm);
                }

                var isOccupied = await _context.Reservations.AnyAsync(r =>
                    r.TableId == vm.TableId.Value &&
                    r.ReservationDate.Date == targetDate &&
                    r.ReservationTime == vm.ReservationTime &&
                    (r.Status == "Confirmada" || r.Status == "Pendiente" || r.Status == "Aprobada"));

                if (isOccupied)
                {
                    ModelState.AddModelError("", $"La Mesa {selectedTable.TableNumber} ya tiene una reserva para el {vm.ReservationDate:dd/MM/yyyy} a las {vm.ReservationTime}. Selecciona otro horario o mesa.");
                    return View(vm);
                }
            }
            else
            {
                // Autoasignar la mejor mesa disponible con capacidad adecuada
                var busyTableIds = await _context.Reservations
                    .Where(r => r.RestaurantId == vm.RestaurantId &&
                                r.ReservationDate.Date == targetDate &&
                                r.ReservationTime == vm.ReservationTime &&
                                (r.Status == "Confirmada" || r.Status == "Pendiente" || r.Status == "Aprobada") &&
                                r.TableId.HasValue)
                    .Select(r => r.TableId!.Value)
                    .ToListAsync();

                selectedTable = restaurant.Tables
                    .Where(t => !busyTableIds.Contains(t.Id) && t.Capacity >= vm.PartySize && t.Status != "Mantenimiento")
                    .OrderBy(t => t.Capacity)
                    .FirstOrDefault();
            }

            // Generar código único RES-XXXX-XXXX
            var code = $"RES-{DateTime.UtcNow:MMdd}-{Random.Shared.Next(1000, 9999)}";

            var reservation = new Reservation
            {
                ReservationCode = code,
                RestaurantId = vm.RestaurantId,
                TableId = selectedTable?.Id,
                CustomerName = vm.CustomerName.Trim(),
                CustomerEmail = vm.CustomerEmail.Trim().ToLower(),
                CustomerPhone = vm.CustomerPhone.Trim(),
                ReservationDate = targetDate,
                ReservationTime = vm.ReservationTime.Trim(),
                PartySize = vm.PartySize,
                SpecialRequests = vm.SpecialRequests?.Trim() ?? string.Empty,
                Status = "Pendiente",
                CreatedAt = DateTime.UtcNow
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            // Cargar relaciones para la vista de confirmación
            reservation.Restaurant = restaurant;
            reservation.Table = selectedTable;

            return RedirectToAction(nameof(Confirmation), new { id = reservation.Id });
        }

        // GET /Reservations/Confirmation/5
        [HttpGet]
        public async Task<IActionResult> Confirmation(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Restaurant)
                .Include(r => r.Table)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
            {
                return NotFound();
            }

            var vm = new ReservationConfirmationViewModel
            {
                Reservation = reservation,
                Restaurant = reservation.Restaurant,
                Table = reservation.Table
            };

            return View(vm);
        }

        // GET /Reservations/Lookup
        [HttpGet]
        public IActionResult Lookup()
        {
            return View(new ReservationLookupViewModel());
        }

        // POST /Reservations/Lookup
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Lookup(ReservationLookupViewModel vm)
        {
            vm.HasSearched = true;

            if (string.IsNullOrWhiteSpace(vm.SearchCode) && string.IsNullOrWhiteSpace(vm.SearchEmail))
            {
                vm.ErrorMessage = "Ingresa el código de reserva o tu correo electrónico para continuar.";
                return View(vm);
            }

            var query = _context.Reservations
                .Include(r => r.Restaurant)
                .Include(r => r.Table)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(vm.SearchCode))
            {
                var code = vm.SearchCode.Trim().ToUpper();
                query = query.Where(r => r.ReservationCode == code);
            }
            else if (!string.IsNullOrWhiteSpace(vm.SearchEmail))
            {
                var email = vm.SearchEmail.Trim().ToLower();
                query = query.Where(r => r.CustomerEmail == email);
            }

            vm.Results = await query
                .OrderByDescending(r => r.ReservationDate)
                .ThenByDescending(r => r.CreatedAt)
                .ToListAsync();

            if (!vm.Results.Any())
            {
                vm.ErrorMessage = "No se encontraron reservas con los datos proporcionados.";
            }

            return View(vm);
        }

        // ===========================
        // ENDPOINTS API (para Swagger)
        // ===========================

        [HttpGet]
        [Route("api/reservations")]
        public async Task<IActionResult> ApiGetReservations(
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
                query = query.Where(r => r.RestaurantId == restaurantId.Value);

            if (!string.IsNullOrWhiteSpace(status) && status != "Todos")
                query = query.Where(r => r.Status == status);

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
                                         r.ReservationCode.ToLower().Contains(s));
            }

            var reservations = await query
                .OrderByDescending(r => r.ReservationDate)
                .ToListAsync();

            return Json(reservations);
        }

        [HttpGet]
        [Route("api/reservations/lookup")]
        public async Task<IActionResult> ApiLookup([FromQuery] string? code, [FromQuery] string? email)
        {
            if (string.IsNullOrWhiteSpace(code) && string.IsNullOrWhiteSpace(email))
                return BadRequest(new { message = "Se requiere código o correo electrónico" });

            var query = _context.Reservations
                .Include(r => r.Restaurant)
                .Include(r => r.Table)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(code))
                query = query.Where(r => r.ReservationCode == code.Trim().ToUpper());
            else if (!string.IsNullOrWhiteSpace(email))
                query = query.Where(r => r.CustomerEmail.ToLower() == email.Trim().ToLower());

            var results = await query.OrderByDescending(r => r.ReservationDate).ToListAsync();
            return Json(results);
        }
    }
}
