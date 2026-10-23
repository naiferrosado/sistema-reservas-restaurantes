# 🍽️ Sistema de Gestión de Reservas para Restaurantes

> **Asignatura:** Automatización de Procesos  
> **Trabajo Práctico:** Proyecto Tecnológico – Sistema de Reservas para Restaurantes  
> **Backend:** ASP.NET Core & Entity Framework Core  
> **Base de Datos:** PostgreSQL  
> **Frontend:** Single-Page Application (HTML5, CSS3, JavaScript ES Modules)

---

## 📋 Tabla de Contenidos
1. [Descripción del Proyecto](#-descripción-del-proyecto)
2. [Requisitos Previos (Software a Instalar)](#-requisitos-previos-software-a-instalar)
3. [Extensiones Recomendadas para Visual Studio Code](#-extensiones-recomendadas-para-visual-studio-code)
4. [Guía Paso a Paso de Instalación y Ejecución](#-guía-paso-a-paso-de-instalación-y-ejecución)
5. [Guía de Prueba y Demostración](#-guía-de-prueba-y-demostración-para-evaluación)
6. [Características Implementadas](#-características-implementadas)
7. [Arquitectura y Tecnologías](#-arquitectura-y-tecnologías)
8. [Estructura del Proyecto](#-estructura-del-proyecto)
9. [Seguridad y Variables de Entorno](#-seguridad-y-variables-de-entorno)

---

## 📖 Descripción del Proyecto
Plataforma web integral para la automatización del proceso de reservas en restaurantes. Permite a los comensales explorar la oferta gastronómica, verificar la disponibilidad de mesas en tiempo real y reservar en línea con confirmación inmediata. A su vez, dota a los administradores de un panel de control con métricas en vivo, gestión del plano de mesas en sala y envío automatizado de correos electrónicos de confirmación.

---

## 💻 Requisitos Previos (Software a Instalar)

Antes de ejecutar el proyecto, asegúrate de tener instaladas las siguientes herramientas en tu sistema operativo:

### 1. [.NET SDK](https://dotnet.microsoft.com/download) (.NET 10 o .NET 8 / 9)
- **Descripción:** Entorno de ejecución y compilador de C# / ASP.NET Core.
- **Verificación en consola:**
  ```bash
  dotnet --version
  ```
  *(Debe mostrar una versión numérica como `10.0.x`, `9.0.x` o `8.0.x`)*.

### 2. [PostgreSQL Server](https://www.postgresql.org/download/) (Versión 14, 15, 16, 17 o 18)
- **Descripción:** Sistema de gestión de bases de datos relacionales SQL.
- **Herramienta gráfica recomendada:** [pgAdmin 4](https://www.pgadmin.org/download/) (suele incluirse en el instalador oficial de PostgreSQL).
- **Verificación en consola:**
  ```bash
  psql --version
  ```
  *(Asegúrate de que el servicio de PostgreSQL esté iniciado en el puerto `5432`)*.

### 3. [Git](https://git-scm.com/)
- **Descripción:** Sistema de control de versiones para clonar el repositorio.
- **Verificación en consola:**
  ```bash
  git --version
  ```

### 4. Navegador Web Moderno
- Google Chrome, Microsoft Edge, Mozilla Firefox o Brave.

---

## 🧩 Extensiones Recomendadas para Visual Studio Code

Si abres el proyecto con **Visual Studio Code**, se sugiere instalar las siguientes extensiones desde la pestaña de Extensiones (`Ctrl + Shift + X`):

| Extensión | Identificador / Autor | Utilidad |
| :--- | :--- | :--- |
| **C# Dev Kit** | `ms-dotnettools.csdevkit` (Microsoft) | Soporte completo oficial para C#, explorador de soluciones y depuración. |
| **C#** | `ms-dotnettools.csharp` (Microsoft) | Motor de lenguaje C#, IntelliSense, resaltado de sintaxis y autocompletado. |
| **PostgreSQL** | `ckolkman.vscode-postgres` (Chris Kolkman) | Conexión, navegación y ejecución de consultas SQL a PostgreSQL desde VS Code. |
| **REST Client** | `humao.rest-client` (Huachao Mao) | Permite ejecutar las peticiones HTTP de prueba del archivo `SistemaReservas.http`. |

---

## 🚀 Guía Paso a Paso de Instalación y Ejecución

Sigue estos 5 sencillos pasos para poner en marcha el proyecto desde cero:

### Paso 1: Clonar el Repositorio
Abre tu terminal (PowerShell, CMD o Bash) y clona el proyecto:
```bash
git clone https://github.com/tu-usuario/sistema-reservas-restaurantes.git
cd sistema-reservas-restaurantes
```

### Paso 2: Crear la Base de Datos en PostgreSQL
Asegúrate de que PostgreSQL esté en ejecución y crea la base de datos llamada **`restaurantereservas`**.

- **Opción A (Desde consola `psql`):**
  ```bash
  psql -U postgres -c "CREATE DATABASE restaurantereservas;"
  ```
- **Opción B (Desde pgAdmin 4):**
  1. Conéctate a tu servidor local PostgreSQL.
  2. Haz clic derecho sobre **Databases** > **Create** > **Database...**
  3. En *Database Name*, escribe: `restaurantereservas` y guarda.

### Paso 3: Configurar las Credenciales (`appsettings.json`)
Por seguridad, el archivo con contraseñas no se sube a Git. Debes crearlo a partir de la plantilla de ejemplo:

1. Copia el archivo `appsettings.example.json` y nómbralo `appsettings.json`:
   - **En Windows PowerShell:**
     ```powershell
     Copy-Item appsettings.example.json appsettings.json
     ```
   - **En Linux / macOS:**
     ```bash
     cp appsettings.example.json appsettings.json
     ```
2. Abre `appsettings.json` y coloca tu contraseña de PostgreSQL en `DefaultConnection`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=localhost;Port=5432;Database=restaurantereservas;Username=postgres;Password=TU_CONTRASENA"
   }
   ```
   *(Si tu usuario o puerto son distintos, ajústalos correspondientemente)*.

### Paso 4: Restaurar Paquetes y Compilar
Ejecuta en la raíz del proyecto:
```bash
dotnet restore
dotnet build
```
*(Debe indicar `Build succeeded. 0 Warning(s), 0 Error(s)`)*.

### Paso 5: Iniciar la Aplicación
Inicia el servidor web con:
```bash
dotnet run
```

Al iniciar, Entity Framework Core se conectará a PostgreSQL, creará las tablas y sembrará los restaurantes y cartas automáticamente.

Una vez veas en consola `Now listening on: http://localhost:5043`, abre tu navegador en:
- 🌐 **Aplicación Web:** [http://localhost:5043](http://localhost:5043)
- 📖 **Documentación Swagger API:** [http://localhost:5043/swagger](http://localhost:5043/swagger)

---

## 🧪 Guía de Prueba y Demostración (Para Evaluación)

Para comprobar el correcto funcionamiento de cada requisito de la consigna:

1. **Explorar Restaurantes y Menús:**
   - En la página principal, haz clic en los chips superiores para alternar entre *La Terraza Gourmet*, *Trattoria Don Corleone* y *Sakura Izakaya*. Observa cómo cambian las fotos, horarios, tipos de cocina y la carta gastronómica.
2. **Consultar Distribución de Mesas:**
   - Haz clic en la pestaña *“Disponibilidad de Mesas y Ubicaciones”* para ver la capacidad de cada área (Terraza, Salón, VIP).
3. **Hacer una Reserva Online:**
   - Presiona el botón **“📅 Reservar Mesa Ahora”**.
   - Completa el formulario (fecha, hora, número de personas, nombre y correo) y confirma.
   - El sistema generará una clave oficial única (ej. `RES-1023-4835`).
4. **Consultar Estado de la Reserva:**
   - Dirígete a la pestaña superior **“🔍 Consultar Reserva”**, ingresa el código o tu correo y verifica que figure como *Pendiente*.
5. **Dashboard Administrativo del Restaurante:**
   - Ve a **“⚙️ Dashboard Restaurante”**.
   - Observa los indicadores KPI en tiempo real (Reservas hoy, comensales, ocupación en sala %).
   - En la lista de reservas, localiza la reserva recién creada y haz clic en **“✓ Aprobar”**.
   - Verás una notificación toast confirmando la aprobación y el envío del correo.
6. **Plano de Mesas en Sala:**
   - Entra al subtab *“🪑 Plano de Mesas en Tiempo Real”* y haz clic sobre cualquier mesa para alternar su estado operativo en vivo (🟢 *Disponible* ➔ 🔵 *Reservada* ➔ 🔴 *Ocupada*).
7. **Verificación del Correo de Confirmación:**
   - En el subtab *“✉️ Bandeja de Notificaciones y Correos”*, haz clic en **“Ver Correo ↗”** para inspeccionar la plantilla HTML enviada al cliente con todos los detalles de la reserva.

---

## 🌟 Características Implementadas

### 1. 🏢 Perfiles de Restaurantes
- Información integral: nombre, dirección, teléfono, correo, horarios, tipo de cocina, fotos y valoración.
- Menús completos categorizados con precios, descripciones y etiquetas dietéticas (*Sin Gluten, Vegetariano*).
- Configuración de mesas por aforo y ubicación (*Terraza, Salón, VIP*).

### 2. 📅 Reservas para Clientes
- Selección dinámica de fecha, hora y comensales con prevención de sobreventa.
- Asignación inteligente automática de mesa o selección manual.
- Código único oficial de reserva y consulta en tiempo real.

### 3. 📊 Dashboard de Administración
- Tarjetas KPI en vivo (Ocupación %, reservas del día, comensales esperados, pendientes).
- Tabla interactiva con filtros y acciones inmediatas de *Aprobación* y *Rechazo con motivo*.
- Control operativo del plano de mesas en sala.
- Gestión de cartas y platillos.

### 4. ✉️ Notificaciones Automáticas
- Plantilla HTML visual con detalles de reserva, código e instrucciones de llegada.
- Bandeja de inspección de correos dentro del dashboard.
- Soporte para envío SMTP real o simulación registrada.

---

## 🛠️ Arquitectura y Tecnologías

| Capa | Tecnología |
| :--- | :--- |
| **Backend** | **ASP.NET Core (.NET 10 / C#)** con Web API y controladores RESTful |
| **Persistencia** | **PostgreSQL** a través de **Entity Framework Core (`Npgsql`)** |
| **Documentación API**| **OpenAPI / Swagger UI** interactivo en `/swagger` |
| **Frontend** | **HTML5 Semántico, Vanilla CSS3** (Glassmorphism, temas Dark/Light, Google Fonts) y **JavaScript Moderno (ES Modules)** |
| **Servicio de Correo** | `EmailService` con plantillas HTML y soporte SMTP |

---

## 📂 Estructura del Proyecto

```text
sistema-reservas-restaurantes/
├── .env.example              # Plantilla de variables de entorno para Docker / Deploy
├── .gitignore                # Reglas de exclusión de credenciales y binarios
├── appsettings.example.json  # Plantilla de configuración de conexión pública
├── appsettings.json          # Configuración local (ignorado por Git por seguridad)
├── Controllers/              # Controladores RESTful
│   ├── DashboardController.cs
│   ├── MenuController.cs
│   ├── NotificationsController.cs
│   ├── ReservationsController.cs
│   ├── RestaurantsController.cs
│   └── TablesController.cs
├── Data/                     # Contexto de base de datos e inicializador
│   ├── ApplicationDbContext.cs
│   └── DbInitializer.cs
├── DTOs/                     # Data Transfer Objects y validaciones
│   ├── CreateReservationDto.cs
│   ├── DashboardStatsDto.cs
│   ├── MenuItemDtos.cs
│   ├── TableDtos.cs
│   └── UpdateReservationStatusDto.cs
├── Models/                   # Entidades del dominio (ORM)
│   ├── MenuItem.cs
│   ├── NotificationLog.cs
│   ├── Reservation.cs
│   ├── Restaurant.cs
│   └── RestaurantTable.cs
├── Services/                 # Lógica de negocio y envío de correos
│   ├── EmailService.cs
│   └── IEmailService.cs
├── wwwroot/                  # Frontend Single-Page Application
│   ├── css/
│   │   └── styles.css        # Sistema de diseño, temas claro/oscuro y animaciones
│   ├── js/
│   │   ├── api.js            # Cliente fetch para consumir los endpoints REST
│   │   └── app.js            # Lógica reactiva, plano de mesas y modales
│   └── index.html            # Markup HTML5 con vistas de cliente y administrador
├── Program.cs                # Configuración de servicios, Swagger, static files y pipeline
├── SistemaReservas.csproj    # Archivo del proyecto y paquetes NuGet
└── SistemaReservas.http      # Archivo para pruebas directas de endpoints REST
```

---

## 🔒 Seguridad y Variables de Entorno

- Los archivos [`appsettings.json`](file:///c:/Users/Naifer/Documents/sistema-reservas-restaurantes/appsettings.json) y `.env` están protegidos en [`.gitignore`](file:///c:/Users/Naifer/Documents/sistema-reservas-restaurantes/.gitignore) para prevenir filtraciones de contraseñas de bases de datos o credenciales SMTP.
- Utiliza siempre los archivos de plantilla [`appsettings.example.json`](file:///c:/Users/Naifer/Documents/sistema-reservas-restaurantes/appsettings.example.json) y [`.env.example`](file:///c:/Users/Naifer/Documents/sistema-reservas-restaurantes/.env.example) al compartir o clonar el código en nuevos entornos.
