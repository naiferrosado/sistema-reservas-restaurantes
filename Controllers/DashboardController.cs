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
            var restaurants = await _context.Restaurants
                .Include(r => r.Tables)
                .Include(r => r.MenuItems)
                .Include(r => r.Reservations)
                .OrderBy(r => r.Id)
                .ToListAsync();

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

        // =====================================================================
        // CRUD GESTIÓN DE MENÚ
        // =====================================================================

        // POST /Dashboard/CreateMenuItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMenuItem(
            int restaurantId,
            string name,
            string? description,
            decimal price,
            string category,
            string? imageUrl,
            string? tags,
            bool isAvailable = true)
        {
            var restaurant = await _context.Restaurants.FindAsync(restaurantId);
            if (restaurant == null) return NotFound();

            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["ErrorMessage"] = "El nombre del platillo es obligatorio.";
                return RedirectToAction(nameof(Index), new { restaurantId, tab = "menu" });
            }

            var item = new MenuItem
            {
                RestaurantId = restaurantId,
                Name = name.Trim(),
                Description = description?.Trim() ?? string.Empty,
                Price = price > 0 ? price : 0.01m,
                Category = string.IsNullOrWhiteSpace(category) ? "Platos Fuertes" : category.Trim(),
                ImageUrl = string.IsNullOrWhiteSpace(imageUrl)
                    ? "https://images.unsplash.com/photo-1546069901-ba9599a7e63c?auto=format&fit=crop&w=600&q=80"
                    : imageUrl.Trim(),
                Tags = tags?.Trim() ?? string.Empty,
                IsAvailable = isAvailable
            };

            _context.MenuItems.Add(item);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Platillo \"{item.Name}\" agregado a la carta correctamente.";
            return RedirectToAction(nameof(Index), new { restaurantId, tab = "menu" });
        }

        // POST /Dashboard/UpdateMenuItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateMenuItem(
            int id,
            int restaurantId,
            string name,
            string? description,
            decimal price,
            string category,
            string? imageUrl,
            string? tags,
            bool isAvailable = true)
        {
            var item = await _context.MenuItems.FindAsync(id);
            if (item == null) return NotFound();

            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["ErrorMessage"] = "El nombre del platillo es obligatorio.";
                return RedirectToAction(nameof(Index), new { restaurantId, tab = "menu" });
            }

            item.Name = name.Trim();
            item.Description = description?.Trim() ?? string.Empty;
            item.Price = price > 0 ? price : 0.01m;
            item.Category = string.IsNullOrWhiteSpace(category) ? item.Category : category.Trim();
            if (!string.IsNullOrWhiteSpace(imageUrl))
                item.ImageUrl = imageUrl.Trim();
            item.Tags = tags?.Trim() ?? string.Empty;
            item.IsAvailable = isAvailable;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Platillo \"{item.Name}\" actualizado exitosamente.";
            return RedirectToAction(nameof(Index), new { restaurantId, tab = "menu" });
        }

        // POST /Dashboard/DeleteMenuItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMenuItem(int id, int restaurantId)
        {
            var item = await _context.MenuItems.FindAsync(id);
            if (item == null) return NotFound();

            var name = item.Name;
            _context.MenuItems.Remove(item);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Platillo \"{name}\" eliminado de la carta.";
            return RedirectToAction(nameof(Index), new { restaurantId, tab = "menu" });
        }

        // =====================================================================
        // CRUD PLANO DE MESAS
        // =====================================================================

        // POST /Dashboard/CreateTable
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTable(
            int restaurantId,
            int tableNumber,
            int capacity,
            string location,
            string status)
        {
            var restaurant = await _context.Restaurants.FindAsync(restaurantId);
            if (restaurant == null) return NotFound();

            if (tableNumber <= 0)
            {
                TempData["ErrorMessage"] = "El número de mesa debe ser un entero positivo mayor a cero.";
                return RedirectToAction(nameof(Index), new { restaurantId, tab = "floorplan" });
            }

            var duplicate = await _context.Tables.AnyAsync(t => t.RestaurantId == restaurantId && t.TableNumber == tableNumber);
            if (duplicate)
            {
                TempData["ErrorMessage"] = $"Ya existe una mesa con el número {tableNumber} en este restaurante.";
                return RedirectToAction(nameof(Index), new { restaurantId, tab = "floorplan" });
            }

            var table = new RestaurantTable
            {
                RestaurantId = restaurantId,
                TableNumber = tableNumber,
                Capacity = capacity > 0 ? capacity : 2,
                Location = string.IsNullOrWhiteSpace(location) ? "Salón Principal" : location.Trim(),
                Status = string.IsNullOrWhiteSpace(status) ? "Disponible" : status.Trim(),
                PosX = 0,
                PosY = 0
            };

            _context.Tables.Add(table);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Mesa {table.TableNumber} creada exitosamente en {table.Location}.";
            return RedirectToAction(nameof(Index), new { restaurantId, tab = "floorplan" });
        }

        // POST /Dashboard/UpdateTable
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTable(
            int id,
            int restaurantId,
            int tableNumber,
            int capacity,
            string location,
            string status)
        {
            var table = await _context.Tables.FindAsync(id);
            if (table == null) return NotFound();

            if (tableNumber <= 0)
            {
                TempData["ErrorMessage"] = "El número de mesa debe ser un entero positivo mayor a cero.";
                return RedirectToAction(nameof(Index), new { restaurantId, tab = "floorplan" });
            }

            var duplicate = await _context.Tables.AnyAsync(t => t.RestaurantId == restaurantId && t.TableNumber == tableNumber && t.Id != id);
            if (duplicate)
            {
                TempData["ErrorMessage"] = $"El número de mesa {tableNumber} ya está en uso por otra mesa de este restaurante.";
                return RedirectToAction(nameof(Index), new { restaurantId, tab = "floorplan" });
            }

            table.TableNumber = tableNumber;
            table.Capacity = capacity > 0 ? capacity : table.Capacity;
            table.Location = string.IsNullOrWhiteSpace(location) ? table.Location : location.Trim();
            table.Status = string.IsNullOrWhiteSpace(status) ? table.Status : status.Trim();

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Mesa {table.TableNumber} actualizada exitosamente.";
            return RedirectToAction(nameof(Index), new { restaurantId, tab = "floorplan" });
        }

        // POST /Dashboard/DeleteTable
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTable(int id, int restaurantId)
        {
            var table = await _context.Tables.FindAsync(id);
            if (table == null) return NotFound();

            var num = table.TableNumber;
            _context.Tables.Remove(table);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Mesa {num} eliminada correctamente.";
            return RedirectToAction(nameof(Index), new { restaurantId, tab = "floorplan" });
        }

        // ===========================
        // GESTIÓN DE RESTAURANTES (CRUD)
        // ===========================

        // POST /Dashboard/CreateRestaurant
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRestaurant(
            string name,
            string description,
            string address,
            string phone,
            string email,
            string cuisineType,
            string openingTime,
            string closingTime,
            string? imageUrl,
            double rating,
            string priceRange,
            int totalCapacity)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["ErrorMessage"] = "El nombre del restaurante es obligatorio.";
                return RedirectToAction(nameof(Index), new { tab = "restaurants" });
            }

            var restaurant = new Restaurant
            {
                Name = name.Trim(),
                Description = description?.Trim() ?? string.Empty,
                Address = address?.Trim() ?? "Santo Domingo, República Dominicana",
                Phone = phone?.Trim() ?? "+1 (809) 000-0000",
                Email = email?.Trim() ?? "info@restaurante.com.do",
                CuisineType = cuisineType?.Trim() ?? "Criolla Contemporánea",
                OpeningTime = string.IsNullOrWhiteSpace(openingTime) ? "12:00" : openingTime.Trim(),
                ClosingTime = string.IsNullOrWhiteSpace(closingTime) ? "23:00" : closingTime.Trim(),
                ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? "https://images.unsplash.com/photo-1517248135467-4c7edcad34c4?w=800&q=80" : imageUrl.Trim(),
                Rating = rating > 0 ? rating : 4.8,
                PriceRange = string.IsNullOrWhiteSpace(priceRange) ? "$$$" : priceRange.Trim(),
                TotalCapacity = totalCapacity > 0 ? totalCapacity : 50
            };

            _context.Restaurants.Add(restaurant);
            await _context.SaveChangesAsync();

            // Agregar mesas iniciales para el nuevo restaurante
            var starterTables = new List<RestaurantTable>
            {
                new() { RestaurantId = restaurant.Id, TableNumber = 1, Capacity = 2, Location = "Principal", Status = "Disponible" },
                new() { RestaurantId = restaurant.Id, TableNumber = 2, Capacity = 4, Location = "Principal", Status = "Disponible" },
                new() { RestaurantId = restaurant.Id, TableNumber = 3, Capacity = 6, Location = "Terraza", Status = "Disponible" },
                new() { RestaurantId = restaurant.Id, TableNumber = 4, Capacity = 4, Location = "Terraza", Status = "Disponible" }
            };
            _context.Tables.AddRange(starterTables);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Restaurante \"{restaurant.Name}\" creado exitosamente.";
            return RedirectToAction(nameof(Index), new { restaurantId = restaurant.Id, tab = "restaurants" });
        }

        // POST /Dashboard/UpdateRestaurant
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRestaurant(
            int id,
            string name,
            string description,
            string address,
            string phone,
            string email,
            string cuisineType,
            string openingTime,
            string closingTime,
            string? imageUrl,
            double rating,
            string priceRange,
            int totalCapacity)
        {
            var restaurant = await _context.Restaurants.FindAsync(id);
            if (restaurant == null) return NotFound();

            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["ErrorMessage"] = "El nombre del restaurante no puede estar vacío.";
                return RedirectToAction(nameof(Index), new { restaurantId = id, tab = "restaurants" });
            }

            restaurant.Name = name.Trim();
            restaurant.Description = description?.Trim() ?? string.Empty;
            restaurant.Address = address?.Trim() ?? restaurant.Address;
            restaurant.Phone = phone?.Trim() ?? restaurant.Phone;
            restaurant.Email = email?.Trim() ?? restaurant.Email;
            restaurant.CuisineType = cuisineType?.Trim() ?? restaurant.CuisineType;
            restaurant.OpeningTime = string.IsNullOrWhiteSpace(openingTime) ? restaurant.OpeningTime : openingTime.Trim();
            restaurant.ClosingTime = string.IsNullOrWhiteSpace(closingTime) ? restaurant.ClosingTime : closingTime.Trim();
            if (!string.IsNullOrWhiteSpace(imageUrl))
                restaurant.ImageUrl = imageUrl.Trim();
            restaurant.Rating = rating > 0 ? rating : restaurant.Rating;
            restaurant.PriceRange = string.IsNullOrWhiteSpace(priceRange) ? restaurant.PriceRange : priceRange.Trim();
            restaurant.TotalCapacity = totalCapacity > 0 ? totalCapacity : restaurant.TotalCapacity;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Restaurante \"{restaurant.Name}\" actualizado exitosamente.";
            return RedirectToAction(nameof(Index), new { restaurantId = id, tab = "restaurants" });
        }

        // POST /Dashboard/DeleteRestaurant
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRestaurant(int id)
        {
            var count = await _context.Restaurants.CountAsync();
            if (count <= 1)
            {
                TempData["ErrorMessage"] = "No es posible eliminar el único restaurante del sistema.";
                return RedirectToAction(nameof(Index), new { restaurantId = id, tab = "restaurants" });
            }

            var restaurant = await _context.Restaurants
                .Include(r => r.Reservations)
                .Include(r => r.Tables)
                .Include(r => r.MenuItems)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (restaurant == null) return NotFound();

            var name = restaurant.Name;

            if (restaurant.Reservations.Any())
            {
                var reservationIds = restaurant.Reservations.Select(res => res.Id).ToList();
                var logs = await _context.Notifications
                    .Where(n => n.ReservationId != null && reservationIds.Contains(n.ReservationId.Value))
                    .ToListAsync();
                _context.Notifications.RemoveRange(logs);
                _context.Reservations.RemoveRange(restaurant.Reservations);
            }

            _context.MenuItems.RemoveRange(restaurant.MenuItems);
            _context.Tables.RemoveRange(restaurant.Tables);
            _context.Restaurants.RemoveRange(restaurant);
            await _context.SaveChangesAsync();

            var fallback = await _context.Restaurants.OrderBy(r => r.Id).FirstOrDefaultAsync();

            TempData["SuccessMessage"] = $"Restaurante \"{name}\" eliminado correctamente.";
            return RedirectToAction(nameof(Index), new { restaurantId = fallback?.Id, tab = "restaurants" });
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
