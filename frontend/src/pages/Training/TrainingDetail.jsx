import { useState, useEffect } from 'react';
import { FiRepeat, FiUserPlus, FiUserMinus } from 'react-icons/fi';
import { trainingsApi, playersApi } from '../../api/client';
import { format } from 'date-fns';

export default function TrainingDetail({ session, onUpdate }) {
  const [attendances, setAttendances] = useState([]);
  const [enrollments, setEnrollments] = useState([]);
  const [players, setPlayers] = useState([]);
  const [selectedPlayerId, setSelectedPlayerId] = useState('');
  const [loading, setLoading] = useState(true);

  const loadData = async () => {
    setLoading(true);
    try {
      const [aRes, eRes, pRes] = await Promise.all([
        trainingsApi.getAttendances(session.id),
        trainingsApi.getEnrollments(session.id),
        playersApi.getAll(),
      ]);
      setAttendances(aRes.data);
      setEnrollments(eRes.data);
      setPlayers(pRes.data);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { loadData(); }, [session.id]);

  const handleEnrollPlayer = async () => {
    if (!selectedPlayerId) return;
    try {
      await trainingsApi.enrollPlayer(session.id, Number(selectedPlayerId));
      setSelectedPlayerId('');
      loadData();
      onUpdate();
    } catch (err) {
      alert(err.response?.data?.message || 'Enrollment failed');
    }
  };

  const handleUnenroll = async (enrollmentId) => {
    await trainingsApi.unenroll(enrollmentId);
    loadData();
    onUpdate();
  };

  const enrolledIds = new Set(enrollments.map((e) => e.playerId));
  const availablePlayers = players.filter((p) => !enrolledIds.has(p.id));

  return (
    <div>
      <div className="detail-info">
        <p><strong>Date:</strong> {format(new Date(session.scheduledAt), 'MMM dd, yyyy HH:mm')}</p>
        <p><strong>Duration:</strong> {session.durationMinutes} minutes</p>
        <p><strong>Court:</strong> {session.courtName || 'TBD'}</p>
        <p><strong>Trainer:</strong> {session.trainerName || 'TBD'}</p>
        <p><strong>Capacity:</strong> {session.enrolledCount}/{session.maxParticipants} enrolled</p>
        {session.isRecurring && (
          <p>
            <span className="badge badge-recurring"><FiRepeat size={10} /> Recurring</span>
            {' '}{session.recurrenceDay}s at {session.recurrenceTime}
          </p>
        )}
        {session.description && <p><strong>Description:</strong> {session.description}</p>}
      </div>

      <div className="register-section mt-4">
        <h4>Enroll Player</h4>
        <div className="form-row">
          <select value={selectedPlayerId} onChange={(e) => setSelectedPlayerId(e.target.value)} className="flex-grow">
            <option value="">Select a player...</option>
            {availablePlayers.map((p) => (
              <option key={p.id} value={p.id}>{p.firstName} {p.lastName}</option>
            ))}
          </select>
          <button className="btn btn-primary" onClick={handleEnrollPlayer} disabled={!selectedPlayerId}>
            <FiUserPlus /> Enroll
          </button>
        </div>
      </div>

      <h4 className="mt-4">Enrolled Players ({enrollments.length})</h4>
      {loading ? (
        <p>Loading...</p>
      ) : enrollments.length === 0 ? (
        <p className="empty-text">No players enrolled yet</p>
      ) : (
        <table className="table">
          <thead>
            <tr><th>Player</th><th>Enrolled</th><th>Actions</th></tr>
          </thead>
          <tbody>
            {enrollments.map((e) => (
              <tr key={e.id}>
                <td>{e.playerName}</td>
                <td>{format(new Date(e.enrolledAt), 'MMM dd, yyyy')}</td>
                <td>
                  <button className="btn btn-sm btn-danger" onClick={() => handleUnenroll(e.id)}>
                    <FiUserMinus /> Remove
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}

      <h4 className="mt-4">Attendance History</h4>
      {attendances.length === 0 ? (
        <p className="empty-text">No attendances recorded yet</p>
      ) : (
        <table className="table">
          <thead>
            <tr><th>Player</th><th>Checked In</th><th>QR Scan</th></tr>
          </thead>
          <tbody>
            {attendances.map((a) => (
              <tr key={a.id}>
                <td>{a.playerName}</td>
                <td>{format(new Date(a.checkedInAt), 'MMM dd, yyyy HH:mm')}</td>
                <td><span className={`badge ${a.wasQrScanned ? 'badge-success' : 'badge-info'}`}>
                  {a.wasQrScanned ? 'QR Scanned' : 'Manual'}
                </span></td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}
