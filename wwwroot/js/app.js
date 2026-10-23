import { api } from './api.js';

// Application State
const state = {
  restaurants: [],
  currentRestaurantId: null,
  currentRestaurant: null,
  adminRestaurantId: null,
  clientSubtab: 'menu',
  adminSubtab: 'reservations',
  reservations: [],
  reservationFilter: 'Todos',
  reservationSearch: '',
  theme: localStorage.getItem('theme') || 'dark',
};

// Utilities: Toast Notification
function showToast(message, type = 'success') {
  const container = document.getElementById('toast-container');
  const toast = document.createElement('div');
  toast.className = `toast toast-${type}`;
  
  const icon = type === 'success' ? '✓' : type === 'error' ? '✕' : 'ℹ';
  toast.innerHTML = `<span style="font-weight: bold;">${icon}</span> <span>${message}</span>`;
  
  container.appendChild(toast);
  setTimeout(() => {
    toast.style.opacity = '0';
    toast.style.transform = 'translateY(10px)';
    setTimeout(() => toast.remove(), 300);
  }, 4000);
}

// Formatting helpers
function formatDate(dateStr) {
  if (!dateStr) return '';
  const d = new Date(dateStr);
  return d.toLocaleDateString('es-ES', { weekday: 'short', day: 'numeric', month: 'short', year: 'numeric' });
}

function formatStatusPill(status) {
  const s = (status || 'Pendiente').toLowerCase();
  let pillClass = 'pill-disponible';
  if (s === 'pendiente') pillClass = 'pill-reservada';
  if (s === 'confirmada') pillClass = 'pill-disponible';
  if (s === 'rechazada' || s === 'cancelada') pillClass = 'pill-ocupada';
  return `<span class="table-status-pill ${pillClass}">${status}</span>`;
}

// -------------------------------------------------------------
// INITIALIZATION
// -------------------------------------------------------------
document.addEventListener('DOMContentLoaded', async () => {
  initTheme();
  initNavigation();
  initModals();
  await loadRestaurants();
  setDefaultBookingDate();
});

// Theme handling
function initTheme() {
  document.documentElement.setAttribute('data-theme', state.theme);
  const btn = document.getElementById('theme-toggle');
  btn.textContent = state.theme === 'dark' ? '☀️' : '🌙';

  btn.addEventListener('click', () => {
    state.theme = state.theme === 'dark' ? 'light' : 'dark';
    document.documentElement.setAttribute('data-theme', state.theme);
    localStorage.setItem('theme', state.theme);
    btn.textContent = state.theme === 'dark' ? '☀️' : '🌙';
  });
}

// Navigation Tabs
function initNavigation() {
  const tabs = document.querySelectorAll('.nav-tab-btn');
  tabs.forEach(tab => {
    tab.addEventListener('click', () => {
      const view = tab.dataset.view;
      switchView(view);
    });
  });

  document.getElementById('brand-home').addEventListener('click', (e) => {
    e.preventDefault();
    switchView('client');
  });

  // Client subtabs (Menú vs Mesas)
  document.getElementById('client-subtab-menu').addEventListener('click', () => switchClientSubtab('menu'));
  document.getElementById('client-subtab-tables').addEventListener('click', () => switchClientSubtab('tables'));

  // Admin subtabs
  document.querySelectorAll('[data-admin-tab]').forEach(btn => {
    btn.addEventListener('click', () => switchAdminSubtab(btn.dataset.adminTab));
  });

  // Admin restaurant switcher
  document.getElementById('admin-restaurant-select').addEventListener('change', (e) => {
    state.adminRestaurantId = parseInt(e.target.value);
    loadAdminDashboard();
  });

  document.getElementById('btn-refresh-dashboard').addEventListener('click', () => {
    loadAdminDashboard();
    showToast('Dashboard actualizado');
  });

  // Scroll to menu
  document.getElementById('btn-scroll-menu').addEventListener('click', () => {
    switchClientSubtab('menu');
    document.getElementById('menu-items-grid').scrollIntoView({ behavior: 'smooth' });
  });

  // Search in admin reservations
  document.getElementById('admin-reservation-search').addEventListener('input', (e) => {
    state.reservationSearch = e.target.value.toLowerCase();
    renderAdminReservations();
  });

  // Filters in admin reservations
  document.querySelectorAll('.filter-btn').forEach(btn => {
    btn.addEventListener('click', () => {
      document.querySelectorAll('.filter-btn').forEach(b => b.classList.remove('active'));
      btn.classList.add('active');
      state.reservationFilter = btn.dataset.filter;
      renderAdminReservations();
    });
  });

  // Lookup button
  document.getElementById('btn-search-reservation').addEventListener('click', performReservationLookup);
  document.getElementById('lookup-input').addEventListener('keypress', (e) => {
    if (e.key === 'Enter') performReservationLookup();
  });

  // Refresh notifications
  document.getElementById('btn-refresh-notifications').addEventListener('click', () => {
    loadNotifications();
    showToast('Bandeja de correos sincronizada');
  });
}

function switchView(viewName) {
  document.querySelectorAll('.nav-tab-btn').forEach(btn => {
    btn.classList.toggle('active', btn.dataset.view === viewName);
  });

  document.querySelectorAll('.app-view').forEach(view => {
    view.style.display = 'none';
  });

  const target = document.getElementById(`view-${viewName}`);
  if (target) target.style.display = 'block';

  if (viewName === 'admin') {
    loadAdminDashboard();
  }
}

function switchClientSubtab(subtab) {
  state.clientSubtab = subtab;
  document.getElementById('client-subtab-menu').classList.toggle('active', subtab === 'menu');
  document.getElementById('client-subtab-tables').classList.toggle('active', subtab === 'tables');

  document.getElementById('client-content-menu').style.display = subtab === 'menu' ? 'block' : 'none';
  document.getElementById('client-content-tables').style.display = subtab === 'tables' ? 'block' : 'none';
}

function switchAdminSubtab(subtab) {
  state.adminSubtab = subtab;
  document.querySelectorAll('[data-admin-tab]').forEach(btn => {
    btn.classList.toggle('active', btn.dataset.adminTab === subtab);
  });

  document.getElementById('admin-content-reservations').style.display = subtab === 'reservations' ? 'block' : 'none';
  document.getElementById('admin-content-floorplan').style.display = subtab === 'floorplan' ? 'block' : 'none';
  document.getElementById('admin-content-menu').style.display = subtab === 'menu' ? 'block' : 'none';
  document.getElementById('admin-content-notifications').style.display = subtab === 'notifications' ? 'block' : 'none';

  if (subtab === 'notifications') {
    loadNotifications();
  }
}

// -------------------------------------------------------------
// LOAD RESTAURANTS & CLIENT VIEW
// -------------------------------------------------------------
async function loadRestaurants() {
  try {
    const list = await api.getRestaurants();
    state.restaurants = list;
    if (list.length > 0) {
      state.currentRestaurantId = list[0].id;
      state.adminRestaurantId = list[0].id;
      renderRestaurantChips();
      populateAdminRestaurantSelect();
      await selectRestaurant(list[0].id);
    }
  } catch (err) {
    showToast('Error al conectar con la API: ' + err.message, 'error');
  }
}

function renderRestaurantChips() {
  const container = document.getElementById('restaurant-chips-container');
  container.innerHTML = state.restaurants.map(r => `
    <div class="restaurant-chip ${r.id === state.currentRestaurantId ? 'active' : ''}" data-id="${r.id}">
      <span>🍽️</span>
      <span>${r.name}</span>
      <span style="font-size:0.75rem;opacity:0.8;">(${r.cuisineType})</span>
    </div>
  `).join('');

  container.querySelectorAll('.restaurant-chip').forEach(chip => {
    chip.addEventListener('click', () => {
      const id = parseInt(chip.dataset.id);
      selectRestaurant(id);
    });
  });
}

function populateAdminRestaurantSelect() {
  const select = document.getElementById('admin-restaurant-select');
  select.innerHTML = state.restaurants.map(r => `
    <option value="${r.id}" ${r.id === state.adminRestaurantId ? 'selected' : ''}>${r.name}</option>
  `).join('');
}

async function selectRestaurant(id) {
  state.currentRestaurantId = id;
  renderRestaurantChips();

  try {
    const data = await api.getRestaurant(id);
    state.currentRestaurant = data;
    renderRestaurantShowcase(data);
    renderClientMenu(data.menuItems || []);
    renderClientTables(data.tables || []);
  } catch (err) {
    showToast('Error cargando restaurante: ' + err.message, 'error');
  }
}

function renderRestaurantShowcase(r) {
  document.getElementById('showcase-name').textContent = r.name;
  document.getElementById('showcase-cuisine').textContent = r.cuisineType;
  document.getElementById('showcase-desc').textContent = r.description;
  document.getElementById('showcase-address').textContent = r.address;
  document.getElementById('showcase-schedule').textContent = `${r.openingTime} - ${r.closingTime} hrs`;
  document.getElementById('showcase-phone').textContent = r.phone;
  document.getElementById('showcase-price').textContent = `Rango: ${r.priceRange}`;
  document.getElementById('showcase-rating').textContent = `⭐ ${r.rating} (Excelente)`;
  if (r.imageUrl) {
    document.getElementById('showcase-img').src = r.imageUrl;
  }
}

function renderClientMenu(items) {
  const grid = document.getElementById('menu-items-grid');
  if (!items || items.length === 0) {
    grid.innerHTML = `<div style="grid-column: 1/-1; text-align: center; color: var(--text-muted); padding: 40px;">No hay platillos registrados aún en este restaurante.</div>`;
    return;
  }

  grid.innerHTML = items.map(item => `
    <div class="menu-card">
      <img src="${item.imageUrl || 'https://images.unsplash.com/photo-1546069901-ba9599a7e63c?auto=format&fit=crop&w=600&q=80'}" alt="${item.name}" class="menu-img">
      <div class="menu-info">
        <div class="menu-header">
          <div class="menu-name">${item.name}</div>
          <div class="menu-price">${item.price.toFixed(2)} €</div>
        </div>
        <div class="menu-desc">${item.description}</div>
        <div class="menu-tags">
          <span class="tag-pill" style="background: rgba(99, 102, 241, 0.15); color: #a5b4fc;">${item.category}</span>
          ${item.tags ? item.tags.split(',').map(t => `<span class="tag-pill">${t.trim()}</span>`).join('') : ''}
        </div>
      </div>
    </div>
  `).join('');
}

function renderClientTables(tables) {
  const grid = document.getElementById('client-tables-grid');
  if (!tables || tables.length === 0) {
    grid.innerHTML = `<div style="grid-column: 1/-1; text-align: center; color: var(--text-muted); padding: 30px;">No hay mesas registradas.</div>`;
    return;
  }

  grid.innerHTML = tables.map(t => {
    const statusClass = `status-${t.status.toLowerCase()}`;
    const pillClass = `pill-${t.status.toLowerCase()}`;
    return `
      <div class="table-slot ${statusClass}" data-table-id="${t.id}">
        <div class="table-number">Mesa ${t.tableNumber}</div>
        <div class="table-cap">👥 Capacidad: ${t.capacity} pers.</div>
        <div class="table-loc">📍 ${t.location}</div>
        <div class="table-status-pill ${pillClass}">${t.status}</div>
      </div>
    `;
  }).join('');
}

// -------------------------------------------------------------
// BOOKING MODAL & FLOW
// -------------------------------------------------------------
function setDefaultBookingDate() {
  const today = new Date();
  const yyyy = today.getFullYear();
  const mm = String(today.getMonth() + 1).padStart(2, '0');
  const dd = String(today.getDate()).padStart(2, '0');
  document.getElementById('booking-date').value = `${yyyy}-${mm}-${dd}`;
  document.getElementById('booking-date').min = `${yyyy}-${mm}-${dd}`;
}

function initModals() {
  const modalBooking = document.getElementById('modal-booking');
  const modalSuccess = document.getElementById('modal-booking-success');
  const modalEmail = document.getElementById('modal-email-preview');
  const modalTable = document.getElementById('modal-create-table');
  const modalMenu = document.getElementById('modal-create-menu');

  // Open booking modal
  document.getElementById('btn-open-booking-modal').addEventListener('click', () => {
    if (!state.currentRestaurant) return;
    document.getElementById('booking-modal-restaurant-name').textContent = state.currentRestaurant.name;
    populateTableSelectForBooking(state.currentRestaurant.tables || []);
    modalBooking.classList.add('active');
  });

  // Close booking modal
  document.getElementById('btn-close-booking').addEventListener('click', () => modalBooking.classList.remove('active'));
  document.getElementById('btn-cancel-booking').addEventListener('click', () => modalBooking.classList.remove('active'));

  // Close success modal
  document.getElementById('btn-close-success').addEventListener('click', () => modalSuccess.classList.remove('active'));

  // Close email modal
  document.getElementById('btn-close-email-preview').addEventListener('click', () => modalEmail.classList.remove('active'));
  document.getElementById('btn-close-email-preview-footer').addEventListener('click', () => modalEmail.classList.remove('active'));

  // Table modal controls
  document.getElementById('btn-open-create-table-modal').addEventListener('click', () => modalTable.classList.add('active'));
  document.getElementById('btn-close-table-modal').addEventListener('click', () => modalTable.classList.remove('active'));
  document.getElementById('btn-cancel-table').addEventListener('click', () => modalTable.classList.remove('active'));

  // Menu modal controls
  document.getElementById('btn-open-create-menu-modal').addEventListener('click', () => modalMenu.classList.add('active'));
  document.getElementById('btn-close-menu-modal').addEventListener('click', () => modalMenu.classList.remove('active'));
  document.getElementById('btn-cancel-menu').addEventListener('click', () => modalMenu.classList.remove('active'));

  // Submit booking form
  document.getElementById('form-booking').addEventListener('submit', handleBookingSubmit);

  // Submit create table form
  document.getElementById('form-create-table').addEventListener('submit', handleCreateTableSubmit);

  // Submit create menu item form
  document.getElementById('form-create-menu').addEventListener('submit', handleCreateMenuSubmit);
}

function populateTableSelectForBooking(tables) {
  const select = document.getElementById('booking-table-select');
  select.innerHTML = '<option value="">Asignación Inteligente (Recomendado)</option>';
  tables.forEach(t => {
    select.innerHTML += `
      <option value="${t.id}">Mesa ${t.tableNumber} - ${t.location} (Máx: ${t.capacity} pers) [${t.status}]</option>
    `;
  });
}

async function handleBookingSubmit(e) {
  e.preventDefault();
  const btn = document.getElementById('btn-submit-booking');
  btn.disabled = true;
  btn.textContent = 'Enviando solicitud...';

  const tableVal = document.getElementById('booking-table-select').value;
  const dto = {
    restaurantId: state.currentRestaurantId,
    tableId: tableVal ? parseInt(tableVal) : null,
    customerName: document.getElementById('booking-name').value.trim(),
    customerEmail: document.getElementById('booking-email').value.trim(),
    customerPhone: document.getElementById('booking-phone').value.trim(),
    reservationDate: document.getElementById('booking-date').value,
    reservationTime: document.getElementById('booking-time').value,
    partySize: parseInt(document.getElementById('booking-party-size').value),
    specialRequests: document.getElementById('booking-notes').value.trim()
  };

  try {
    const res = await api.createReservation(dto);
    document.getElementById('modal-booking').classList.remove('active');
    document.getElementById('form-booking').reset();
    setDefaultBookingDate();

    // Show success dialog
    document.getElementById('success-res-code').textContent = res.reservationCode;
    document.getElementById('success-res-summary').innerHTML = `
      <div><strong>Restaurante:</strong> ${res.restaurant?.name || state.currentRestaurant?.name}</div>
      <div><strong>Fecha:</strong> ${formatDate(res.reservationDate)} a las ${res.reservationTime} hrs</div>
      <div><strong>Comensales:</strong> ${res.partySize} personas</div>
      <div><strong>Mesa:</strong> ${res.table ? `Mesa ${res.table.tableNumber} (${res.table.location})` : 'Mesa asignada al llegar'}</div>
      <div><strong>Cliente:</strong> ${res.customerName} (${res.customerEmail})</div>
    `;
    document.getElementById('modal-booking-success').classList.add('active');
    showToast('¡Reserva registrada con éxito!');
  } catch (err) {
    showToast(err.message, 'error');
  } finally {
    btn.disabled = false;
    btn.textContent = 'Confirmar Solicitud de Reserva';
  }
}

// -------------------------------------------------------------
// RESERVATION LOOKUP VIEW
// -------------------------------------------------------------
async function performReservationLookup() {
  const input = document.getElementById('lookup-input').value.trim();
  const container = document.getElementById('lookup-results-container');
  if (!input) {
    showToast('Ingresa un código de reserva o correo', 'error');
    return;
  }

  container.innerHTML = `<div style="text-align: center; padding: 24px; color: var(--text-muted);">Buscando...</div>`;

  try {
    const isEmail = input.includes('@');
    const results = await api.lookupReservation(isEmail ? null : input, isEmail ? input : null);

    if (!results || results.length === 0) {
      container.innerHTML = `
        <div class="glass-card" style="text-align: center; padding: 40px;">
          <div style="font-size: 2.4rem; margin-bottom: 12px;">🔍</div>
          <h3>No se encontraron reservas</h3>
          <p style="color: var(--text-muted); font-size: 0.9rem; margin-top: 6px;">Verifica el código (ej. RES-98421) o correo electrónico ingresado.</p>
        </div>
      `;
      return;
    }

    container.innerHTML = results.map(r => `
      <div class="glass-card" style="margin-bottom: 16px;">
        <div style="display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 14px; flex-wrap: wrap; gap: 8px;">
          <div>
            <div style="font-size: 0.78rem; text-transform: uppercase; color: var(--accent-primary); font-weight: 700; letter-spacing: 0.05em;">Código de Reserva</div>
            <div style="font-family: var(--font-heading); font-size: 1.4rem; font-weight: 700;">${r.reservationCode}</div>
          </div>
          <div>${formatStatusPill(r.status)}</div>
        </div>

        <div style="display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 12px; font-size: 0.9rem; margin-bottom: 16px;">
          <div><span style="color: var(--text-dim);">Restaurante:</span> <strong>${r.restaurant?.name || 'N/A'}</strong></div>
          <div><span style="color: var(--text-dim);">Fecha & Hora:</span> <strong>${formatDate(r.reservationDate)} - ${r.reservationTime} hrs</strong></div>
          <div><span style="color: var(--text-dim);">Comensales:</span> <strong>${r.partySize} personas</strong></div>
          <div><span style="color: var(--text-dim);">Mesa:</span> <strong>${r.table ? `Mesa ${r.table.tableNumber} (${r.table.location})` : 'Asignada al llegar'}</strong></div>
          <div><span style="color: var(--text-dim);">Titular:</span> <strong>${r.customerName}</strong></div>
          <div><span style="color: var(--text-dim);">Contacto:</span> <strong>${r.customerPhone}</strong></div>
        </div>

        ${r.specialRequests ? `<div style="background: rgba(255,255,255,0.03); padding: 10px 14px; border-radius: 8px; font-size: 0.85rem; margin-bottom: 14px;"><strong style="color: var(--accent-gold);">Peticiones especiales:</strong> ${r.specialRequests}</div>` : ''}

        ${r.status === 'Pendiente' ? `
          <div style="display: flex; justify-content: flex-end;">
            <button class="btn btn-outline btn-sm" style="color: var(--color-danger); border-color: rgba(239, 68, 68, 0.4);" onclick="window.cancelCustomerReservation(${r.id})">
              Cancelar Solicitud
            </button>
          </div>
        ` : ''}
      </div>
    `).join('');
  } catch (err) {
    showToast(err.message, 'error');
  }
}

window.cancelCustomerReservation = async (id) => {
  if (!confirm('¿Estás seguro de que deseas cancelar esta reserva?')) return;
  try {
    await api.cancelReservation(id);
    showToast('Reserva cancelada');
    performReservationLookup();
  } catch (err) {
    showToast(err.message, 'error');
  }
};

// -------------------------------------------------------------
// ADMIN DASHBOARD
// -------------------------------------------------------------
async function loadAdminDashboard() {
  if (!state.adminRestaurantId) return;
  const currentRest = state.restaurants.find(r => r.id === state.adminRestaurantId);
  if (currentRest) {
    document.getElementById('admin-restaurant-name').textContent = `Dashboard - ${currentRest.name}`;
  }

  try {
    const [stats, reservations, fullRest] = await Promise.all([
      api.getDashboardStats(state.adminRestaurantId),
      api.getReservations({ restaurantId: state.adminRestaurantId }),
      api.getRestaurant(state.adminRestaurantId)
    ]);

    // Update KPI cards
    document.getElementById('kpi-reservations-today').textContent = stats.totalReservationsToday;
    document.getElementById('kpi-guests-today').textContent = stats.expectedGuestsToday;
    document.getElementById('kpi-occupied-tables').textContent = `${stats.occupiedTables} / ${stats.totalTables}`;
    document.getElementById('kpi-occupancy-rate').textContent = `${stats.occupancyRate}%`;
    document.getElementById('kpi-pending-reservations').textContent = stats.pendingCount;

    // Render reservations table
    state.reservations = reservations;
    renderAdminReservations();

    // Render floor plan
    renderAdminFloorPlan(fullRest.tables || []);

    // Render menu items
    renderAdminMenu(fullRest.menuItems || []);
  } catch (err) {
    console.error('Error cargando dashboard:', err);
  }
}

function renderAdminReservations() {
  const tbody = document.getElementById('admin-reservations-tbody');
  let list = state.reservations;

  if (state.reservationFilter !== 'Todos') {
    list = list.filter(r => r.status === state.reservationFilter);
  }

  if (state.reservationSearch) {
    list = list.filter(r => 
      r.customerName.toLowerCase().includes(state.reservationSearch) ||
      r.customerEmail.toLowerCase().includes(state.reservationSearch) ||
      r.reservationCode.toLowerCase().includes(state.reservationSearch) ||
      r.customerPhone.includes(state.reservationSearch)
    );
  }

  if (list.length === 0) {
    tbody.innerHTML = `<tr><td colspan="7" style="text-align: center; color: var(--text-muted); padding: 32px;">No hay reservas que coincidan con el filtro actual.</td></tr>`;
    return;
  }

  tbody.innerHTML = list.map(r => {
    const initials = r.customerName.split(' ').map(n => n[0]).join('').substring(0, 2).toUpperCase();
    return `
      <tr>
        <td>
          <span style="font-weight: 700; font-family: var(--font-heading); letter-spacing: 0.05em; color: #a5b4fc;">${r.reservationCode}</span>
        </td>
        <td>
          <div class="customer-cell">
            <div class="customer-avatar">${initials}</div>
            <div>
              <div class="customer-name">${r.customerName}</div>
              <div class="customer-contact">${r.customerEmail} &bull; ${r.customerPhone}</div>
            </div>
          </div>
        </td>
        <td>
          <div style="font-weight: 600;">${formatDate(r.reservationDate)}</div>
          <div style="color: var(--text-dim); font-size: 0.8rem;">⏰ ${r.reservationTime} hrs</div>
        </td>
        <td><strong>${r.partySize}</strong> personas</td>
        <td>
          ${r.table ? `<span style="font-weight: 600;">Mesa ${r.table.tableNumber}</span> <span style="font-size:0.75rem;color:var(--text-dim);">(${r.table.location})</span>` : '<span style="color:var(--text-dim);">Sin mesa</span>'}
        </td>
        <td>${formatStatusPill(r.status)}</td>
        <td style="text-align: right;">
          ${r.status === 'Pendiente' ? `
            <button class="btn btn-success btn-sm" onclick="window.approveReservation(${r.id})" title="Aprobar y Enviar Correo de Confirmación">
              ✓ Aprobar
            </button>
            <button class="btn btn-danger btn-sm" onclick="window.rejectReservation(${r.id})" title="Rechazar y Notificar">
              ✕ Rechazar
            </button>
          ` : `
            <span style="font-size: 0.8rem; color: var(--text-dim); font-style: italic;">Gestionada</span>
          `}
        </td>
      </tr>
    `;
  }).join('');
}

// Global actions for admin reservation approval and rejection
window.approveReservation = async (id) => {
  try {
    await api.updateReservationStatus(id, { status: 'Confirmada' });
    showToast('¡Reserva aprobada! Se generó y envió el correo de confirmación al cliente.', 'success');
    await loadAdminDashboard();
  } catch (err) {
    showToast(err.message, 'error');
  }
};

window.rejectReservation = async (id) => {
  const reason = prompt('Motivo del rechazo (opcional):', 'Aforo completo para el horario solicitado');
  if (reason === null) return;

  try {
    await api.updateReservationStatus(id, { status: 'Rechazada', adminNotes: reason });
    showToast('Reserva rechazada. Se notificó al cliente.', 'info');
    await loadAdminDashboard();
  } catch (err) {
    showToast(err.message, 'error');
  }
};

// Admin Floor Plan & Table Management
function renderAdminFloorPlan(tables) {
  const grid = document.getElementById('admin-tables-grid');
  if (!tables || tables.length === 0) {
    grid.innerHTML = `<div style="grid-column: 1/-1; text-align: center; color: var(--text-muted); padding: 30px;">No hay mesas registradas.</div>`;
    return;
  }

  grid.innerHTML = tables.map(t => {
    const statusClass = `status-${t.status.toLowerCase()}`;
    const pillClass = `pill-${t.status.toLowerCase()}`;
    return `
      <div class="table-slot ${statusClass}" onclick="window.cycleTableStatus(${t.id}, '${t.status}')" title="Haz clic para alternar estado">
        <div class="table-number">Mesa ${t.tableNumber}</div>
        <div class="table-cap">👥 Capacidad: ${t.capacity}</div>
        <div class="table-loc">${t.location}</div>
        <div class="table-status-pill ${pillClass}">${t.status}</div>
        <div style="font-size: 0.72rem; color: var(--text-dim); margin-top: 8px;">Clic para alternar</div>
      </div>
    `;
  }).join('');
}

window.cycleTableStatus = async (id, currentStatus) => {
  const nextStatus = currentStatus === 'Disponible' 
    ? 'Reservada' 
    : currentStatus === 'Reservada' 
      ? 'Ocupada' 
      : 'Disponible';

  try {
    await api.updateTableStatus(id, nextStatus);
    showToast(`Mesa actualizada a: ${nextStatus}`);
    await loadAdminDashboard();
  } catch (err) {
    showToast(err.message, 'error');
  }
};

async function handleCreateTableSubmit(e) {
  e.preventDefault();
  const dto = {
    restaurantId: state.adminRestaurantId,
    tableNumber: parseInt(document.getElementById('table-num').value),
    capacity: parseInt(document.getElementById('table-cap').value),
    location: document.getElementById('table-loc').value,
    status: document.getElementById('table-init-status').value
  };

  try {
    await api.createTable(dto);
    document.getElementById('modal-create-table').classList.remove('active');
    document.getElementById('form-create-table').reset();
    showToast('Nueva mesa añadida exitosamente');
    await loadAdminDashboard();
  } catch (err) {
    showToast(err.message, 'error');
  }
}

// Admin Menu Management
function renderAdminMenu(items) {
  const grid = document.getElementById('admin-menu-grid');
  if (!items || items.length === 0) {
    grid.innerHTML = `<div style="grid-column: 1/-1; text-align: center; color: var(--text-muted); padding: 30px;">No hay platillos registrados en la carta.</div>`;
    return;
  }

  grid.innerHTML = items.map(item => `
    <div class="menu-card">
      <img src="${item.imageUrl || 'https://images.unsplash.com/photo-1546069901-ba9599a7e63c?auto=format&fit=crop&w=600&q=80'}" alt="${item.name}" class="menu-img">
      <div class="menu-info">
        <div class="menu-header">
          <div class="menu-name">${item.name}</div>
          <div class="menu-price">${item.price.toFixed(2)} €</div>
        </div>
        <div class="menu-desc">${item.description}</div>
        <div class="menu-tags" style="display: flex; justify-content: space-between; align-items: center;">
          <span class="tag-pill">${item.category}</span>
          <button class="btn btn-outline btn-sm" style="color: var(--color-danger); border: none; padding: 2px 6px;" onclick="window.deleteMenuItem(${item.id})">
            🗑️ Eliminar
          </button>
        </div>
      </div>
    </div>
  `).join('');
}

window.deleteMenuItem = async (id) => {
  if (!confirm('¿Eliminar este platillo de la carta?')) return;
  try {
    await api.deleteMenuItem(id);
    showToast('Platillo eliminado');
    await loadAdminDashboard();
  } catch (err) {
    showToast(err.message, 'error');
  }
};

async function handleCreateMenuSubmit(e) {
  e.preventDefault();
  const dto = {
    restaurantId: state.adminRestaurantId,
    name: document.getElementById('menu-item-name').value.trim(),
    description: document.getElementById('menu-item-desc').value.trim(),
    price: parseFloat(document.getElementById('menu-item-price').value),
    category: document.getElementById('menu-item-cat').value,
    tags: document.getElementById('menu-item-tags').value.trim(),
    imageUrl: document.getElementById('menu-item-img').value.trim()
  };

  try {
    await api.createMenuItem(dto);
    document.getElementById('modal-create-menu').classList.remove('active');
    document.getElementById('form-create-menu').reset();
    showToast('Platillo añadido a la carta');
    await loadAdminDashboard();
  } catch (err) {
    showToast(err.message, 'error');
  }
}

// -------------------------------------------------------------
// NOTIFICATIONS & LIVE EMAIL VIEWER
// -------------------------------------------------------------
async function loadNotifications() {
  const container = document.getElementById('notifications-list');
  container.innerHTML = `<div style="text-align: center; padding: 20px; color: var(--text-muted);">Cargando notificaciones...</div>`;

  try {
    const logs = await api.getNotifications(state.adminRestaurantId);
    if (!logs || logs.length === 0) {
      container.innerHTML = `<div style="text-align: center; color: var(--text-muted); padding: 30px;">Aún no se han generado notificaciones por correo.</div>`;
      return;
    }

    container.innerHTML = logs.map(log => `
      <div class="glass-card" style="padding: 16px 20px; display: flex; justify-content: space-between; align-items: center; cursor: pointer; transition: var(--transition-fast);" onclick="window.previewEmailById(${log.id})">
        <div>
          <div style="font-weight: 600; font-size: 0.95rem; margin-bottom: 4px;">${log.subject}</div>
          <div style="font-size: 0.82rem; color: var(--text-muted);">
            Para: <strong>${log.recipientName}</strong> &lt;${log.recipientEmail}&gt; &bull; ${new Date(log.sentAt).toLocaleString('es-ES')}
          </div>
        </div>
        <div style="display: flex; align-items: center; gap: 12px;">
          <span class="table-status-pill pill-disponible" style="font-size: 0.72rem;">${log.status}</span>
          <button class="btn btn-secondary btn-sm">Ver Correo ↗</button>
        </div>
      </div>
    `).join('');

    // Cache logs for modal display
    window._lastNotificationLogs = logs;
  } catch (err) {
    showToast('Error cargando notificaciones: ' + err.message, 'error');
  }
}

window.previewEmailById = (id) => {
  const log = window._lastNotificationLogs?.find(l => l.id === id);
  if (!log) return;

  document.getElementById('email-preview-meta').textContent = `Destinatario: ${log.recipientName} <${log.recipientEmail}> • ${new Date(log.sentAt).toLocaleString('es-ES')}`;
  document.getElementById('email-preview-content').innerHTML = log.bodyHtml;
  document.getElementById('modal-email-preview').classList.add('active');
};
