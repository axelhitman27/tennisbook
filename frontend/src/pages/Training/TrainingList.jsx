import { useState, useEffect } from 'react';
import { FiPlus, FiTrash2, FiEye, FiSearch, FiRepeat, FiUserPlus, FiUserMinus } from 'react-icons/fi';
import { trainingsApi } from '../../api/client';
import { useAuth } from '../../context/AuthContext';
import Modal from '../../components/Modal';
import LoadingSpinner from '../../components/LoadingSpinner';
import TrainingForm from './TrainingForm';
import TrainingDetail from './TrainingDetail';
import { format } from 'date-fns';

const DAYS = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];

export default function TrainingList() {
  const { isAdmin, user } = useAuth();
  const [sessions, setSessions] = useState([]);
  const [myEnrollments, setMyEnrollments] = useState([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [showCreate, setShowCreate] = useState(false);
  const [selectedSession, setSelectedSession] = useState(null);

  const load = async () => {
    setLoading(true);
    try {
      const res = await trainingsApi.getAll();
      setSessions(res.data);
      if (!isAdmin && user?.playerId) {
        const eRes = await trainingsApi.getMyEnrollments();
        setMyEnrollments(eRes.data);
      }
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, []);

  const handleDelete = async (id) => {
    if (!window.confirm('Delete this training session?')) return;
    await trainingsApi.delete(id);
    load();
  };

  const handleEnroll = async (sessionId) => {
    try {
      await trainingsApi.enroll(sessionId);
      load();
    } catch (err) {
      alert(err.response?.data?.message || 'Enrollment failed');
    }
  };

  const handleUnenroll = async (sessionId) => {
    const enrollment = myEnrollments.find((e) => e.trainingSessionId === sessionId);
    if (!enrollment) return;
    await trainingsApi.unenroll(enrollment.id);
    load();
  };

  const isEnrolled = (sessionId) => myEnrollments.some((e) => e.trainingSessionId === sessionId);

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
        {isAdmin && (
          <button className="btn btn-primary" onClick={() => setShowCreate(true)}>
            <FiPlus /> New Session
          </button>
        )}
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
              <th>Schedule</th>
              <th>Court</th>
              <th>Trainer</th>
              <th>Enrolled</th>
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
                  <td>
                    <strong>{s.title}</strong>
                    {s.isRecurring && (
                      <span className="badge badge-recurring ml-2">
                        <FiRepeat size={10} /> Weekly
                      </span>
                    )}
                  </td>
                  <td>
                    {s.isRecurring && s.recurrenceDay ? (
                      <span>{s.recurrenceDay}s at {s.recurrenceTime || '--:--'}</span>
                    ) : (
                      format(new Date(s.scheduledAt), 'MMM dd, yyyy HH:mm')
                    )}
                  </td>
                  <td>{s.courtName || '-'}</td>
                  <td>{s.trainerName || '-'}</td>
                  <td>{s.enrolledCount}/{s.maxParticipants}</td>
                  <td><span className={`badge ${statusColor(s.status)}`}>{s.status}</span></td>
                  <td className="actions">
                    {isAdmin ? (
                      <>
                        <button className="btn btn-sm btn-outline" onClick={() => setSelectedSession(s)}>
                          <FiEye />
                        </button>
                        <button className="btn btn-sm btn-danger" onClick={() => handleDelete(s.id)}>
                          <FiTrash2 />
                        </button>
                      </>
                    ) : (
                      isEnrolled(s.id) ? (
                        <button className="btn btn-sm btn-danger" onClick={() => handleUnenroll(s.id)}>
                          <FiUserMinus /> Unenroll
                        </button>
                      ) : (
                        <button
                          className="btn btn-sm btn-primary"
                          onClick={() => handleEnroll(s.id)}
                          disabled={s.enrolledCount >= s.maxParticipants}
                        >
                          <FiUserPlus /> Enroll
                        </button>
                      )
                    )}
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      {isAdmin && (
        <>
          <Modal isOpen={showCreate} onClose={() => setShowCreate(false)} title="New Training Session">
            <TrainingForm onSuccess={() => { setShowCreate(false); load(); }} />
          </Modal>

          <Modal isOpen={!!selectedSession} onClose={() => setSelectedSession(null)} title="Session Details" size="large">
            {selectedSession && <TrainingDetail session={selectedSession} onUpdate={load} />}
          </Modal>
        </>
      )}
    </div>
  );
}
