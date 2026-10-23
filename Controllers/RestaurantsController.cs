using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaReservas.Data;
using SistemaReservas.Models;

namespace SistemaReservas.Controllers
{
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
        public async Task<ActionResult<IEnumerable<Restaurant>>> GetAll()
        {
            var restaurants = await _context.Restaurants
                .OrderBy(r => r.Id)
                .ToListAsync();
            return Ok(restaurants);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetById(int id)
        {
            var restaurant = await _context.Restaurants
                .Include(r => r.Tables)
                .Include(r => r.MenuItems)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (restaurant == null)
            {
                return NotFound(new { message = "Restaurante no encontrado" });
            }

            return Ok(new
            {
                restaurant.Id,
                restaurant.Name,
                restaurant.Description,
                restaurant.Address,
                restaurant.Phone,
                restaurant.Email,
                restaurant.CuisineType,
                restaurant.OpeningTime,
                restaurant.ClosingTime,
                restaurant.ImageUrl,
                restaurant.Rating,
                restaurant.PriceRange,
                restaurant.TotalCapacity,
                Tables = restaurant.Tables.OrderBy(t => t.TableNumber).ToList(),
                MenuItems = restaurant.MenuItems.OrderBy(m => m.Category).ThenBy(m => m.Name).ToList()
            });
        }

        [HttpPost]
        public async Task<ActionResult<Restaurant>> Create([FromBody] Restaurant restaurant)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Restaurants.Add(restaurant);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = restaurant.Id }, restaurant);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Restaurant updated)
        {
            var restaurant = await _context.Restaurants.FindAsync(id);
            if (restaurant == null)
            {
                return NotFound(new { message = "Restaurante no encontrado" });
            }

            restaurant.Name = updated.Name;
            restaurant.Description = updated.Description;
            restaurant.Address = updated.Address;
            restaurant.Phone = updated.Phone;
            restaurant.Email = updated.Email;
            restaurant.CuisineType = updated.CuisineType;
            restaurant.OpeningTime = updated.OpeningTime;
            restaurant.ClosingTime = updated.ClosingTime;
            restaurant.ImageUrl = updated.ImageUrl;
            restaurant.PriceRange = updated.PriceRange;

            await _context.SaveChangesAsync();
            return Ok(restaurant);
        }
    }
}
