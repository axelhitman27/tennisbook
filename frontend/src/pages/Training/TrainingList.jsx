import { useState, useEffect } from 'react';
import { FiPlus, FiTrash2, FiEye, FiSearch } from 'react-icons/fi';
import { trainingsApi } from '../../api/client';
import Modal from '../../components/Modal';
import LoadingSpinner from '../../components/LoadingSpinner';
import TrainingForm from './TrainingForm';
import TrainingDetail from './TrainingDetail';
import { format } from 'date-fns';

export default function TrainingList() {
  const [sessions, setSessions] = useState([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [showCreate, setShowCreate] = useState(false);
  const [selectedSession, setSelectedSession] = useState(null);

  const load = () => {
    setLoading(true);
    trainingsApi.getAll()
      .then((res) => setSessions(res.data))
      .catch(console.error)
      .finally(() => setLoading(false));
  };

  useEffect(() => { load(); }, []);

  const handleDelete = async (id) => {
    if (!window.confirm('Delete this training session?')) return;
    await trainingsApi.delete(id);
    load();
  };

  const filtered = sessions.filter((s) =>
    `${s.title} ${s.courtName} ${s.trainerName}`.toLowerCase().includes(search.toLowerCase())
  );

  const statusColor = (status) => {
    switch (status) {
      case 'Scheduled': return 'badge-info';
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
          <h1>Training Sessions</h1>
          <p className="page-subtitle">{sessions.length} sessions</p>
        </div>
        <button className="btn btn-primary" onClick={() => setShowCreate(true)}>
          <FiPlus /> New Session
        </button>
      </div>

      <div className="search-bar">
        <FiSearch className="search-icon" />
        <input
          type="text"
          placeholder="Search sessions..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
      </div>

      <div className="card">
        <table className="table">
          <thead>
            <tr>
              <th>Title</th>
              <th>Date & Time</th>
              <th>Court</th>
              <th>Trainer</th>
              <th>Participants</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {filtered.length === 0 ? (
              <tr><td colSpan={7} className="empty-text">No sessions found</td></tr>
            ) : (
              filtered.map((s) => (
                <tr key={s.id}>
                  <td><strong>{s.title}</strong></td>
                  <td>{format(new Date(s.scheduledAt), 'MMM dd, yyyy HH:mm')}</td>
                  <td>{s.courtName || '-'}</td>
                  <td>{s.trainerName || '-'}</td>
                  <td>{s.currentParticipants}/{s.maxParticipants}</td>
                  <td><span className={`badge ${statusColor(s.status)}`}>{s.status}</span></td>
                  <td className="actions">
                    <button className="btn btn-sm btn-outline" onClick={() => setSelectedSession(s)}>
                      <FiEye />
                    </button>
                    <button className="btn btn-sm btn-danger" onClick={() => handleDelete(s.id)}>
                      <FiTrash2 />
                    </button>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      <Modal isOpen={showCreate} onClose={() => setShowCreate(false)} title="New Training Session">
        <TrainingForm onSuccess={() => { setShowCreate(false); load(); }} />
      </Modal>

      <Modal isOpen={!!selectedSession} onClose={() => setSelectedSession(null)} title="Session Details" size="large">
        {selectedSession && <TrainingDetail session={selectedSession} onUpdate={load} />}
      </Modal>
    </div>
  );
}
