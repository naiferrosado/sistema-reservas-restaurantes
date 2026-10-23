# Sistema de Gestion de Reservas para Restaurantes

Plataforma web integral desarrollada bajo el patron arquitectonico **ASP.NET Core MVC** con **Entity Framework Core** y **PostgreSQL**. Incorpora un sistema de diseno corporativo de fondo blanco, sin emojis y con iconografia vectorial, orientado a la automatizacion del proceso de reservas gastronomicas.

---

## Tabla de Contenidos
1. [Requisitos de Software](#requisitos-de-software)
2. [Extensiones Recomendadas para Visual Studio Code](#extensiones-recomendadas-para-visual-studio-code)
3. [Instalacion y Puesta en Marcha](#instalacion-y-puesta-en-marcha)
4. [Enlaces del Sistema y Credenciales](#enlaces-del-sistema-y-credenciales)
5. [Guia de Prueba y Demostracion](#guia-de-prueba-y-demostracion)
6. [Resumen Tecnico y Arquitectura](#resumen-tecnico-y-arquitectura)

---

## Requisitos de Software

Antes de ejecutar el proyecto, asegurate de tener instaladas las siguientes herramientas:

| Herramienta | Version Minima | Proposito |
| :--- | :--- | :--- |
| **.NET SDK** | .NET 10 (o .NET 8 / 9) | Compilador y entorno de ejecucion de ASP.NET Core |
| **PostgreSQL** | v14 o superior | Motor de base de datos relacional (puerto `5432`) |
| **Git** | v2.x | Control de versiones para clonar el repositorio |

---

## Extensiones Recomendadas para Visual Studio Code

Si trabajas con **Visual Studio Code**, se recomienda instalar las siguientes extensiones:

| Extension | Identificador | Proposito |
| :--- | :--- | :--- |
| **C# Dev Kit** | `ms-dotnettools.csdevkit` | Explorador de proyectos, navegacion y depuracion |
| **C#** | `ms-dotnettools.csharp` | Motor de lenguaje C#, IntelliSense y resaltado sintactico |
| **PostgreSQL** | `ckolkman.vscode-postgres` | Conexion y consultas SQL a PostgreSQL desde el editor |
| **REST Client** | `humao.rest-client` | Pruebas de endpoints HTTP en `SistemaReservas.http` |

---

## Instalacion y Puesta en Marcha

### Paso 1: Clonar el Repositorio
```bash
git clone https://github.com/naiferrosado/sistema-reservas-restaurantes.git
cd sistema-reservas-restaurantes
```

### Paso 2: Crear la Base de Datos en PostgreSQL
Crea la base de datos llamada **`restaurantereservas`** en tu servidor local:

- **Desde consola `psql`:**
  ```bash
  psql -U postgres -c "CREATE DATABASE restaurantereservas;"
  ```
- **Desde pgAdmin 4 o DBeaver:**
  Crear una base de datos con el nombre `restaurantereservas`.

### Paso 3: Configurar Credenciales (`appsettings.json`)
Copia la plantilla `appsettings.example.json`:

```bash
# Windows PowerShell
Copy-Item appsettings.example.json appsettings.json

# Linux / macOS
cp appsettings.example.json appsettings.json
```

Abre `appsettings.json` y coloca tu contrasena de PostgreSQL en `DefaultConnection`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=restaurantereservas;Username=postgres;Password=TU_CONTRASENA"
}
```

### Paso 4: Compilar y Ejecutar
```bash
dotnet build
dotnet run
```

Al iniciar por primera vez, Entity Framework Core creara automaticamente las tablas y sembrara los restaurantes, cartas, mesas y el usuario administrador.

---

## Enlaces del Sistema y Credenciales

Una vez iniciado el servidor, abre tu navegador en:

| Recurso | URL | Acceso |
| :--- | :--- | :--- |
| **Portada y Catalogo Gastronomico** | [http://localhost:5043](http://localhost:5043) | Publico |
| **Consultar Estado de Reserva** | [http://localhost:5043/Reservations/Lookup](http://localhost:5043/Reservations/Lookup) | Publico |
| **Hacer una Reserva en Linea** | [http://localhost:5043/Reservations/Create](http://localhost:5043/Reservations/Create) | Publico |
| **Panel Administrativo** | [http://localhost:5043/Dashboard/Index](http://localhost:5043/Dashboard/Index) | Requiere Login |
| **Documentacion Swagger API** | [http://localhost:5043/swagger](http://localhost:5043/swagger) | Publico |

### Credenciales del Administrador
- **Correo:** `admin@sistemareservas.com`
- **Contrasena:** `Admin123!`

---

## Guia de Prueba y Demostracion

Para verificar todas las funcionalidades implementadas en la plataforma:

1. **Explorar Restaurantes y Menus (`/Home/Index`):**
   - Alterna entre *La Terraza Gourmet*, *Trattoria Don Corleone* y *Sakura Izakaya*.
   - Revisa la ficha con foto, direccion, telefono, horario y valoracion numerica.
   - Consulta las cartas gastronomicas categorizadas y la distribucion de mesas por zonas.
2. **Generar una Reserva (`/Reservations/Create`):**
   - Haz clic en **Reservar Mesa**.
   - Completa el formulario seleccionando fecha, hora, comensales y mesa opcional.
   - El sistema valida disponibilidad, previene sobreventa y emite el comprobante con la clave oficial `RES-XXXX-XXXX` (estado inicial *Pendiente*).
3. **Consultar Estado de Reserva (`/Reservations/Lookup`):**
   - Ingresa el codigo de reserva o tu correo electronico para visualizar el estado y detalle de la solicitud.
4. **Acceso al Panel Administrativo (`/Account/Login`):**
   - Al intentar entrar al panel administrativo sin sesion, el sistema redirige al login.
   - Ingresa con las credenciales de administrador (`admin@sistemareservas.com` / `Admin123!`).
5. **Gestion de Reservas y KPIs en Vivo (`/Dashboard/Index`):**
   - Observa las 4 tarjetas KPI (Reservas hoy, comensales, ocupacion en sala % y pendientes).
   - En la tabla de reservas, aprueba una solicitud para disparar la confirmacion por correo o rechazala indicando un motivo.
6. **Plano de Mesas en Sala (`/Dashboard/Index?tab=floorplan`):**
   - Haz clic en cualquier mesa para alternar su estado operativo en vivo (*Disponible* -> *Reservada* -> *Ocupada* -> *Disponible*) en menos de 50 ms.
7. **Bandeja de Auditoria de Correos (`/Dashboard/Index?tab=inbox`):**
   - Haz clic sobre un correo registrado para abrir el modal de previsualizacion HTML con el diseno oficial enviado al cliente.

---

## Resumen Tecnico y Arquitectura

- **Patron Arquitectonico:** ASP.NET Core MVC con enrutamiento convencional `{controller=Home}/{action=Index}/{id?}` y Web API dual.
- **Modelos y Vistas:** Vistas Razor (`Views/`) modulares con Tag Helpers, asociadas a ViewModels fuertemente tipados (`ViewModels/`).
- **Persistencia:** Entity Framework Core con proveedor `Npgsql.EntityFrameworkCore.PostgreSQL`.
- **Seguridad:** Control de acceso con `[Authorize]` y autenticacion por cookies (`CookieAuthenticationDefaults`).
- **Diseno:** Vanilla CSS en [styles.css](file:///c:/Users/Naifer/Documents/sistema-reservas-restaurantes/wwwroot/css/styles.css) con paleta corporativa (`#1B365D`, `#C85A32`, fondo blanco puro) y cero emojis.
- **Servicio de Notificaciones:** `EmailService` con generacion de plantillas HTML y respaldo auditable en `NotificationLog`.
