using Microsoft.EntityFrameworkCore;
using SistemaReservas.Models;

namespace SistemaReservas.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            if (context.Restaurants.Any())
            {
                return; // Ya fue inicializada
            }

            var r1 = new Restaurant
            {
                Name = "La Terraza Gourmet",
                Description = "Experiencia culinaria de autor con vistas panorámicas, cocina mediterránea contemporánea y maridajes excepcionales seleccionados por sumiller.",
                Address = "Paseo de la Marina 45, Distrito Gastronómico",
                Phone = "+34 912 345 678",
                Email = "contacto@laterrazagourmet.com",
                CuisineType = "Mediterránea & Autor",
                OpeningTime = "13:00",
                ClosingTime = "23:30",
                ImageUrl = "https://images.unsplash.com/photo-1517248135467-4c7edcad34c4?auto=format&fit=crop&w=1200&q=80",
                Rating = 4.9,
                PriceRange = "$$$",
                TotalCapacity = 42
            };

            var r2 = new Restaurant
            {
                Name = "Trattoria Don Corleone",
                Description = "Auténtica tradición italiana desde 1985. Pastas frescas amasadas a mano a diario, pizzas napolitanas al horno de piedra volcánica y salsas de la nonna.",
                Address = "Calle Bellas Artes 12, Centro Histórico",
                Phone = "+34 913 987 654",
                Email = "prenotazioni@doncorleone.com",
                CuisineType = "Italiana Tradicional",
                OpeningTime = "12:30",
                ClosingTime = "23:00",
                ImageUrl = "https://images.unsplash.com/photo-1555396273-367ea4eb4db5?auto=format&fit=crop&w=1200&q=80",
                Rating = 4.7,
                PriceRange = "$$",
                TotalCapacity = 38
            };

            var r3 = new Restaurant
            {
                Name = "Sakura Izakaya & Robata Grill",
                Description = "Vanguardia gastronómica nipona. Nigiris y sashimis de corte maestro, brochetas al carbón binchotan japonés y coctelería botánica oriental.",
                Address = "Avenida Sakura 88, Zona Financiera",
                Phone = "+34 911 223 344",
                Email = "reservas@sakuraizakaya.com",
                CuisineType = "Japonesa & Robata Grill",
                OpeningTime = "13:30",
                ClosingTime = "00:00",
                ImageUrl = "https://images.unsplash.com/photo-1550966871-3ed3cdb5ed0c?auto=format&fit=crop&w=1200&q=80",
                Rating = 4.8,
                PriceRange = "$$$$",
                TotalCapacity = 36
            };

            context.Restaurants.AddRange(r1, r2, r3);
            context.SaveChanges();

            // Mesas para La Terraza Gourmet
            var tablesR1 = new List<RestaurantTable>
            {
                new() { RestaurantId = r1.Id, TableNumber = 1, Capacity = 2, Location = "Terraza Panorámica", Status = "Disponible", PosX = 1, PosY = 1 },
                new() { RestaurantId = r1.Id, TableNumber = 2, Capacity = 2, Location = "Terraza Panorámica", Status = "Reservada", PosX = 2, PosY = 1 },
                new() { RestaurantId = r1.Id, TableNumber = 3, Capacity = 4, Location = "Terraza Panorámica", Status = "Disponible", PosX = 3, PosY = 1 },
                new() { RestaurantId = r1.Id, TableNumber = 4, Capacity = 4, Location = "Salón Principal", Status = "Ocupada", PosX = 1, PosY = 2 },
                new() { RestaurantId = r1.Id, TableNumber = 5, Capacity = 6, Location = "Salón Principal", Status = "Disponible", PosX = 2, PosY = 2 },
                new() { RestaurantId = r1.Id, TableNumber = 6, Capacity = 4, Location = "Salón Principal", Status = "Disponible", PosX = 3, PosY = 2 },
                new() { RestaurantId = r1.Id, TableNumber = 7, Capacity = 8, Location = "Zona VIP / Reservado", Status = "Disponible", PosX = 1, PosY = 3 },
                new() { RestaurantId = r1.Id, TableNumber = 8, Capacity = 2, Location = "Balcón Romántico", Status = "Reservada", PosX = 2, PosY = 3 }
            };

            // Mesas para Don Corleone
            var tablesR2 = new List<RestaurantTable>
            {
                new() { RestaurantId = r2.Id, TableNumber = 1, Capacity = 2, Location = "Patio Toscano", Status = "Disponible", PosX = 1, PosY = 1 },
                new() { RestaurantId = r2.Id, TableNumber = 2, Capacity = 4, Location = "Patio Toscano", Status = "Disponible", PosX = 2, PosY = 1 },
                new() { RestaurantId = r2.Id, TableNumber = 3, Capacity = 4, Location = "Salón de la Nonna", Status = "Reservada", PosX = 3, PosY = 1 },
                new() { RestaurantId = r2.Id, TableNumber = 4, Capacity = 6, Location = "Salón de la Nonna", Status = "Disponible", PosX = 1, PosY = 2 },
                new() { RestaurantId = r2.Id, TableNumber = 5, Capacity = 4, Location = "Cava de Vinos", Status = "Disponible", PosX = 2, PosY = 2 },
                new() { RestaurantId = r2.Id, TableNumber = 6, Capacity = 8, Location = "Cava de Vinos", Status = "Ocupada", PosX = 3, PosY = 2 }
            };

            // Mesas para Sakura Izakaya
            var tablesR3 = new List<RestaurantTable>
            {
                new() { RestaurantId = r3.Id, TableNumber = 1, Capacity = 2, Location = "Barra Omakase", Status = "Disponible", PosX = 1, PosY = 1 },
                new() { RestaurantId = r3.Id, TableNumber = 2, Capacity = 2, Location = "Barra Omakase", Status = "Reservada", PosX = 2, PosY = 1 },
                new() { RestaurantId = r3.Id, TableNumber = 3, Capacity = 4, Location = "Jardín Zen", Status = "Disponible", PosX = 3, PosY = 1 },
                new() { RestaurantId = r3.Id, TableNumber = 4, Capacity = 4, Location = "Jardín Zen", Status = "Disponible", PosX = 1, PosY = 2 },
                new() { RestaurantId = r3.Id, TableNumber = 5, Capacity = 6, Location = "Salón Tatami VIP", Status = "Disponible", PosX = 2, PosY = 2 }
            };

            context.Tables.AddRange(tablesR1);
            context.Tables.AddRange(tablesR2);
            context.Tables.AddRange(tablesR3);
            context.SaveChanges();

            // Platos para La Terraza Gourmet
            var menuR1 = new List<MenuItem>
            {
                new() { RestaurantId = r1.Id, Name = "Carpaccio de Pulpo con Vinagreta de Cítricos", Description = "Finas láminas de pulpo braseado, aceite de oliva virgen extra arbequina, microbrotes y perlas de lima.", Price = 18.50m, Category = "Entrantes", Tags = "Sin Gluten, Marisco", ImageUrl = "https://images.unsplash.com/photo-1544025162-d76694265947?auto=format&fit=crop&w=600&q=80" },
                new() { RestaurantId = r1.Id, Name = "Burrata di Puglia con Higos y Jamón Ibérico", Description = "Cremosa burrata fresca con higos caramelizados, reducción de Pedro Ximénez y lascas de bellota.", Price = 16.00m, Category = "Entrantes", Tags = "Vegetariano Adaptable", ImageUrl = "https://images.unsplash.com/photo-1592417817098-8f3d6910985b?auto=format&fit=crop&w=600&q=80" },
                new() { RestaurantId = r1.Id, Name = "Solomillo Rossini al Foie Caramelizado", Description = "Centro de solomillo madurado a la brasa, escalope de foie fresco, puré trufado y demi-glace de oporto.", Price = 29.50m, Category = "Platos Fuertes", Tags = "Especialidad del Chef", ImageUrl = "https://images.unsplash.com/photo-1558030006-450675393462?auto=format&fit=crop&w=600&q=80" },
                new() { RestaurantId = r1.Id, Name = "Arroz Meloso de Bogavante y Azafrán", Description = "Arroz bomba de autor cocinado en fondo concentrado de marisco salvaje y bogavante entero al punto.", Price = 32.00m, Category = "Platos Fuertes", Tags = "Sin Gluten, Recomendado para 2", ImageUrl = "https://images.unsplash.com/photo-1534080564583-6be75777b70a?auto=format&fit=crop&w=600&q=80" },
                new() { RestaurantId = r1.Id, Name = "Esfera de Chocolate Valrhona con Fruta de la Pasión", Description = "Mousse de chocolate negro al 72%, corazón fundente de maracuyá y helado artesanal de vainilla Bourbon.", Price = 9.50m, Category = "Postres", Tags = "Dulce Gourmet", ImageUrl = "https://images.unsplash.com/photo-1579372786545-d24232daf58c?auto=format&fit=crop&w=600&q=80" },
                new() { RestaurantId = r1.Id, Name = "Cóctel Signature Sunset Boulevard", Description = "Ginebra premium botánica, licor de saúco, zumo natural de pomelo rosa y toque de cava brut nature.", Price = 11.00m, Category = "Bebidas", Tags = "Coctelería de Autor", ImageUrl = "https://images.unsplash.com/photo-1514362545857-3bc16c4c7d1b?auto=format&fit=crop&w=600&q=80" }
            };

            // Platos para Don Corleone
            var menuR2 = new List<MenuItem>
            {
                new() { RestaurantId = r2.Id, Name = "Bruschetta Campana al Tartufo", Description = "Pan rústico al horno con tomates secos, albahaca genovesa y crema de trufa negra.", Price = 12.00m, Category = "Entrantes", Tags = "Vegetariano", ImageUrl = "https://images.unsplash.com/photo-1572695157366-5e585ab2b69f?auto=format&fit=crop&w=600&q=80" },
                new() { RestaurantId = r2.Id, Name = "Tagliatelle al Ragù Tradizionale Bolognese", Description = "Pasta fresca elaborada hoy, cocinada al dente con ragù lento de 6 horas y parmigiano reggiano 24 meses.", Price = 17.50m, Category = "Platos Fuertes", Tags = "Clásico de la Casa", ImageUrl = "https://images.unsplash.com/photo-1551183053-bf91a1d81141?auto=format&fit=crop&w=600&q=80" },
                new() { RestaurantId = r2.Id, Name = "Pizza Margherita Verace D.O.P.", Description = "Masa fermentada 48 horas, salsa San Marzano, mozzarella fior di latte y albahaca fresca.", Price = 15.00m, Category = "Platos Fuertes", Tags = "Vegetariano, Artesanal", ImageUrl = "https://images.unsplash.com/photo-1604382354936-07c5d9983bd3?auto=format&fit=crop&w=600&q=80" },
                new() { RestaurantId = r2.Id, Name = "Tiramisú Veneziano al Café Espresso", Description = "Bizcocho savoiardi impregnado en espresso italiano, crema mascarpone suave y cacao amargo puro.", Price = 8.50m, Category = "Postres", Tags = "Receta Familiar", ImageUrl = "https://images.unsplash.com/photo-1571877227200-a0d98ea607e9?auto=format&fit=crop&w=600&q=80" }
            };

            // Platos para Sakura Izakaya
            var menuR3 = new List<MenuItem>
            {
                new() { RestaurantId = r3.Id, Name = "Gyoza Wagyu Artesanal al Vapor y Plancha", Description = "Empanadillas japonesas rellenas de carne wagyu picada a cuchillo, cebollino y salsa ponzu trufada.", Price = 14.50m, Category = "Entrantes", Tags = "Favorito", ImageUrl = "https://images.unsplash.com/photo-1498654896293-37aacf113fd9?auto=format&fit=crop&w=600&q=80" },
                new() { RestaurantId = r3.Id, Name = "Nigiri Selección Moriawase (8 piezas)", Description = "Cortes selectos de atún toro, salmón flameado, vieira gallega y anguila unagi.", Price = 26.00m, Category = "Platos Fuertes", Tags = "Pescado Fresco Diario", ImageUrl = "https://images.unsplash.com/photo-1579871494447-9811cf80d66c?auto=format&fit=crop&w=600&q=80" },
                new() { RestaurantId = r3.Id, Name = "Brochetas Kushiyaki de Pollo Teriyaki", Description = "Pechuga y puerro confitados al carbón binchotan bañados en glaseado teriyaki casero.", Price = 16.00m, Category = "Platos Fuertes", Tags = "Robata Grill", ImageUrl = "https://images.unsplash.com/photo-1529193591184-b1d58069ecdd?auto=format&fit=crop&w=600&q=80" },
                new() { RestaurantId = r3.Id, Name = "Mochi Artesanal de Té Verde Matcha y Yuzu", Description = "Masa de arroz tradicional japonesa rellena de helado artesanal de matcha de Uji.", Price = 7.50m, Category = "Postres", Tags = "Tradición Japonesa", ImageUrl = "https://images.unsplash.com/photo-1563805042-7684c019e1cb?auto=format&fit=crop&w=600&q=80" }
            };

            context.MenuItems.AddRange(menuR1);
            context.MenuItems.AddRange(menuR2);
            context.MenuItems.AddRange(menuR3);
            context.SaveChanges();

            // Sembrar reservas iniciales representativas
            var today = DateTime.UtcNow.Date;
            var res1 = new Reservation
            {
                ReservationCode = "RES-1023-9842",
                RestaurantId = r1.Id,
                TableId = tablesR1[1].Id, // Mesa 2
                CustomerName = "Carlos Mendoza",
                CustomerEmail = "carlos.mendoza@ejemplo.com",
                CustomerPhone = "+34 600 123 456",
                ReservationDate = today,
                ReservationTime = "20:30",
                PartySize = 2,
                SpecialRequests = "Mesa junto a la ventana. Aniversario de bodas.",
                Status = "Confirmada",
                AdminNotes = "Asignada mesa con vistas panoramicas.",
                CreatedAt = today.AddDays(-1).AddHours(14)
            };

            var res2 = new Reservation
            {
                ReservationCode = "RES-1023-8731",
                RestaurantId = r1.Id,
                TableId = tablesR1[3].Id, // Mesa 4
                CustomerName = "Elena Navarro",
                CustomerEmail = "elena.navarro@ejemplo.com",
                CustomerPhone = "+34 611 789 012",
                ReservationDate = today,
                ReservationTime = "14:00",
                PartySize = 4,
                SpecialRequests = "Uno de los comensales es celiaco.",
                Status = "Confirmada",
                AdminNotes = "Informado al equipo de cocina.",
                CreatedAt = today.AddDays(-2).AddHours(10)
            };

            var res3 = new Reservation
            {
                ReservationCode = "RES-1024-5619",
                RestaurantId = r1.Id,
                TableId = tablesR1[4].Id, // Mesa 5
                CustomerName = "Mariano Soto",
                CustomerEmail = "mariano.soto@ejemplo.com",
                CustomerPhone = "+34 655 432 109",
                ReservationDate = today.AddDays(1),
                ReservationTime = "21:00",
                PartySize = 5,
                SpecialRequests = "Cena de equipo de empresa.",
                Status = "Pendiente",
                CreatedAt = today.AddHours(2)
            };

            var res4 = new Reservation
            {
                ReservationCode = "RES-1023-3310",
                RestaurantId = r2.Id,
                TableId = tablesR2[2].Id,
                CustomerName = "Sofia Valenzuela",
                CustomerEmail = "sofia.valenzuela@ejemplo.com",
                CustomerPhone = "+34 688 901 234",
                ReservationDate = today,
                ReservationTime = "21:30",
                PartySize = 4,
                SpecialRequests = "Celebracion familiar.",
                Status = "Confirmada",
                CreatedAt = today.AddDays(-1)
            };

            context.Reservations.AddRange(res1, res2, res3, res4);
            context.SaveChanges();

            // Notificaciones de prueba con diseno corporativo
            context.Notifications.Add(new NotificationLog
            {
                ReservationId = res1.Id,
                RecipientEmail = res1.CustomerEmail,
                RecipientName = res1.CustomerName,
                Subject = "Reserva Aprobada en La Terraza Gourmet — Codigo: RES-1023-9842",
                BodyHtml = $@"<!DOCTYPE html>
<html lang='es'>
<head><meta charset='utf-8'/><style>body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background-color: #F8FAFC; margin: 0; padding: 24px; color: #0F172A; }} .card {{ max-width: 580px; margin: 0 auto; background: #FFFFFF; border-radius: 12px; overflow: hidden; border: 1px solid #E2E8F0; }} .header {{ background: #1B365D; padding: 28px; text-align: center; color: #FFFFFF; }} .content {{ padding: 28px; }} .code-box {{ text-align: center; border: 2px dashed #CBD5E1; border-radius: 8px; padding: 16px; margin: 20px 0; background: #F8FAFC; }} .code-value {{ font-size: 24px; font-weight: 800; color: #1B365D; letter-spacing: 0.08em; font-family: monospace; }}</style></head>
<body><div class='card'><div class='header'><div style='font-size:12px;text-transform:uppercase;letter-spacing:0.08em;opacity:0.8;'>La Terraza Gourmet</div><h1 style='font-size:20px;margin:8px 0 0;'>Reserva Confirmada</h1></div><div class='content'><p>Hola, <strong>{res1.CustomerName}</strong></p><p>Tu solicitud de reserva ha sido aprobada con exito.</p><div class='code-box'><div style='font-size:11px;font-weight:700;color:#64748B;text-transform:uppercase;'>Codigo Oficial</div><div class='code-value'>RES-1023-9842</div></div><p><strong>Mesa asignada:</strong> Mesa 2 (Terraza Panoramica) &bull; 2 personas</p><p><strong>Fecha y Hora:</strong> {res1.ReservationDate:dd/MM/yyyy} a las {res1.ReservationTime} hrs</p></div></div></body></html>",
                Status = "Enviado",
                SentAt = DateTime.UtcNow.AddMinutes(-30)
            });

            context.SaveChanges();
        }
    }
}
