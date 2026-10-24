// API Client for Sistema de Reservas para Restaurantes

const API_BASE = '/api';

async function request(endpoint, options = {}) {
  const url = `${API_BASE}${endpoint}`;
  const config = {
    headers: {
      'Content-Type': 'application/json',
      ...options.headers,
    },
    ...options,
  };

  try {
    const response = await fetch(url, config);
    if (!response.ok) {
      let errorMsg = `Error HTTP ${response.status}`;
      try {
        const errorData = await response.json();
        errorMsg = errorData.message || (errorData.errors ? Object.values(errorData.errors).flat().join(', ') : errorMsg);
      } catch (_) {}
      throw new Error(errorMsg);
    }

    if (response.status === 204) {
      return null;
    }

    return await response.json();
  } catch (err) {
    console.error(`Error en llamada API [${endpoint}]:`, err);
    throw err;
  }
}

export const api = {
  // Restaurantes
  getRestaurants: () => request('/restaurants'),
  getRestaurant: (id) => request(`/restaurants/${id}`),
  createRestaurant: (data) => request('/restaurants', { method: 'POST', body: JSON.stringify(data) }),
  updateRestaurant: (id, data) => request(`/restaurants/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
  deleteRestaurant: (id) => request(`/restaurants/${id}`, { method: 'DELETE' }),

  // Mesas
  getTables: (restaurantId) => request(`/tables?restaurantId=${restaurantId}`),
  createTable: (data) => request('/tables', { method: 'POST', body: JSON.stringify(data) }),
  updateTable: (id, data) => request(`/tables/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
  updateTableStatus: (id, status) => request(`/tables/${id}/status`, { method: 'PUT', body: JSON.stringify({ status }) }),
  deleteTable: (id) => request(`/tables/${id}`, { method: 'DELETE' }),

  // Menú
  getMenu: (restaurantId) => request(`/menu?restaurantId=${restaurantId}`),
  createMenuItem: (data) => request('/menu', { method: 'POST', body: JSON.stringify(data) }),
  updateMenuItem: (id, data) => request(`/menu/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
  deleteMenuItem: (id) => request(`/menu/${id}`, { method: 'DELETE' }),

  // Reservas
  getReservations: (params = {}) => {
    const query = new URLSearchParams();
    if (params.restaurantId) query.append('restaurantId', params.restaurantId);
    if (params.status) query.append('status', params.status);
    if (params.date) query.append('date', params.date);
    if (params.search) query.append('search', params.search);
    return request(`/reservations?${query.toString()}`);
  },
  getReservation: (id) => request(`/reservations/${id}`),
  lookupReservation: (code, email) => {
    const query = new URLSearchParams();
    if (code) query.append('code', code);
    if (email) query.append('email', email);
    return request(`/reservations/lookup?${query.toString()}`);
  },
  createReservation: (data) => request('/reservations', { method: 'POST', body: JSON.stringify(data) }),
  updateReservationStatus: (id, data) => request(`/reservations/${id}/status`, { method: 'PUT', body: JSON.stringify(data) }),
  cancelReservation: (id) => request(`/reservations/${id}`, { method: 'DELETE' }),

  // Dashboard y Métricas
  getDashboardStats: (restaurantId) => request(`/dashboard/stats?restaurantId=${restaurantId}`),

  // Notificaciones y Correos
  getNotifications: (restaurantId) => request(`/notifications?restaurantId=${restaurantId || ''}`),
};
