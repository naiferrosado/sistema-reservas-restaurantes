using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaReservas.Data;
using SistemaReservas.DTOs;
using SistemaReservas.Models;

namespace SistemaReservas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TablesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TablesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RestaurantTable>>> GetTables([FromQuery] int? restaurantId)
        {
            var query = _context.Tables.AsQueryable();

            if (restaurantId.HasValue)
                query = query.Where(t => t.RestaurantId == restaurantId.Value);

            var tables = await query.OrderBy(t => t.Location).ThenBy(t => t.TableNumber).ToListAsync();
            return Ok(tables);
        }

        [HttpPost]
        public async Task<ActionResult<RestaurantTable>> CreateTable([FromBody] CreateTableDto dto)
        {
            var restaurant = await _context.Restaurants.FindAsync(dto.RestaurantId);
            if (restaurant == null)
                return BadRequest(new { message = "Restaurante no válido" });

            var table = new RestaurantTable
            {
                RestaurantId = dto.RestaurantId,
                TableNumber = dto.TableNumber,
                Capacity = dto.Capacity,
                Location = dto.Location,
                Status = dto.Status,
                PosX = dto.PosX,
                PosY = dto.PosY
            };

            _context.Tables.Add(table);
            await _context.SaveChangesAsync();

            return Ok(table);
        }

        // Cambiar estado de mesa en tiempo real (Disponible -> Reservada -> Ocupada)
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateTableStatusDto dto)
        {
            var table = await _context.Tables.FindAsync(id);
            if (table == null)
                return NotFound(new { message = "Mesa no encontrada" });

            table.Status = dto.Status;
            await _context.SaveChangesAsync();

            return Ok(table);
        }

        // Toggle cíclico: Disponible -> Reservada -> Ocupada -> Disponible
        [HttpPost("{id}/toggle")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var table = await _context.Tables.FindAsync(id);
            if (table == null)
                return NotFound(new { message = "Mesa no encontrada" });

            table.Status = table.Status switch
            {
                "Disponible" => "Reservada",
                "Reservada" => "Ocupada",
                "Ocupada" => "Disponible",
                _ => "Disponible"
            };

            await _context.SaveChangesAsync();

            return Ok(new { tableId = table.Id, newStatus = table.Status, tableNumber = table.TableNumber });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTable(int id)
        {
            var table = await _context.Tables.FindAsync(id);
            if (table == null)
                return NotFound(new { message = "Mesa no encontrada" });

            _context.Tables.Remove(table);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
