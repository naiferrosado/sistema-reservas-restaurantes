# Sistema de Gestion de Reservas para Restaurantes

Plataforma web desarrollada en **ASP.NET Core MVC** con **Entity Framework Core** y **PostgreSQL**, disenada bajo una interfaz corporativa limpia de fondo blanco, sin emojis y con iconografia vectorial.

---

## Tecnologias Principales
- **Backend:** ASP.NET Core (.NET 10 / C#) con patron MVC y Web API
- **Vistas:** Razor Views (.cshtml) fuertemente tipadas con ViewModels
- **Base de Datos:** PostgreSQL con Entity Framework Core (`Npgsql`)
- **Seguridad:** Autenticacion por cookies para administradores con `[Authorize]`
- **Documentacion API:** Swagger UI interactivo en `/swagger`

---

## Requisitos Previos
- [.NET SDK](https://dotnet.microsoft.com/download) (.NET 10 o .NET 8 / 9)
- [PostgreSQL Server](https://www.postgresql.org/download/) (v14 o superior) en el puerto `5432`

---

## Instalacion y Puesta en Marcha

### 1. Clonar el repositorio
```bash
git clone https://github.com/naiferrosado/sistema-reservas-restaurantes.git
cd sistema-reservas-restaurantes
```

### 2. Configurar la base de datos
Crea la base de datos `restaurantereservas` en tu servidor PostgreSQL:
```bash
psql -U postgres -c "CREATE DATABASE restaurantereservas;"
```

Copia la plantilla de configuracion y asigna tu contrasena en `appsettings.json`:
```bash
# Windows PowerShell
Copy-Item appsettings.example.json appsettings.json

# Linux / macOS
cp appsettings.example.json appsettings.json
```

Edita la cadena de conexion en `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=restaurantereservas;Username=postgres;Password=TU_CONTRASENA"
}
```

### 3. Compilar y Ejecutar
```bash
dotnet build
dotnet run
```

Al iniciar por primera vez, el sistema creara las tablas y sembrara automaticamente los restaurantes, cartas, mesas y el usuario administrador inicial.

---

## Enlaces y Uso del Sistema

- **Portada y Catalogo:** [http://localhost:5043](http://localhost:5043)
- **Consultar Reserva:** [http://localhost:5043/Reservations/Lookup](http://localhost:5043/Reservations/Lookup)
- **Panel Administrativo (Requiere Login):** [http://localhost:5043/Dashboard/Index](http://localhost:5043/Dashboard/Index)
- **Documentacion Swagger API:** [http://localhost:5043/swagger](http://localhost:5043/swagger)

### Acceso Administrativo
- **Usuario:** `admin@sistemareservas.com`
- **Contrasena:** `Admin123!`

---

## Funcionalidades Clave
1. **Catalogo Gastronomico:** Ficha informativa y cartas filtrables para *La Terraza Gourmet*, *Trattoria Don Corleone* y *Sakura Izakaya*.
2. **Motor de Reservas:** Asignacion inteligente de mesa, prevencion de sobreventa y generacion de clave unica oficial `RES-XXXX-XXXX`.
3. **Dashboard con KPIs en Vivo:** Conteo en tiempo real de reservas del dia, comensales, porcentaje de ocupacion y pendientes.
4. **Plano de Mesas en Sala:** Cambio operativo de estado (*Disponible*, *Reservada*, *Ocupada*) en menos de 50 ms.
5. **Gestion de Solicitudes:** Aprobacion con envio de correo corporativo y rechazo con motivo obligatorio.
6. **Bandeja de Auditoria:** Previsualizacion HTML en modal de todos los correos generados.
