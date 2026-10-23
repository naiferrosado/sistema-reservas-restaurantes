using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaReservas.Data;
using SistemaReservas.DTOs;

namespace SistemaReservas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("stats")]
        public async Task<ActionResult<DashboardStatsDto>> GetStats([FromQuery] int? restaurantId)
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
            int availableTables = allTables.Count(t => t.Status == "Disponible");

            int totalReservationsToday = todayReservations.Count;
            int expectedGuestsToday = todayReservations
                .Where(r => r.Status == "Confirmada" || r.Status == "Pendiente")
                .Sum(r => r.PartySize);

            int confirmedCount = todayReservations.Count(r => r.Status == "Confirmada");
            int pendingCount = todayReservations.Count(r => r.Status == "Pendiente");
            int rejectedCount = todayReservations.Count(r => r.Status == "Rechazada");

            // Tasa de ocupación basada en mesas ocupadas / total de mesas
            double occupancyRate = totalTables > 0 
                ? Math.Round(((double)occupiedTables / totalTables) * 100, 1) 
                : 0.0;

            // Desglose horario
            var hourlyOccupancy = todayReservations
                .GroupBy(r => r.ReservationTime)
                .Select(g => new HourlyOccupancyDto
                {
                    TimeSlot = g.Key,
                    ReservationsCount = g.Count(),
                    GuestsCount = g.Sum(r => r.PartySize)
                })
                .OrderBy(h => h.TimeSlot)
                .ToList();

            var stats = new DashboardStatsDto
            {
                TotalReservationsToday = totalReservationsToday,
                ExpectedGuestsToday = expectedGuestsToday,
                ConfirmedCount = confirmedCount,
                PendingCount = pendingCount,
                RejectedCount = rejectedCount,
                TotalTables = totalTables,
                OccupiedTables = occupiedTables,
                AvailableTables = availableTables,
                OccupancyRate = occupancyRate,
                HourlyOccupancy = hourlyOccupancy
            };

            return Ok(stats);
        }
    }
}
