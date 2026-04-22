import { useState, useEffect } from 'react';
import { FiPlus, FiTrash2, FiEye, FiSearch } from 'react-icons/fi';
import { tournamentsApi } from '../../api/client';
import { useAuth } from '../../context/AuthContext';
import Modal from '../../components/Modal';
import LoadingSpinner from '../../components/LoadingSpinner';
import TournamentForm from './TournamentForm';
import TournamentDetail from './TournamentDetail';
import { format } from 'date-fns';

export default function TournamentList() {
  const { isAdmin } = useAuth();
  const [tournaments, setTournaments] = useState([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [showCreate, setShowCreate] = useState(false);
  const [selectedTournament, setSelectedTournament] = useState(null);

  const load = () => {
    setLoading(true);
    tournamentsApi.getAll()
      .then((res) => setTournaments(res.data))
      .catch(console.error)
      .finally(() => setLoading(false));
  };

  useEffect(() => { load(); }, []);

  const handleDelete = async (id) => {
    if (!window.confirm('Delete this tournament?')) return;
    await tournamentsApi.delete(id);
    load();
  };

  const filtered = tournaments.filter((t) =>
    `${t.name} ${t.location} ${t.category} ${t.surface}`.toLowerCase().includes(search.toLowerCase())
  );

  const statusColor = (status) => {
    switch (status) {
      case 'Upcoming': return 'badge-info';
      case 'InProgress': return 'badge-warning';
      case 'Completed': return 'badge-success';
      case 'Cancelled': return 'badge-danger';
      default: return '';
    }
  };

  if (loading) return <LoadingSpinner />;

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1>Tournaments</h1>
          <p className="page-subtitle">{tournaments.length} tournaments</p>
        </div>
        {isAdmin && (
          <button className="btn btn-primary" onClick={() => setShowCreate(true)}>
            <FiPlus /> New Tournament
          </button>
        )}
      </div>

      <div className="search-bar">
        <FiSearch className="search-icon" />
        <input type="text" placeholder="Search tournaments..." value={search} onChange={(e) => setSearch(e.target.value)} />
      </div>

      <div className="card">
        <table className="table">
          <thead>
            <tr>
              <th>Name</th>
              <th>Dates</th>
              <th>Surface</th>
              <th>Format</th>
              <th>Players</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {filtered.length === 0 ? (
              <tr><td colSpan={7} className="empty-text">No tournaments found</td></tr>
            ) : (
              filtered.map((t) => (
                <tr key={t.id}>
                  <td>
                    <strong>{t.name}</strong>
                    {t.category && <span className="badge badge-level ml-2">{t.category}</span>}
                  </td>
                  <td>{format(new Date(t.startDate), 'MMM dd')} — {format(new Date(t.endDate), 'MMM dd, yyyy')}</td>
                  <td>{t.surface || '-'}</td>
                  <td>{t.format === 'SingleElimination' ? 'Elimination' : 'Round Robin'}</td>
                  <td>{t.currentParticipants}/{t.maxParticipants}</td>
                  <td><span className={`badge ${statusColor(t.status)}`}>{t.status}</span></td>
                  <td className="actions">
                    <button className="btn btn-sm btn-outline" onClick={() => setSelectedTournament(t)}>
                      <FiEye />
                    </button>
                    {isAdmin && (
                      <button className="btn btn-sm btn-danger" onClick={() => handleDelete(t.id)}>
                        <FiTrash2 />
                      </button>
                    )}
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      {isAdmin && (
        <Modal isOpen={showCreate} onClose={() => setShowCreate(false)} title="Create Tournament">
          <TournamentForm onSuccess={() => { setShowCreate(false); load(); }} />
        </Modal>
      )}

      <Modal isOpen={!!selectedTournament} onClose={() => setSelectedTournament(null)} title={selectedTournament?.name || 'Tournament'} size="large">
        {selectedTournament && <TournamentDetail tournament={selectedTournament} onUpdate={load} />}
      </Modal>
    </div>
  );
}
