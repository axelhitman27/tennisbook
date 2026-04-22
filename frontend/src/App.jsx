import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './context/AuthContext';
import Layout from './components/Layout';
import LoginPage from './pages/Auth/LoginPage';
import RegisterPage from './pages/Auth/RegisterPage';
import Dashboard from './pages/Dashboard/Dashboard';
import PlayersList from './pages/Players/PlayersList';
import PlayerDetail from './pages/Players/PlayerDetail';
import TrainingList from './pages/Training/TrainingList';
import TournamentList from './pages/Tournaments/TournamentList';
import ReportsPage from './pages/Reports/ReportsPage';
import QrScannerPage from './pages/QrScanner/QrScannerPage';
import MyProfilePage from './pages/MyProfile/MyProfilePage';

function RequireAuth({ children }) {
  const { isAuthenticated } = useAuth();
  return isAuthenticated ? children : <Navigate to="/login" replace />;
}

function RequireAdmin({ children }) {
  const { isAuthenticated, isAdmin } = useAuth();
  if (!isAuthenticated) return <Navigate to="/login" replace />;
  if (!isAdmin) return <Navigate to="/my-profile" replace />;
  return children;
}

function AppRoutes() {
  const { isAuthenticated, isAdmin } = useAuth();

  return (
    <Routes>
      <Route path="/login" element={isAuthenticated ? <Navigate to={isAdmin ? '/' : '/my-profile'} replace /> : <LoginPage />} />
      <Route path="/register" element={isAuthenticated ? <Navigate to="/my-profile" replace /> : <RegisterPage />} />

      <Route path="/" element={<RequireAuth><Layout /></RequireAuth>}>
        {/* Admin routes */}
        <Route index element={<RequireAdmin><Dashboard /></RequireAdmin>} />
        <Route path="players" element={<RequireAdmin><PlayersList /></RequireAdmin>} />
        <Route path="players/:id" element={<RequireAdmin><PlayerDetail /></RequireAdmin>} />
        <Route path="reports" element={<RequireAdmin><ReportsPage /></RequireAdmin>} />
        <Route path="qr-scanner" element={<RequireAdmin><QrScannerPage /></RequireAdmin>} />

        {/* Shared routes */}
        <Route path="training" element={<TrainingList />} />
        <Route path="tournaments" element={<TournamentList />} />

        {/* Player routes */}
        <Route path="my-profile" element={<MyProfilePage />} />
      </Route>

      <Route path="*" element={<Navigate to={isAuthenticated ? (isAdmin ? '/' : '/my-profile') : '/login'} replace />} />
    </Routes>
  );
}

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <AppRoutes />
      </AuthProvider>
    </BrowserRouter>
  );
}
