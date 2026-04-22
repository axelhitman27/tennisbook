import axios from 'axios';

const api = axios.create({
  baseURL: 'http://localhost:5191/api',
  headers: { 'Content-Type': 'application/json' },
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      if (window.location.pathname !== '/login') {
        window.location.href = '/login';
      }
    }
    return Promise.reject(error);
  }
);

export const authApi = {
  login: (data) => api.post('/auth/login', data),
  register: (data) => api.post('/auth/register', data),
};

export const playersApi = {
  getAll: () => api.get('/players'),
  getById: (id) => api.get(`/players/${id}`),
  getMe: () => api.get('/players/me'),
  create: (data) => api.post('/players', data),
  update: (id, data) => api.put(`/players/${id}`, data),
  delete: (id) => api.delete(`/players/${id}`),
  getByQr: (qrCode) => api.get(`/players/qr/${qrCode}`),
  getQrImageUrl: (id) => `http://localhost:5191/api/players/${id}/qr-image`,
};

export const subscriptionsApi = {
  getAll: () => api.get('/subscriptions'),
  getById: (id) => api.get(`/subscriptions/${id}`),
  getByPlayer: (playerId) => api.get(`/subscriptions/player/${playerId}`),
  create: (data) => api.post('/subscriptions', data),
  update: (id, data) => api.put(`/subscriptions/${id}`, data),
};

export const trainingsApi = {
  getAll: () => api.get('/trainings'),
  getById: (id) => api.get(`/trainings/${id}`),
  create: (data) => api.post('/trainings', data),
  update: (id, data) => api.put(`/trainings/${id}`, data),
  delete: (id) => api.delete(`/trainings/${id}`),
  checkIn: (sessionId, qrCode) => api.post(`/trainings/${sessionId}/check-in?qrCode=${qrCode}`),
  getAttendances: (sessionId) => api.get(`/trainings/${sessionId}/attendances`),
  enroll: (sessionId) => api.post(`/trainings/${sessionId}/enroll`),
  enrollPlayer: (sessionId, playerId) => api.post(`/trainings/${sessionId}/enroll-player`, { playerId, trainingSessionId: sessionId }),
  unenroll: (enrollmentId) => api.delete(`/trainings/enrollment/${enrollmentId}`),
  getEnrollments: (sessionId) => api.get(`/trainings/${sessionId}/enrollments`),
  getMyEnrollments: () => api.get('/trainings/my-enrollments'),
};

export const tournamentsApi = {
  getAll: () => api.get('/tournaments'),
  getById: (id) => api.get(`/tournaments/${id}`),
  create: (data) => api.post('/tournaments', data),
  update: (id, data) => api.put(`/tournaments/${id}`, data),
  delete: (id) => api.delete(`/tournaments/${id}`),
  register: (data) => api.post('/tournaments/register', data),
  getParticipants: (id) => api.get(`/tournaments/${id}/participants`),
  updateResult: (participationId, data) => api.put(`/tournaments/participation/${participationId}`, data),
  generateDraw: (id) => api.post(`/tournaments/${id}/generate-draw`),
  getMatches: (id) => api.get(`/tournaments/${id}/matches`),
  updateMatchScore: (matchId, data) => api.put(`/tournaments/matches/${matchId}/score`, data),
};

export const reportsApi = {
  getPlayerReport: (playerId) => api.get(`/reports/player/${playerId}`),
  getDashboard: () => api.get('/reports/dashboard'),
};

export const paymentsApi = {
  create: (data) => api.post('/payments', data),
  markPaid: (id) => api.put(`/payments/${id}/pay`),
  getByPlayer: (playerId) => api.get(`/payments/player/${playerId}`),
  getPending: () => api.get('/payments/pending'),
};

export default api;
