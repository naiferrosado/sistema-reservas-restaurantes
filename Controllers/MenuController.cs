using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaReservas.Data;
using SistemaReservas.DTOs;
using SistemaReservas.Models;

namespace SistemaReservas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MenuController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MenuController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MenuItem>>> GetMenu([FromQuery] int? restaurantId)
        {
            var query = _context.MenuItems.AsQueryable();

            if (restaurantId.HasValue)
            {
                query = query.Where(m => m.RestaurantId == restaurantId.Value);
            }

            var items = await query
                .OrderBy(m => m.Category)
                .ThenBy(m => m.Name)
                .ToListAsync();

            return Ok(items);
        }

        [HttpPost]
        public async Task<ActionResult<MenuItem>> CreateItem([FromBody] CreateMenuItemDto dto)
        {
            var restaurant = await _context.Restaurants.FindAsync(dto.RestaurantId);
            if (restaurant == null)
            {
                return BadRequest(new { message = "Restaurante no encontrado" });
            }

            var item = new MenuItem
            {
                RestaurantId = dto.RestaurantId,
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Category = dto.Category,
                ImageUrl = string.IsNullOrWhiteSpace(dto.ImageUrl) 
                    ? "https://images.unsplash.com/photo-1546069901-ba9599a7e63c?auto=format&fit=crop&w=600&q=80" 
                    : dto.ImageUrl,
                Tags = dto.Tags,
                IsAvailable = dto.IsAvailable
            };

            _context.MenuItems.Add(item);
            await _context.SaveChangesAsync();

            return Ok(item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateItem(int id, [FromBody] CreateMenuItemDto dto)
        {
            var item = await _context.MenuItems.FindAsync(id);
            if (item == null)
            {
                return NotFound(new { message = "Platillo no encontrado" });
            }

            item.Name = dto.Name;
            item.Description = dto.Description;
            item.Price = dto.Price;
            item.Category = dto.Category;
            item.ImageUrl = dto.ImageUrl;
            item.Tags = dto.Tags;
            item.IsAvailable = dto.IsAvailable;

            await _context.SaveChangesAsync();
            return Ok(item);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var item = await _context.MenuItems.FindAsync(id);
            if (item == null)
            {
                return NotFound(new { message = "Platillo no encontrado" });
            }

            _context.MenuItems.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
