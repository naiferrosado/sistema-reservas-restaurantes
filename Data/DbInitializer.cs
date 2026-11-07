using Microsoft.EntityFrameworkCore;
using SistemaReservas.Models;

namespace SistemaReservas.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            // Asegurar tabla Users y sembrar administrador inicial
            try
            {
                context.Database.ExecuteSqlRaw(@"
                    CREATE TABLE IF NOT EXISTS ""Users"" (
                        ""Id"" SERIAL PRIMARY KEY,
                        ""Email"" VARCHAR(150) NOT NULL UNIQUE,
                        ""PasswordHash"" VARCHAR(500) NOT NULL,
                        ""FullName"" VARCHAR(150) NOT NULL,
                        ""Role"" VARCHAR(50) NOT NULL,
                        ""CreatedAt"" TIMESTAMP WITHOUT TIME ZONE NOT NULL
                    );
                ");

                if (!context.Users.Any())
                {
                    var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<string>();
                    var adminUser = new User
                    {
                        Email = "admin@sistemareservas.com",
                        FullName = "Administrador General",
                        Role = "Administrador",
                        PasswordHash = hasher.HashPassword("admin@sistemareservas.com", "Admin123!"),
                        CreatedAt = DateTime.UtcNow
                    };
                    context.Users.Add(adminUser);
                    context.SaveChanges();
                }
            }
            catch
            {
                // Manejo preventivo si la tabla ya está gestionada
            }

            if (context.Restaurants.Any())
            {
                return; // Ya fue inicializada
            }

            var r1 = new Restaurant
            {
                Name = "Mesón Colonial Quisqueya",
                Description = "Experiencia gastronómica dominicana de autor en el corazón de la Ciudad Colonial, fusionando recetas criollas ancestrales con alta cocina caribeña contemporánea.",
                Address = "Calle Las Damas 102, Ciudad Colonial, Santo Domingo",
                Phone = "+1 809 689 1234",
                Email = "contacto@mesoncolonial.do",
                CuisineType = "Cocina Dominicana de Autor & Criolla",
                OpeningTime = "12:00",
                ClosingTime = "23:30",
                ImageUrl = "https://images.unsplash.com/photo-1517248135467-4c7edcad34c4?auto=format&fit=crop&w=1200&q=80",
                Rating = 4.9,
                PriceRange = "$$$",
                TotalCapacity = 42
            };

            var r2 = new Restaurant
            {
                Name = "Boca Marina & Mar Caribe",
                Description = "Especialidades en pescados y mariscos frescos del Mar Caribe. Deck sobre el agua, brisa tropical y cocina marina dominicana de primer nivel.",
                Address = "Avenida España 45, Santo Domingo Este",
                Phone = "+1 809 598 7654",
                Email = "reservas@bocamarina.do",
                CuisineType = "Mariscos & Pescados Caribeños",
                OpeningTime = "12:00",
                ClosingTime = "23:00",
                ImageUrl = "https://images.unsplash.com/photo-1555396273-367ea4eb4db5?auto=format&fit=crop&w=1200&q=80",
                Rating = 4.8,
                PriceRange = "$$$$",
                TotalCapacity = 38
            };

            var r3 = new Restaurant
            {
                Name = "La Yola Grill & Lounge Piantini",
                Description = "Cortes de carne premium al carbón de leña criolla, parrilladas selectas y coctelería contemporánea en la zona más exclusiva del Distrito Nacional.",
                Address = "Avenida Gustavo Mejía Ricart 88, Piantini, Santo Domingo",
                Phone = "+1 829 450 3344",
                Email = "reservas@layolagrill.do",
                CuisineType = "Parrilla, Carnes & Lounge Criollo",
                OpeningTime = "13:00",
                ClosingTime = "00:00",
                ImageUrl = "https://images.unsplash.com/photo-1550966871-3ed3cdb5ed0c?auto=format&fit=crop&w=1200&q=80",
                Rating = 4.7,
                PriceRange = "$$$",
                TotalCapacity = 36
            };

            context.Restaurants.AddRange(r1, r2, r3);
            context.SaveChanges();

            // Mesas para Mesón Colonial Quisqueya
            var tablesR1 = new List<RestaurantTable>
            {
                new() { RestaurantId = r1.Id, TableNumber = 1, Capacity = 2, Location = "Patio Colonial", Status = "Disponible", PosX = 1, PosY = 1 },
                new() { RestaurantId = r1.Id, TableNumber = 2, Capacity = 2, Location = "Patio Colonial", Status = "Reservada", PosX = 2, PosY = 1 },
                new() { RestaurantId = r1.Id, TableNumber = 3, Capacity = 4, Location = "Patio Colonial", Status = "Disponible", PosX = 3, PosY = 1 },
                new() { RestaurantId = r1.Id, TableNumber = 4, Capacity = 4, Location = "Salón Los Frailes", Status = "Ocupada", PosX = 1, PosY = 2 },
                new() { RestaurantId = r1.Id, TableNumber = 5, Capacity = 6, Location = "Salón Los Frailes", Status = "Disponible", PosX = 2, PosY = 2 },
                new() { RestaurantId = r1.Id, TableNumber = 6, Capacity = 4, Location = "Salón Los Frailes", Status = "Disponible", PosX = 3, PosY = 2 },
                new() { RestaurantId = r1.Id, TableNumber = 7, Capacity = 8, Location = "Cava Virreinal VIP", Status = "Disponible", PosX = 1, PosY = 3 },
                new() { RestaurantId = r1.Id, TableNumber = 8, Capacity = 2, Location = "Balcón Histórico", Status = "Reservada", PosX = 2, PosY = 3 }
            };

            // Mesas para Boca Marina & Mar Caribe
            var tablesR2 = new List<RestaurantTable>
            {
                new() { RestaurantId = r2.Id, TableNumber = 1, Capacity = 2, Location = "Deck Sobre el Agua", Status = "Disponible", PosX = 1, PosY = 1 },
                new() { RestaurantId = r2.Id, TableNumber = 2, Capacity = 4, Location = "Deck Sobre el Agua", Status = "Disponible", PosX = 2, PosY = 1 },
                new() { RestaurantId = r2.Id, TableNumber = 3, Capacity = 4, Location = "Terraza Caribeña", Status = "Reservada", PosX = 3, PosY = 1 },
                new() { RestaurantId = r2.Id, TableNumber = 4, Capacity = 6, Location = "Terraza Caribeña", Status = "Disponible", PosX = 1, PosY = 2 },
                new() { RestaurantId = r2.Id, TableNumber = 5, Capacity = 4, Location = "Salón Náutico", Status = "Disponible", PosX = 2, PosY = 2 },
                new() { RestaurantId = r2.Id, TableNumber = 6, Capacity = 8, Location = "Salón Náutico", Status = "Ocupada", PosX = 3, PosY = 2 }
            };

            // Mesas para La Yola Grill & Lounge Piantini
            var tablesR3 = new List<RestaurantTable>
            {
                new() { RestaurantId = r3.Id, TableNumber = 1, Capacity = 2, Location = "Barra Lounge Piantini", Status = "Disponible", PosX = 1, PosY = 1 },
                new() { RestaurantId = r3.Id, TableNumber = 2, Capacity = 2, Location = "Barra Lounge Piantini", Status = "Reservada", PosX = 2, PosY = 1 },
                new() { RestaurantId = r3.Id, TableNumber = 3, Capacity = 4, Location = "Terraza Grill", Status = "Disponible", PosX = 3, PosY = 1 },
                new() { RestaurantId = r3.Id, TableNumber = 4, Capacity = 4, Location = "Terraza Grill", Status = "Disponible", PosX = 1, PosY = 2 },
                new() { RestaurantId = r3.Id, TableNumber = 5, Capacity = 6, Location = "Salón Ejecutivo VIP", Status = "Disponible", PosX = 2, PosY = 2 }
            };

            context.Tables.AddRange(tablesR1);
            context.Tables.AddRange(tablesR2);
            context.Tables.AddRange(tablesR3);
            context.SaveChanges();

            // Platos para Mesón Colonial Quisqueya
            var menuR1 = new List<MenuItem>
            {
                new() { RestaurantId = r1.Id, Name = "Croquetas de Chicharrón de Villa Mella", Description = "Crujientes croquetas artesanales rellenas de auténtico chicharrón criollo y emulsión ligera de mayochimi.", Price = 480.00m, Category = "Entrantes", Tags = "Típico Dominicano", ImageUrl = "https://images.unsplash.com/photo-1544025162-d76694265947?auto=format&fit=crop&w=600&q=80" },
                new() { RestaurantId = r1.Id, Name = "Ceviche Caribeño al Maracuyá", Description = "Dados de filete de dorado fresco marinados en cítricos y chinola, cilantro ancho, cebolla roja y tostones crujientes.", Price = 590.00m, Category = "Entrantes", Tags = "Fresco & Cítrico", ImageUrl = "https://images.unsplash.com/photo-1592417817098-8f3d6910985b?auto=format&fit=crop&w=600&q=80" },
                new() { RestaurantId = r1.Id, Name = "Chivo Liniero al Ron Dominicano", Description = "Chivo tierno macerado con orégano silvestre de Los Quemados, ají gustoso y reducción de ron dorado sobre chenchén sureño.", Price = 980.00m, Category = "Platos Fuertes", Tags = "Especialidad de la Casa", ImageUrl = "https://images.unsplash.com/photo-1558030006-450675393462?auto=format&fit=crop&w=600&q=80" },
                new() { RestaurantId = r1.Id, Name = "Sancocho de Siete Carnes de la Colonia", Description = "Tradicional caldo espeso criollo elaborado con siete carnes selectas, víveres frescos, mazorcas tiernas, arroz blanco y aguacate.", Price = 850.00m, Category = "Platos Fuertes", Tags = "Patrimonio Nacional", ImageUrl = "https://images.unsplash.com/photo-1534080564583-6be75777b70a?auto=format&fit=crop&w=600&q=80" },
                new() { RestaurantId = r1.Id, Name = "Majarete Cremoso con Canela y Coco Tostado", Description = "Postre tradicional de maíz tierno cocinado a fuego lento con leche de coco fresca, azúcar de caña y canela molida.", Price = 350.00m, Category = "Postres", Tags = "Tradición Criolla", ImageUrl = "https://images.unsplash.com/photo-1579372786545-d24232daf58c?auto=format&fit=crop&w=600&q=80" },
                new() { RestaurantId = r1.Id, Name = "Cóctel Quisqueya Libre Signature", Description = "Ron extra añejo dominicano, jarabe especiado de guarapo de caña, amargo de angostura y cítricos caribeños.", Price = 420.00m, Category = "Bebidas", Tags = "Coctelería de Autor", ImageUrl = "https://images.unsplash.com/photo-1514362545857-3bc16c4c7d1b?auto=format&fit=crop&w=600&q=80" }
            };

            // Platos para Boca Marina & Mar Caribe
            var menuR2 = new List<MenuItem>
            {
                new() { RestaurantId = r2.Id, Name = "Empanaditas de Catibía de Cangrejo (4 uds)", Description = "Masa crujiente de yuca rellena de jaiba guisada al caldero con sofrito dominicano y salsa tártara criolla.", Price = 460.00m, Category = "Entrantes", Tags = "Artesanal", ImageUrl = "https://images.unsplash.com/photo-1572695157366-5e585ab2b69f?auto=format&fit=crop&w=600&q=80" },
                new() { RestaurantId = r2.Id, Name = "Pescado Frito Estilo Boca Chica", Description = "Chillo caribeño entero y crujiente al sazón de ajo criollo y limón, servido con tostones verdes dorados y aguacate.", Price = 1150.00m, Category = "Platos Fuertes", Tags = "Favorito Caribeño", ImageUrl = "https://images.unsplash.com/photo-1551183053-bf91a1d81141?auto=format&fit=crop&w=600&q=80" },
                new() { RestaurantId = r2.Id, Name = "Mofongo Relleno de Camarones al Ajillo", Description = "Plátano verde majado en pilón con chicharrón crujiente y ajo dorado, coronado con camarones en salsa criolla.", Price = 920.00m, Category = "Platos Fuertes", Tags = "Clásico del Mar", ImageUrl = "https://images.unsplash.com/photo-1604382354936-07c5d9983bd3?auto=format&fit=crop&w=600&q=80" },
                new() { RestaurantId = r2.Id, Name = "Dulce de Coco Tierno con Piña", Description = "Ralladura de coco tierno cocido lentamente con piña de Cotuí, canela en rama y un toque suave de ron blanco.", Price = 320.00m, Category = "Postres", Tags = "Casero", ImageUrl = "https://images.unsplash.com/photo-1571877227200-a0d98ea607e9?auto=format&fit=crop&w=600&q=80" }
            };

            // Platos para La Yola Grill & Lounge Piantini
            var menuR3 = new List<MenuItem>
            {
                new() { RestaurantId = r3.Id, Name = "Queso Frito Dorado con Mermelada de Guayaba", Description = "Dados de queso de freír dominicano con costra crujiente, servidos con reducción agridulce de guayaba al ron añejo.", Price = 450.00m, Category = "Entrantes", Tags = "Entrante Criollo", ImageUrl = "https://images.unsplash.com/photo-1498654896293-37aacf113fd9?auto=format&fit=crop&w=600&q=80" },
                new() { RestaurantId = r3.Id, Name = "Churrasco Angus a la Leña (12 oz)", Description = "Corte angus certificado cocinado a la brasa con chimichurri de orégano de monte y yuca al mojo de ajo.", Price = 1450.00m, Category = "Platos Fuertes", Tags = "Corte Premium", ImageUrl = "https://images.unsplash.com/photo-1579871494447-9811cf80d66c?auto=format&fit=crop&w=600&q=80" },
                new() { RestaurantId = r3.Id, Name = "Parrillada Mixta Dominicana (Para 2)", Description = "Selección de churrasco, pechuga parrillera, longaniza artesanal del cibao, tostones crujientes y ensalada verde.", Price = 2200.00m, Category = "Platos Fuertes", Tags = "Para Compartir", ImageUrl = "https://images.unsplash.com/photo-1529193591184-b1d58069ecdd?auto=format&fit=crop&w=600&q=80" },
                new() { RestaurantId = r3.Id, Name = "Tres Leches Criollo al Ron Imperial", Description = "Bizcocho tradicional esponjoso embebido en mezcla de tres leches y ron gran reserva dominicano con merengue flameado.", Price = 380.00m, Category = "Postres", Tags = "Especialidad Dulce", ImageUrl = "https://images.unsplash.com/photo-1563805042-7684c019e1cb?auto=format&fit=crop&w=600&q=80" }
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
                CustomerPhone = "+1 809 555 0142",
                ReservationDate = today,
                ReservationTime = "20:30",
                PartySize = 2,
                SpecialRequests = "Mesa en patio colonial para aniversario.",
                Status = "Confirmada",
                AdminNotes = "Asignada mesa con ambientación especial.",
                CreatedAt = today.AddDays(-1).AddHours(14)
            };

            var res2 = new Reservation
            {
                ReservationCode = "RES-1023-8731",
                RestaurantId = r1.Id,
                TableId = tablesR1[3].Id, // Mesa 4
                CustomerName = "Elena Navarro",
                CustomerEmail = "elena.navarro@ejemplo.com",
                CustomerPhone = "+1 829 555 0198",
                ReservationDate = today,
                ReservationTime = "14:00",
                PartySize = 4,
                SpecialRequests = "Uno de los comensales requiere sancocho sin gluten.",
                Status = "Confirmada",
                AdminNotes = "Informado al chef ejecutivo.",
                CreatedAt = today.AddDays(-2).AddHours(10)
            };

            var res3 = new Reservation
            {
                ReservationCode = "RES-1024-5619",
                RestaurantId = r1.Id,
                TableId = tablesR1[4].Id, // Mesa 5
                CustomerName = "Mariano Soto",
                CustomerEmail = "mariano.soto@ejemplo.com",
                CustomerPhone = "+1 809 555 0183",
                ReservationDate = today.AddDays(1),
                ReservationTime = "21:00",
                PartySize = 5,
                SpecialRequests = "Cena de negocios corporativa.",
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
                CustomerPhone = "+1 829 555 0165",
                ReservationDate = today,
                ReservationTime = "21:30",
                PartySize = 4,
                SpecialRequests = "Celebración familiar frente al mar.",
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
                Subject = "Reserva Aprobada en Mesón Colonial Quisqueya — Código: RES-1023-9842",
                BodyHtml = $@"<!DOCTYPE html>
<html lang='es'>
<head><meta charset='utf-8'/><style>body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background-color: #F8FAFC; margin: 0; padding: 24px; color: #0F172A; }} .card {{ max-width: 580px; margin: 0 auto; background: #FFFFFF; border-radius: 12px; overflow: hidden; border: 1px solid #E2E8F0; }} .header {{ background: #1B365D; padding: 28px; text-align: center; color: #FFFFFF; }} .content {{ padding: 28px; }} .code-box {{ text-align: center; border: 2px dashed #CBD5E1; border-radius: 8px; padding: 16px; margin: 20px 0; background: #F8FAFC; }} .code-value {{ font-size: 24px; font-weight: 800; color: #1B365D; letter-spacing: 0.08em; font-family: monospace; }}</style></head>
<body><div class='card'><div class='header'><div style='font-size:12px;text-transform:uppercase;letter-spacing:0.08em;opacity:0.8;'>Mesón Colonial Quisqueya</div><h1 style='font-size:20px;margin:8px 0 0;'>Reserva Confirmada</h1></div><div class='content'><p>Hola, <strong>{res1.CustomerName}</strong></p><p>Tu solicitud de reserva ha sido aprobada con éxito.</p><div class='code-box'><div style='font-size:11px;font-weight:700;color:#64748B;text-transform:uppercase;'>Código Oficial</div><div class='code-value'>RES-1023-9842</div></div><p><strong>Mesa asignada:</strong> Mesa 2 (Patio Colonial) &bull; 2 personas</p><p><strong>Fecha y Hora:</strong> {res1.ReservationDate:dd/MM/yyyy} a las {res1.ReservationTime} hrs</p></div></div></body></html>",
                Status = "Enviado",
                SentAt = DateTime.UtcNow.AddMinutes(-30)
            });

            context.SaveChanges();
        }
    }
}
