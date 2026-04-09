import { useState, useEffect } from 'react';
import { trainingsApi } from '../../api/client';
import { format } from 'date-fns';

export default function TrainingDetail({ session, onUpdate }) {
  const [attendances, setAttendances] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    trainingsApi.getAttendances(session.id)
      .then((res) => setAttendances(res.data))
      .catch(console.error)
      .finally(() => setLoading(false));
  }, [session.id]);

  return (
    <div>
      <div className="detail-info">
        <p><strong>Date:</strong> {format(new Date(session.scheduledAt), 'MMM dd, yyyy HH:mm')}</p>
        <p><strong>Duration:</strong> {session.durationMinutes} minutes</p>
        <p><strong>Court:</strong> {session.courtName || 'TBD'}</p>
        <p><strong>Trainer:</strong> {session.trainerName || 'TBD'}</p>
        <p><strong>Participants:</strong> {session.currentParticipants}/{session.maxParticipants}</p>
        {session.description && <p><strong>Description:</strong> {session.description}</p>}
      </div>

      <h4 className="mt-4">Attendances</h4>
      {loading ? (
        <p>Loading...</p>
      ) : attendances.length === 0 ? (
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
