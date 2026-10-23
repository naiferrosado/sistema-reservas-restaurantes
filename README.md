# Sistema de Gestion de Reservas para Restaurantes

> **Asignatura:** Automatizacion de Procesos  
> **Trabajo Practico:** Proyecto Tecnologico — Sistema de Reservas para Restaurantes  
> **Arquitectura:** ASP.NET Core MVC (Modelo-Vista-Controlador) con Razor Views y Web API  
> **Base de Datos:** PostgreSQL con Entity Framework Core  
> **Sistema de Diseno:** Corporativo Ejecutivo (Fondo Blanco, Sin Emojis, Iconografia Vectorial SVG)  

---

## Tabla de Contenidos
1. [Descripcion del Proyecto](#descripcion-del-proyecto)
2. [Requisitos Previos (Software a Instalar)](#requisitos-previos-software-a-instalar)
3. [Extensiones Recomendadas para Visual Studio Code](#extensiones-recomendadas-para-visual-studio-code)
4. [Guia Paso a Paso de Instalacion y Ejecucion](#guia-paso-a-paso-de-instalacion-y-ejecucion)
5. [Cumplimiento de Objetivos Tecnicos y Funcionales (OE-01 a OE-05)](#cumplimiento-de-objetivos-tecnicos-y-funcionales)
6. [Guia de Prueba y Demostración](#guia-de-prueba-y-demostracion)
7. [Arquitectura y Tecnologias](#arquitectura-y-tecnologias)
8. [Estructura del Proyecto](#estructura-del-proyecto)
9. [Seguridad y Variables de Entorno](#seguridad-y-variables-de-entorno)

---

## Descripcion del Proyecto
Plataforma web integral bajo la arquitectura **ASP.NET Core MVC (Modelo-Vista-Controlador)** con **Razor Views** fuertemente tipadas y soporte dual para endpoints API/Swagger para la automatizacion del proceso de reservas en restaurantes.

Permite a los comensales explorar la oferta gastronomica de los restaurantes seleccionados (*La Terraza Gourmet*, *Trattoria Don Corleone* y *Sakura Izakaya*), verificar la distribucion de mesas por zonas, reservar en linea con prevencion de sobreventa y consultar en tiempo real el estado de su solicitud mediante un codigo oficial unico (`RES-XXXX-XXXX`).

El panel administrativo permite gestionar reservas en vivo, controlar operativamente el plano de mesas en sala (Disponible / Reservada / Ocupada) en menos de 500 ms, calcular KPIs en tiempo real e inspeccionar la bandeja de auditoria de correos HTML enviados.

Toda la interfaz aplica un **sistema de diseno corporativo de fondo blanco puro**, barra de navegacion fija de 68 px centrada en 1280 px, botones con jerarquia estricta, paleta ejecutiva (`#1B365D` azul marino, `#C85A32` terracota, `#F8FAFC` pizarra claro) y **cero emojis** en toda la plataforma, sustituidos exclusivamente por iconos vectoriales SVG limpios e indicadores geometricos.

---

## Requisitos Previos (Software a Instalar)

Antes de ejecutar el proyecto, asegurate de tener instaladas las siguientes herramientas:

### 1. [.NET SDK](https://dotnet.microsoft.com/download) (.NET 10 o .NET 8 / 9)
- **Descripcion:** Entorno de ejecucion y compilador oficial de C# / ASP.NET Core.
- **Verificacion en consola:**
  ```bash
  dotnet --version
  ```
  *(Debe mostrar una version numerica como `10.0.x`, `9.0.x` o `8.0.x`)*.

### 2. [PostgreSQL Server](https://www.postgresql.org/download/) (Version 14 o superior)
- **Descripcion:** Sistema de gestion de bases de datos relacionales SQL.
- **Herramienta grafica recomendada:** [pgAdmin 4](https://www.pgadmin.org/download/) o DBeaver.
- **Verificacion en consola:**
  ```bash
  psql --version
  ```
  *(Asegurate de que el servicio de PostgreSQL este iniciado en el puerto `5432`)*.

### 3. [Git](https://git-scm.com/)
- **Descripcion:** Sistema de control de versiones.
- **Verificacion en consola:**
  ```bash
  git --version
  ```

### 4. Navegador Web Moderno
- Google Chrome, Microsoft Edge, Mozilla Firefox o Brave.

---

## Extensiones Recomendadas para Visual Studio Code

Si abres el proyecto con **Visual Studio Code**, se sugiere instalar las siguientes extensiones:

| Extension | Identificador / Autor | Utilidad |
| :--- | :--- | :--- |
| **C# Dev Kit** | `ms-dotnettools.csdevkit` (Microsoft) | Soporte oficial completo para C#, explorador de soluciones y depuracion. |
| **C#** | `ms-dotnettools.csharp` (Microsoft) | Motor de lenguaje C#, IntelliSense, resaltado y analisis de codigo. |
| **PostgreSQL** | `ckolkman.vscode-postgres` (Chris Kolkman) | Conexion y ejecucion de consultas SQL a PostgreSQL desde VS Code. |
| **REST Client** | `humao.rest-client` (Huachao Mao) | Permite ejecutar peticiones HTTP directas en `SistemaReservas.http`. |

---

## Guia Paso a Paso de Instalacion y Ejecucion

### Paso 1: Clonar el Repositorio
```bash
git clone https://github.com/naiferrosado/sistema-reservas-restaurantes.git
cd sistema-reservas-restaurantes
```

### Paso 2: Crear la Base de Datos en PostgreSQL
Asegurate de que PostgreSQL este en ejecucion y crea la base de datos llamada **`restaurantereservas`**:

- **Desde consola `psql`:**
  ```bash
  psql -U postgres -c "CREATE DATABASE restaurantereservas;"
  ```
- **Desde pgAdmin 4:**
  1. Conectate a tu servidor local PostgreSQL.
  2. Clic derecho en **Databases** > **Create** > **Database...**
  3. En *Database Name*, escribe `restaurantereservas` y guarda.

### Paso 3: Configurar Credenciales (`appsettings.json`)
Copia la plantilla `appsettings.example.json` para generar tu archivo local:
- **En Windows PowerShell:**
  ```powershell
  Copy-Item appsettings.example.json appsettings.json
  ```
- **En Linux / macOS:**
  ```bash
  cp appsettings.example.json appsettings.json
  ```

Edita `appsettings.json` con tu contrasena de PostgreSQL:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=restaurantereservas;Username=postgres;Password=TU_CONTRASENA"
}
```

### Paso 4: Restaurar y Compilar
```bash
dotnet restore
dotnet build
```
*(Confirmaras `Build succeeded. 0 Warning(s), 0 Error(s)`)*.

### Paso 5: Iniciar el Servidor Web
```bash
dotnet run
```
Al iniciar, `DbInitializer` sembrara automaticamente los 3 restaurantes, cartas categorizadas y mesas por zona.

Abre tu navegador en:
- **Plataforma Web MVC:** [http://localhost:5043](http://localhost:5043)
- **Documentacion API / Swagger:** [http://localhost:5043/swagger](http://localhost:5043/swagger)

---

## Cumplimiento de Objetivos Tecnicos y Funcionales

| Objetivo | Componentes | Descripcion y Cumplimiento |
| :--- | :--- | :--- |
| **OE-01: Perfiles y Cartas Sembradas** | `DbInitializer.cs`, `HomeController.cs`, `MenuController.cs` | Siembra automatica al arrancar de *La Terraza Gourmet*, *Trattoria Don Corleone* y *Sakura Izakaya*, con sus cartas completas, precios, descripciones, etiquetas dieteticas y mesas por zonas (Terraza, Salon, VIP). |
| **OE-02: Motor de Reservas sin Sobreventa y Codigo Oficial** | `ReservationsController.cs`, `CreateReservationViewModel.cs`, `Views/Reservations/` | Validacion estricta contra sobreventa en la misma mesa y horario. Asignacion inteligente o manual. Generacion del codigo unico `RES-XXXX-XXXX` (ej. `RES-1023-4835`) en estado inicial `Pendiente` y consulta publica en `Reservations/Lookup`. |
| **OE-03: Dashboard con KPIs en Vivo y Aprobacion/Rechazo** | `DashboardController.cs`, `DashboardViewModel.cs`, `Views/Dashboard/` | Calculo en tiempo real de reservas del dia, comensales esperados, ocupacion en sala (%) y pendientes. Acciones de aprobacion (actualiza mesa y envia correo) y rechazo con motivo modal. |
| **OE-04: Plano de Mesas en Tiempo Real** | `TablesController.cs`, `_FloorPlanPartial.cshtml`, `app.js` | Accion de alternancia operativa de mesas (Disponible -> Reservada -> Ocupada -> Disponible) en menos de 500 ms con actualizacion reactiva de contadores de ocupacion. |
| **OE-05: Servicio de Correo HTML Profesional y Auditoria** | `EmailService.cs`, `NotificationsController.cs`, `NotificationLog.cs` | Plantilla HTML corporativa limpia con cabecera `#1B365D`, codigo destacado y cero emojis. Registro completo en base de datos visible en la bandeja de auditoria con modal de previsualizacion. |

---

## Guia de Prueba y Demostracion

1. **Exploracion Gastronomica (`/Home/Index`):**
   - Alterna entre los tres restaurantes usando la barra segmentada superior.
   - Observa la ficha hero con foto, direccion, telefono, horario y valoracion numerica (4.8 / 5.0).
   - Explora la Carta Gastronomica filtrable por categorias y la Distribucion de Mesas.
2. **Generacion de Reserva (`/Reservations/Create`):**
   - Haz clic en **Reservar Mesa** en la barra superior o en la ficha del restaurante.
   - Selecciona fecha, hora, comensales y mesa opcional.
   - El sistema valida disponibilidad, registra la reserva y muestra el comprobante con el codigo `RES-XXXX-XXXX`.
3. **Consulta de Estado (`/Reservations/Lookup`):**
   - Ingresa el codigo generado o el correo electronico registrado.
   - Observa la tarjeta detallada con el estado de la reserva y sus datos.
4. **Acceso y Autenticacion Administrativa (`/Account/Login`):**
   - Al hacer clic en **Panel Administrativo** o intentar acceder directamente a `/Dashboard/Index`, el sistema intercepta la peticion y redirige al formulario corporativo de inicio de sesion.
   - Utiliza las credenciales oficiales de demostracion:
     - **Correo:** `admin@sistemareservas.com`
     - **Contrasena:** `Admin123!`
5. **Gestion en el Panel Administrativo (`/Dashboard/Index`):**
   - Tras iniciar sesion, se muestra el indicador de usuario activo (`Administrador General`) en la barra superior.
   - Revisa las 4 tarjetas KPI en vivo.
   - En la tabla de reservas, aprueba o rechaza con motivo la solicitud creada.
   - Verifica el cambio en el contador y la recepcion en la bandeja de auditoria.
6. **Plano de Mesas en Vivo (`/Dashboard/Index?tab=floorplan`):**
   - Haz clic sobre cualquier mesa para alternar su estado en sala. Observa la transicion visual inmediata y la actualizacion de la ocupacion.
7. **Bandeja de Auditoria de Correos (`/Dashboard/Index?tab=inbox`):**
   - Haz clic en cualquier registro para abrir el modal con el correo HTML completo enviado al cliente.
8. **Cierre de Sesion Seguro (`/Account/Logout`):**
   - Haz clic en el boton "Salir" en la barra de navegacion superior para cerrar la sesion y retornar a la portada publica.

---

## Arquitectura y Tecnologias

| Capa | Tecnologia | Caracteristicas |
| :--- | :--- | :--- |
| **Patron Arquitectonico** | **ASP.NET Core MVC** | Modelo-Vista-Controlador con enrutamiento convencional `{controller=Home}/{action=Index}/{id?}`. |
| **Seguridad y Acceso** | **Cookie Authentication** | Control de acceso con atributo `[Authorize]`, hashing seguro de contraseñas y proteccion antiforgery. |
| **Vistas** | **Razor (.cshtml)** | Fuertemente tipadas con ViewModels, Tag Helpers nativos y vistas parciales modulares. |
| **API RESTful** | **Web API / Controllers** | Endpoints JSON concurrentes en `/api/*` y Swagger UI en `/swagger`. |
| **Persistencia** | **PostgreSQL** | Acceso a datos con **Entity Framework Core (`Npgsql`)** y migracion/siembra automatica. |
| **Diseno UI/UX** | **Vanilla CSS Corporativo** | Fondo blanco puro (`#FFFFFF`), paleta ejecutiva, sin frameworks pesados, 100% responsivo. |
| **Interaccion Reactiva**| **Vanilla JavaScript** | Asincronismo con Fetch API para toggles en <500 ms, modales accesibles y toasts sin emojis. |
| **Servicio de Correo** | **EmailService** | Plantillas HTML responsivas y soporte SMTP con respaldo en `NotificationLog`. |

---

## Estructura del Proyecto

```text
sistema-reservas-restaurantes/
├── .env.example                     # Plantilla de variables de entorno para Docker / Deploy
├── .gitignore                       # Exclusion estricta de credenciales (appsettings.json, .env)
├── appsettings.example.json         # Plantilla publica de configuracion
├── appsettings.json                 # Configuracion local protegida
├── Controllers/                     # Controladores MVC y API RESTful
│   ├── AccountController.cs         # Autenticacion, login, logout y sesiones
│   ├── DashboardController.cs       # Panel administrativo protegido con [Authorize]
│   ├── HomeController.cs            # Catalogo gastronomico y vistas de exploracion
│   ├── MenuController.cs            # API para gestion de platillos
│   ├── NotificationsController.cs   # API para consulta de correos enviados
│   ├── ReservationsController.cs    # Flujo de reserva, confirmacion, lookup y API
│   ├── RestaurantsController.cs     # API de consulta de restaurantes
│   └── TablesController.cs          # API y toggle en tiempo real de mesas
├── Data/                            # Contexto ORM e inicializacion
│   ├── ApplicationDbContext.cs      # DbContext con configuracion relacional
│   └── DbInitializer.cs             # Siembra inicial de restaurantes, mesas, cartas y usuario admin
├── DTOs/                            # Objetos de transferencia para API REST
├── Models/                          # Entidades de dominio
│   ├── MenuItem.cs                  # Platillos de la carta
│   ├── NotificationLog.cs           # Auditoria de correos electronicos
│   ├── Reservation.cs               # Reservas y codigos oficiales
│   ├── Restaurant.cs                # Perfiles de restaurantes
│   ├── RestaurantTable.cs           # Mesas, aforo y ubicacion
│   └── User.cs                      # Usuarios administradores y credenciales
├── Services/                        # Logica de negocio y servicios
│   ├── EmailService.cs              # Generacion de HTML corporativo y SMTP
│   └── IEmailService.cs             # Contrato de servicio de correo
├── ViewModels/                      # Modelos de vista fuertemente tipados
│   ├── CreateReservationViewModel.cs
│   ├── DashboardViewModel.cs
│   ├── LoginViewModel.cs            # Modelo para formulario de login
│   ├── ReservationConfirmationViewModel.cs
│   ├── ReservationLookupViewModel.cs
│   └── RestaurantCatalogViewModel.cs
├── Views/                           # Vistas Razor (.cshtml)
│   ├── _ViewImports.cshtml          # Importacion de namespaces y Tag Helpers
│   ├── _ViewStart.cshtml            # Plantilla maestra por defecto
│   ├── Account/
│   │   └── Login.cshtml             # Vista de login administrativo
│   ├── Dashboard/
│   │   └── Index.cshtml             # Panel administrativo integral
│   ├── Home/
│   │   └── Index.cshtml             # Explorador gastronomico y ficha hero
│   ├── Reservations/
│   │   ├── Confirmation.cshtml      # Comprobante con codigo oficial
│   │   ├── Create.cshtml            # Formulario de reserva en linea
│   │   └── Lookup.cshtml            # Consulta de estado por codigo o correo
│   └── Shared/
│       ├── _EmailInboxPartial.cshtml
│       ├── _FloorPlanPartial.cshtml
│       ├── _KpiCardsPartial.cshtml
│       ├── _Layout.cshtml           # Plantilla maestra con navbar fija y footer
│       ├── _MenuManagementPartial.cshtml
│       └── _ReservationsTablePartial.cshtml
├── wwwroot/                         # Activos web estaticos
│   ├── css/
│   │   └── styles.css               # Sistema de diseno corporativo blanco ejecutivo
│   └── js/
│       └── app.js                   # Interactividad, modales, toasts y Fetch API
├── Program.cs                       # Pipeline HTTP, autenticacion por cookies, registro MVC y Swagger
├── SistemaReservas.csproj           # Archivo de proyecto .NET 10
└── SistemaReservas.http             # Archivo para pruebas directas HTTP
```

---

## Seguridad y Variables de Entorno

- **Autenticacion de Administradores:** El panel administrativo (`/Dashboard/*`) requiere sesion activa validada por el atributo `[Authorize]`. Las contraseñas se almacenan mediante el algoritmo de hash oficial de ASP.NET Core (`PasswordHasher<string>`).
- **Credenciales del Administrador Sembrado:**
  - **Correo:** `admin@sistemareservas.com`
  - **Contrasena:** `Admin123!`
- **Proteccion de Credenciales:** Los archivos `appsettings.json` y `.env` estan protegidos mediante `.gitignore` para evitar filtraciones de contraseñas de bases de datos o credenciales SMTP.
- Utiliza siempre las plantillas `appsettings.example.json` y `.env.example` para desplegar el proyecto en nuevos entornos de desarrollo o produccion.
