using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaReservas.Data;
using SistemaReservas.ViewModels;

namespace SistemaReservas.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET / o /Home/Index?restaurantId=1&tab=menu&category=Entrantes
        public async Task<IActionResult> Index(int? restaurantId, string? tab, string? category)
        {
            var restaurants = await _context.Restaurants
                .OrderBy(r => r.Id)
                .ToListAsync();

            if (!restaurants.Any())
            {
                return View(new RestaurantCatalogViewModel());
            }

            var selectedId = restaurantId ?? restaurants.First().Id;
            var selectedRestaurant = restaurants.FirstOrDefault(r => r.Id == selectedId)
                                     ?? restaurants.First();

            // Carta del restaurante activo
            var menuItems = await _context.MenuItems
                .Where(m => m.RestaurantId == selectedId && m.IsAvailable)
                .OrderBy(m => m.Category)
                .ThenBy(m => m.Name)
                .ToListAsync();

            var categories = menuItems.Select(m => m.Category).Distinct().OrderBy(c => c).ToList();

            // Filtrar por categoría si viene en query
            var filteredMenu = string.IsNullOrEmpty(category)
                ? menuItems
                : menuItems.Where(m => m.Category == category).ToList();

            var menuByCategory = filteredMenu
                .GroupBy(m => m.Category)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Mesas del restaurante activo agrupadas por zona
            var tables = await _context.Tables
                .Where(t => t.RestaurantId == selectedId)
                .OrderBy(t => t.Location)
                .ThenBy(t => t.TableNumber)
                .ToListAsync();

            var tablesByZone = tables
                .GroupBy(t => t.Location)
                .ToDictionary(g => g.Key, g => g.ToList());

            var vm = new RestaurantCatalogViewModel
            {
                Restaurants = restaurants,
                SelectedRestaurant = selectedRestaurant,
                SelectedRestaurantId = selectedId,
                MenuByCategory = menuByCategory,
                TablesByZone = tablesByZone,
                ActiveTab = tab ?? "menu",
                ActiveCategory = category,
                Categories = categories
            };

            return View(vm);
        }
    }
}
