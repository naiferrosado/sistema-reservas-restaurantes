using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaReservas.Data;
using SistemaReservas.Models;

namespace SistemaReservas.Controllers
{
    /// <summary>
    /// API REST de restaurantes — mantiene compatibilidad con /swagger
    /// Las vistas Razor usan HomeController.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class RestaurantsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RestaurantsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Restaurant>>> GetRestaurants()
        {
            var restaurants = await _context.Restaurants
                .OrderBy(r => r.Id)
                .ToListAsync();
            return Ok(restaurants);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Restaurant>> GetRestaurant(int id)
        {
            var restaurant = await _context.Restaurants
                .Include(r => r.Tables)
                .Include(r => r.MenuItems)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (restaurant == null)
                return NotFound(new { message = "Restaurante no encontrado" });

            return Ok(restaurant);
        }

        [HttpPost]
        public async Task<ActionResult<Restaurant>> CreateRestaurant([FromBody] SistemaReservas.DTOs.CreateRestaurantDto dto)
        {
            var restaurant = new Restaurant
            {
                Name = dto.Name,
                Description = dto.Description,
                Address = dto.Address,
                Phone = dto.Phone,
                Email = dto.Email,
                CuisineType = dto.CuisineType,
                OpeningTime = dto.OpeningTime,
                ClosingTime = dto.ClosingTime,
                ImageUrl = string.IsNullOrWhiteSpace(dto.ImageUrl)
                    ? "https://images.unsplash.com/photo-1517248135467-4c7edcad34c4?auto=format&fit=crop&w=1200&q=80"
                    : dto.ImageUrl,
                Rating = dto.Rating,
                PriceRange = dto.PriceRange,
                TotalCapacity = dto.TotalCapacity
            };

            _context.Restaurants.Add(restaurant);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRestaurant), new { id = restaurant.Id }, restaurant);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRestaurant(int id, [FromBody] SistemaReservas.DTOs.CreateRestaurantDto dto)
        {
            var restaurant = await _context.Restaurants.FindAsync(id);
            if (restaurant == null)
                return NotFound(new { message = "Restaurante no encontrado" });

            restaurant.Name = dto.Name;
            restaurant.Description = dto.Description;
            restaurant.Address = dto.Address;
            restaurant.Phone = dto.Phone;
            restaurant.Email = dto.Email;
            restaurant.CuisineType = dto.CuisineType;
            restaurant.OpeningTime = dto.OpeningTime;
            restaurant.ClosingTime = dto.ClosingTime;
            if (!string.IsNullOrWhiteSpace(dto.ImageUrl))
                restaurant.ImageUrl = dto.ImageUrl;
            restaurant.Rating = dto.Rating;
            restaurant.PriceRange = dto.PriceRange;
            restaurant.TotalCapacity = dto.TotalCapacity;

            await _context.SaveChangesAsync();
            return Ok(restaurant);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRestaurant(int id)
        {
            var count = await _context.Restaurants.CountAsync();
            if (count <= 1)
                return BadRequest(new { message = "No es posible eliminar el único restaurante del sistema." });

            var restaurant = await _context.Restaurants
                .Include(r => r.Reservations)
                .Include(r => r.Tables)
                .Include(r => r.MenuItems)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (restaurant == null)
                return NotFound(new { message = "Restaurante no encontrado" });

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

            return NoContent();
        }
    }
}
