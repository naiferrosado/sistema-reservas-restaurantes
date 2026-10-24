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

            // Si la base de datos ya tenía datos anteriores (no dominicanos), reinicializar con la ambientación de República Dominicana
            var hasOldData = context.Restaurants.Any(r => r.Name == "La Terraza Gourmet" || r.Phone.StartsWith("+34"));
            if (hasOldData)
            {
                context.Notifications.RemoveRange(context.Notifications);
                context.Reservations.RemoveRange(context.Reservations);
                context.MenuItems.RemoveRange(context.MenuItems);
                context.Tables.RemoveRange(context.Tables);
                context.Restaurants.RemoveRange(context.Restaurants);
                context.SaveChanges();
            }
            else if (context.Restaurants.Any())
            {
                return; // Ya fue inicializada con los datos actuales
            }

            // ==========================================
            // RESTAURANTES EN REPÚBLICA DOMINICANA
            // ==========================================
            var r1 = new Restaurant
            {
                Name = "Mesón Colonial Quisqueya",
                Description = "Alta cocina dominicana de autor en el corazón de la Ciudad Colonial. Rescatamos recetas tradicionales con ingredientes de Samaná, Constanza y Montecristi, acompañadas del mejor maridaje con rones prémium dominicanos.",
                Address = "Calle Las Damas 42, Ciudad Colonial, Santo Domingo, R.D.",
                Phone = "+1 (809) 688-4521",
                Email = "reservas@mesoncolonial.do",
                CuisineType = "Alta Cocina Dominicana & Fusión Criolla",
                OpeningTime = "12:00",
                ClosingTime = "23:30",
                ImageUrl = "https://images.unsplash.com/photo-1517248135467-4c7edcad34c4?auto=format&fit=crop&w=1200&q=80",
                Rating = 4.9,
                PriceRange = "$$$",
                TotalCapacity = 44
            };

            var r2 = new Restaurant
            {
                Name = "Boca Marina & Mar Caribe",
                Description = "Mariscada caribeña frente al mar. Pescados frescos del día traídos por pescadores artesanales de las costas quisqueyanas, langostas a la brasa y mofongos de mariscos para disfrutar el atardecer caribeño.",
                Address = "Av. George Washington 312, Malecón, Santo Domingo, R.D.",
                Phone = "+1 (809) 533-8890",
                Email = "contacto@bocamarina.do",
                CuisineType = "Pescados del Caribe & Mariscos Criollos",
                OpeningTime = "12:30",
                ClosingTime = "23:00",
                ImageUrl = "https://images.unsplash.com/photo-1544025162-d76694265947?auto=format&fit=crop&w=1200&q=80",
                Rating = 4.8,
                PriceRange = "$$$$",
                TotalCapacity = 40
            };

            var r3 = new Restaurant
            {
                Name = "La Yola Grill & Lounge Piantini",
                Description = "Espacio gastronómico de vanguardia en el polígono central de Piantini. Cortes prémium a la leña de carbón de marabú, chicharrones glaseados con chinola y mixología de autor con rones envejecidos dominicanos.",
                Address = "Av. Gustavo Mejía Ricart esq. Federico Geraldino, Piantini, Santo Domingo, R.D.",
                Phone = "+1 (809) 567-3344",
                Email = "reservas@layolapiantini.do",
                CuisineType = "Parrilla a la Leña & Coctelería de Autor",
                OpeningTime = "13:00",
                ClosingTime = "00:00",
                ImageUrl = "https://images.unsplash.com/photo-1555396273-367ea4eb4db5?auto=format&fit=crop&w=1200&q=80",
                Rating = 4.9,
                PriceRange = "$$$$",
                TotalCapacity = 38
            };

            context.Restaurants.AddRange(r1, r2, r3);
            context.SaveChanges();

            // Mesas para Mesón Colonial Quisqueya
            var tablesR1 = new List<RestaurantTable>
            {
                new() { RestaurantId = r1.Id, TableNumber = 1, Capacity = 2, Location = "Terraza Colonial", Status = "Disponible", PosX = 1, PosY = 1 },
                new() { RestaurantId = r1.Id, TableNumber = 2, Capacity = 2, Location = "Terraza Colonial", Status = "Reservada", PosX = 2, PosY = 1 },
                new() { RestaurantId = r1.Id, TableNumber = 3, Capacity = 4, Location = "Terraza Colonial", Status = "Disponible", PosX = 3, PosY = 1 },
                new() { RestaurantId = r1.Id, TableNumber = 4, Capacity = 4, Location = "Salón Virreinal", Status = "Ocupada", PosX = 1, PosY = 2 },
                new() { RestaurantId = r1.Id, TableNumber = 5, Capacity = 6, Location = "Salón Virreinal", Status = "Disponible", PosX = 2, PosY = 2 },
                new() { RestaurantId = r1.Id, TableNumber = 6, Capacity = 4, Location = "Patio Español", Status = "Disponible", PosX = 3, PosY = 2 },
                new() { RestaurantId = r1.Id, TableNumber = 7, Capacity = 8, Location = "Zona VIP Don Diego", Status = "Disponible", PosX = 1, PosY = 3 },
                new() { RestaurantId = r1.Id, TableNumber = 8, Capacity = 2, Location = "Balcón Histórico", Status = "Reservada", PosX = 2, PosY = 3 }
            };

            // Mesas para Boca Marina & Mar Caribe
            var tablesR2 = new List<RestaurantTable>
            {
                new() { RestaurantId = r2.Id, TableNumber = 1, Capacity = 2, Location = "Deck Sobre el Mar", Status = "Disponible", PosX = 1, PosY = 1 },
                new() { RestaurantId = r2.Id, TableNumber = 2, Capacity = 4, Location = "Deck Sobre el Mar", Status = "Disponible", PosX = 2, PosY = 1 },
                new() { RestaurantId = r2.Id, TableNumber = 3, Capacity = 4, Location = "Gazebo Brisa Caribeña", Status = "Reservada", PosX = 3, PosY = 1 },
                new() { RestaurantId = r2.Id, TableNumber = 4, Capacity = 6, Location = "Gazebo Brisa Caribeña", Status = "Disponible", PosX = 1, PosY = 2 },
                new() { RestaurantId = r2.Id, TableNumber = 5, Capacity = 4, Location = "Salón Panorámico", Status = "Disponible", PosX = 2, PosY = 2 },
                new() { RestaurantId = r2.Id, TableNumber = 6, Capacity = 8, Location = "Salón Panorámico", Status = "Ocupada", PosX = 3, PosY = 2 }
            };

            // Mesas para La Yola Grill Piantini
            var tablesR3 = new List<RestaurantTable>
            {
                new() { RestaurantId = r3.Id, TableNumber = 1, Capacity = 2, Location = "Barra de Ron y Cacao", Status = "Disponible", PosX = 1, PosY = 1 },
                new() { RestaurantId = r3.Id, TableNumber = 2, Capacity = 2, Location = "Barra de Ron y Cacao", Status = "Reservada", PosX = 2, PosY = 1 },
                new() { RestaurantId = r3.Id, TableNumber = 3, Capacity = 4, Location = "Terraza Tropical Lounge", Status = "Disponible", PosX = 3, PosY = 1 },
                new() { RestaurantId = r3.Id, TableNumber = 4, Capacity = 4, Location = "Salón Principal Piantini", Status = "Disponible", PosX = 1, PosY = 2 },
                new() { RestaurantId = r3.Id, TableNumber = 5, Capacity = 6, Location = "Salón Principal Piantini", Status = "Disponible", PosX = 2, PosY = 2 }
            };

            context.Tables.AddRange(tablesR1);
            context.Tables.AddRange(tablesR2);
            context.Tables.AddRange(tablesR3);
            context.SaveChanges();

            // Platos para Mesón Colonial Quisqueya (Precios en RD$)
            var menuR1 = new List<MenuItem>
            {
                new() { RestaurantId = r1.Id, Name = "Catibias Crujientes de Chivo Liniero", Description = "Empanadas artesanales de harina de yuca rellenas de chivo desmenuzado en reducción de cerveza criolla y naranja agria.", Price = 620.00m, Category = "Entrantes", Tags = "Especialidad Criolla, Sin Gluten", ImageUrl = "https://images.unsplash.com/photo-1544025162-d76694265947?auto=format&fit=crop&w=600&q=80" },
                new() { RestaurantId = r1.Id, Name = "Croquetas de Plátano Maduro y Queso de Hoja", Description = "Bolitas doradas de plátano maduro con corazón fundente de queso de hoja dominicano y crema de aguacate criollo.", Price = 490.00m, Category = "Entrantes", Tags = "Vegetariano, Receta Típica", ImageUrl = "https://images.unsplash.com/photo-1592417817098-8f3d6910985b?auto=format&fit=crop&w=600&q=80" },
                new() { RestaurantId = r1.Id, Name = "Chivo Liniero al Ron con Chenchén Cremoso", Description = "Chivo estofado a fuego lento con especias criollas y ron dominicano extra viejo, servido sobre chenchén del sur mantecado con queso.", Price = 1480.00m, Category = "Platos Fuertes", Tags = "Plato Insignia, Del Sur", ImageUrl = "https://images.unsplash.com/photo-1558030006-450675393462?auto=format&fit=crop&w=600&q=80" },
                new() { RestaurantId = r1.Id, Name = "La Bandera Dominicana de Autor", Description = "Costilla de res Angus horneada por 12 horas en su jugo, arroz blanco jazmín, habichuelas negras sedosas y tostones de plátano barahonero.", Price = 1250.00m, Category = "Platos Fuertes", Tags = "Clásico Dominicano", ImageUrl = "https://images.unsplash.com/photo-1534080564583-6be75777b70a?auto=format&fit=crop&w=600&q=80" },
                new() { RestaurantId = r1.Id, Name = "Majarete Brûlée con Especias Dulces", Description = "Crema tradicional de maíz tierno caramelizada al soplete con canela de Ceilán y teja crujiente de coco tostado de Samaná.", Price = 450.00m, Category = "Postres", Tags = "Postre Dominicano", ImageUrl = "https://images.unsplash.com/photo-1579372786545-d24232daf58c?auto=format&fit=crop&w=600&q=80" },
                new() { RestaurantId = r1.Id, Name = "Cóctel Quisqueya Libre Reserva", Description = "Ron dominicano imperial, infusión de chinola fresca de Monte Plata, limón agrio y ginger beer artesanal con hojas de menta.", Price = 580.00m, Category = "Bebidas", Tags = "Mixología Dominicana", ImageUrl = "https://images.unsplash.com/photo-1514362545857-3bc16c4c7d1b?auto=format&fit=crop&w=600&q=80" }
            };

            // Platos para Boca Marina & Mar Caribe
            var menuR2 = new List<MenuItem>
            {
                new() { RestaurantId = r2.Id, Name = "Chicharrón de Calamar con Alioli de Cilantrico", Description = "Calamares tiernos rebozados con chips de yuca frita y emulsión de ajo asado con recao verde dominicano.", Price = 740.00m, Category = "Entrantes", Tags = "Mariscos Frescos", ImageUrl = "https://images.unsplash.com/photo-1572695157366-5e585ab2b69f?auto=format&fit=crop&w=600&q=80" },
                new() { RestaurantId = r2.Id, Name = "Ceviche Caribeño de Mero en Chinola", Description = "Cortes de mero fresco de Samaná marinados en maracuyá y lima con cebolla morada y crujiente de plátano verde.", Price = 820.00m, Category = "Entrantes", Tags = "Pescado del Día, Sin Gluten", ImageUrl = "https://images.unsplash.com/photo-1544025162-d76694265947?auto=format&fit=crop&w=600&q=80" },
                new() { RestaurantId = r2.Id, Name = "Pescado al Coco Estilo Samaná", Description = "Filete de mero fresco sellado a la plancha bañado en salsa de leche de coco fresca con ají gustoso y moro de guandules con coco.", Price = 1650.00m, Category = "Platos Fuertes", Tags = "Tradición de Samaná", ImageUrl = "https://images.unsplash.com/photo-1551183053-bf91a1d81141?auto=format&fit=crop&w=600&q=80" },
                new() { RestaurantId = r2.Id, Name = "Mofongo Relleno de Camarones al Ajillo", Description = "Plátano verde frito majado en pilón tradicional con chicharrón crocante y camarones en salsa criolla de ajo y pimientos.", Price = 1380.00m, Category = "Platos Fuertes", Tags = "Favorito Caribeño", ImageUrl = "https://images.unsplash.com/photo-1604382354936-07c5d9983bd3?auto=format&fit=crop&w=600&q=80" },
                new() { RestaurantId = r2.Id, Name = "Tres Leches al Ron Brugal Leyenda", Description = "Bizcochuelo impregnado en mezcla cremosa de tres leches y ron dominicano añejo, coronado con merengue flameado.", Price = 520.00m, Category = "Postres", Tags = "Dulce Típico", ImageUrl = "https://images.unsplash.com/photo-1571877227200-a0d98ea607e9?auto=format&fit=crop&w=600&q=80" },
                new() { RestaurantId = r2.Id, Name = "Cerveza Presidente Vestida de Novia", Description = "La emblemática cerveza dominicana servida al punto de congelación en copa esmerilada con toque de limón criollo.", Price = 320.00m, Category = "Bebidas", Tags = "Clásica Dominicana", ImageUrl = "https://images.unsplash.com/photo-1514362545857-3bc16c4c7d1b?auto=format&fit=crop&w=600&q=80" }
            };

            // Platos para La Yola Grill Piantini
            var menuR3 = new List<MenuItem>
            {
                new() { RestaurantId = r3.Id, Name = "Chicharrón Glaseado en Reducción de Chinola", Description = "Panceta de cerdo crujiente glaseada con reducción agridulce de maracuyá y un toque sutil de ají caribeño.", Price = 890.00m, Category = "Entrantes", Tags = "Favorito de la Casa", ImageUrl = "https://images.unsplash.com/photo-1498654896293-37aacf113fd9?auto=format&fit=crop&w=600&q=80" },
                new() { RestaurantId = r3.Id, Name = "Carpaccio de Res con Queso Geo de Imbert", Description = "Finas láminas de lomo fino marinado en aceite de oliva, alcaparritas baby y queso curado artesanal de Imbert (Puerto Plata).", Price = 780.00m, Category = "Entrantes", Tags = "Ingredientes Locales", ImageUrl = "https://images.unsplash.com/photo-1579871494447-9811cf80d66c?auto=format&fit=crop&w=600&q=80" },
                new() { RestaurantId = r3.Id, Name = "Churrasco Prime con Chimichurri Criollo y Yuca al Mojo", Description = "Jugoso corte a la brasa de leña con yuca mocana tierna bañada en mojo de cebollas moradas salteadas.", Price = 1950.00m, Category = "Platos Fuertes", Tags = "Corte Prémium", ImageUrl = "https://images.unsplash.com/photo-1529193591184-b1d58069ecdd?auto=format&fit=crop&w=600&q=80" },
                new() { RestaurantId = r3.Id, Name = "Costillitas BBQ de Guayaba Criolla y Ron", Description = "Costillas de cerdo tiernas glaseadas con barbacoa artesanal de mermelada de guayaba dominicana.", Price = 1420.00m, Category = "Platos Fuertes", Tags = "A la Brasa", ImageUrl = "https://images.unsplash.com/photo-1558030006-450675393462?auto=format&fit=crop&w=600&q=80" },
                new() { RestaurantId = r3.Id, Name = "Fondant de Chocolate Orgánico de San Francisco", Description = "Pastel tibio de cacao dominicano al 70% con centro fundente y helado artesanal de coco de agua.", Price = 560.00m, Category = "Postres", Tags = "Cacao Dominicano", ImageUrl = "https://images.unsplash.com/photo-1563805042-7684c019e1cb?auto=format&fit=crop&w=600&q=80" },
                new() { RestaurantId = r3.Id, Name = "Santo Domingo Sour", Description = "Ron dominicano añejo, zumo de naranja agria, clara espumosa y gotas de amargo de angostura criollo.", Price = 590.00m, Category = "Bebidas", Tags = "Coctelería de Autor", ImageUrl = "https://images.unsplash.com/photo-1514362545857-3bc16c4c7d1b?auto=format&fit=crop&w=600&q=80" }
            };

            context.MenuItems.AddRange(menuR1);
            context.MenuItems.AddRange(menuR2);
            context.MenuItems.AddRange(menuR3);
            context.SaveChanges();

            // Sembrar reservas iniciales representativas en R.D.
            var today = DateTime.UtcNow.Date;
            var res1 = new Reservation
            {
                ReservationCode = "RES-DO-9842",
                RestaurantId = r1.Id,
                TableId = tablesR1[1].Id, // Mesa 2
                CustomerName = "Carlos Rosario Minaya",
                CustomerEmail = "carlos.rosario@ejemplo.do",
                CustomerPhone = "+1 (809) 710-2345",
                ReservationDate = today,
                ReservationTime = "20:30",
                PartySize = 2,
                SpecialRequests = "Mesa en la terraza colonial para celebración de aniversario de bodas.",
                Status = "Confirmada",
                AdminNotes = "Asignada mesa con vista a Calle Las Damas.",
                CreatedAt = today.AddDays(-1).AddHours(14)
            };

            var res2 = new Reservation
            {
                ReservationCode = "RES-DO-8731",
                RestaurantId = r1.Id,
                TableId = tablesR1[3].Id, // Mesa 4
                CustomerName = "Elena Peña Almonte",
                CustomerEmail = "elena.pena@ejemplo.do",
                CustomerPhone = "+1 (829) 450-8912",
                ReservationDate = today,
                ReservationTime = "13:30",
                PartySize = 4,
                SpecialRequests = "Uno de los comensales tiene alergia a los mariscos.",
                Status = "Confirmada",
                AdminNotes = "Notificado al chef principal.",
                CreatedAt = today.AddDays(-2).AddHours(10)
            };

            var res3 = new Reservation
            {
                ReservationCode = "RES-DO-5619",
                RestaurantId = r1.Id,
                TableId = tablesR1[4].Id, // Mesa 5
                CustomerName = "Mariano Gómez Tavárez",
                CustomerEmail = "mariano.gomez@ejemplo.do",
                CustomerPhone = "+1 (849) 330-4567",
                ReservationDate = today.AddDays(1),
                ReservationTime = "21:00",
                PartySize = 5,
                SpecialRequests = "Cena de bienvenida para socios comerciales.",
                Status = "Pendiente",
                CreatedAt = today.AddHours(2)
            };

            var res4 = new Reservation
            {
                ReservationCode = "RES-DO-3310",
                RestaurantId = r2.Id,
                TableId = tablesR2[2].Id,
                CustomerName = "Sofía Valenzuela Cabral",
                CustomerEmail = "sofia.valenzuela@ejemplo.do",
                CustomerPhone = "+1 (809) 580-9988",
                ReservationDate = today,
                ReservationTime = "19:30",
                PartySize = 4,
                SpecialRequests = "Celebración familiar en el deck con vista al mar.",
                Status = "Confirmada",
                CreatedAt = today.AddDays(-1)
            };

            context.Reservations.AddRange(res1, res2, res3, res4);
            context.SaveChanges();

            // Notificación de prueba con diseño corporativo dominicano
            context.Notifications.Add(new NotificationLog
            {
                ReservationId = res1.Id,
                RecipientEmail = res1.CustomerEmail,
                RecipientName = res1.CustomerName,
                Subject = "Reserva Aprobada en Mesón Colonial Quisqueya — Código: RES-DO-9842",
                BodyHtml = $@"<!DOCTYPE html>
<html lang='es'>
<head><meta charset='utf-8'/><style>body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background-color: #F8FAFC; margin: 0; padding: 24px; color: #0F172A; }} .card {{ max-width: 580px; margin: 0 auto; background: #FFFFFF; border-radius: 12px; overflow: hidden; border: 1px solid #E2E8F0; }} .header {{ background: #1B365D; padding: 28px; text-align: center; color: #FFFFFF; }} .content {{ padding: 28px; }} .code-box {{ text-align: center; border: 2px dashed #CBD5E1; border-radius: 8px; padding: 16px; margin: 20px 0; background: #F8FAFC; }} .code-value {{ font-size: 24px; font-weight: 800; color: #1B365D; letter-spacing: 0.08em; font-family: monospace; }}</style></head>
<body><div class='card'><div class='header'><div style='font-size:12px;text-transform:uppercase;letter-spacing:0.08em;opacity:0.8;'>Mesón Colonial Quisqueya</div><h1 style='font-size:20px;margin:8px 0 0;'>Reserva Confirmada</h1></div><div class='content'><p>Hola, <strong>{res1.CustomerName}</strong></p><p>Tu solicitud de reserva ha sido aprobada con éxito.</p><div class='code-box'><div style='font-size:11px;font-weight:700;color:#64748B;text-transform:uppercase;'>Código Oficial</div><div class='code-value'>RES-DO-9842</div></div><p><strong>Mesa asignada:</strong> Mesa 2 (Terraza Colonial) &bull; 2 comensales</p><p><strong>Fecha y Hora:</strong> {res1.ReservationDate:dd/MM/yyyy} a las {res1.ReservationTime} hrs</p><p><strong>Ubicación:</strong> Calle Las Damas 42, Ciudad Colonial, Santo Domingo, R.D.</p></div></div></body></html>",
                Status = "Enviado",
                SentAt = DateTime.UtcNow.AddMinutes(-30)
            });

            context.SaveChanges();
        }
    }
}
