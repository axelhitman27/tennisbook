import { BrowserRouter, Routes, Route } from 'react-router-dom';
import Layout from './components/Layout';
import Dashboard from './pages/Dashboard/Dashboard';
import PlayersList from './pages/Players/PlayersList';
import PlayerDetail from './pages/Players/PlayerDetail';
import TrainingList from './pages/Training/TrainingList';
import TournamentList from './pages/Tournaments/TournamentList';
import ReportsPage from './pages/Reports/ReportsPage';
import QrScannerPage from './pages/QrScanner/QrScannerPage';

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Layout />}>
          <Route index element={<Dashboard />} />
          <Route path="players" element={<PlayersList />} />
          <Route path="players/:id" element={<PlayerDetail />} />
          <Route path="training" element={<TrainingList />} />
          <Route path="tournaments" element={<TournamentList />} />
          <Route path="reports" element={<ReportsPage />} />
          <Route path="qr-scanner" element={<QrScannerPage />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}
