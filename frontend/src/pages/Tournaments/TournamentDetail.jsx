import { useState, useEffect } from 'react';
import { FiUserPlus } from 'react-icons/fi';
import { tournamentsApi, playersApi } from '../../api/client';
import { format } from 'date-fns';

export default function TournamentDetail({ tournament, onUpdate }) {
  const [participants, setParticipants] = useState([]);
  const [players, setPlayers] = useState([]);
  const [selectedPlayerId, setSelectedPlayerId] = useState('');
  const [loading, setLoading] = useState(true);
  const [registering, setRegistering] = useState(false);

  const loadData = async () => {
    setLoading(true);
    try {
      const [pRes, plRes] = await Promise.all([
        tournamentsApi.getParticipants(tournament.id),
        playersApi.getAll(),
      ]);
      setParticipants(pRes.data);
      setPlayers(plRes.data);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { loadData(); }, [tournament.id]);

  const handleRegister = async () => {
    if (!selectedPlayerId) return;
    setRegistering(true);
    try {
      await tournamentsApi.register({
        playerId: Number(selectedPlayerId),
        tournamentId: tournament.id,
      });
      await loadData();
      onUpdate();
      setSelectedPlayerId('');
    } catch (err) {
      alert(err.response?.data || 'Registration failed');
    } finally {
      setRegistering(false);
    }
  };

  const registeredIds = new Set(participants.map((p) => p.playerId));
  const availablePlayers = players.filter((p) => !registeredIds.has(p.id));

  return (
    <div>
      <div className="detail-info">
        <p><strong>Dates:</strong> {format(new Date(tournament.startDate), 'MMM dd, yyyy')} — {format(new Date(tournament.endDate), 'MMM dd, yyyy')}</p>
        <p><strong>Location:</strong> {tournament.location || 'TBD'}</p>
        <p><strong>Category:</strong> {tournament.category || 'N/A'}</p>
        <p><strong>Participants:</strong> {tournament.currentParticipants}/{tournament.maxParticipants}</p>
        {tournament.description && <p><strong>Description:</strong> {tournament.description}</p>}
      </div>

      <div className="register-section mt-4">
        <h4>Register Player</h4>
        <div className="form-row">
          <select value={selectedPlayerId} onChange={(e) => setSelectedPlayerId(e.target.value)} className="flex-grow">
            <option value="">Select a player...</option>
            {availablePlayers.map((p) => (
              <option key={p.id} value={p.id}>{p.firstName} {p.lastName}</option>
            ))}
          </select>
          <button className="btn btn-primary" onClick={handleRegister} disabled={!selectedPlayerId || registering}>
            <FiUserPlus /> Register
          </button>
        </div>
      </div>

      <h4 className="mt-4">Participants</h4>
      {loading ? (
        <p>Loading...</p>
      ) : participants.length === 0 ? (
        <p className="empty-text">No participants registered yet</p>
      ) : (
        <table className="table">
          <thead>
            <tr><th>Player</th><th>Registered</th><th>Result</th><th>Placement</th></tr>
          </thead>
          <tbody>
            {participants.map((p) => (
              <tr key={p.id}>
                <td>{p.playerName}</td>
                <td>{format(new Date(p.registeredAt), 'MMM dd, yyyy')}</td>
                <td>{p.result || '-'}</td>
                <td>{p.placement || '-'}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}
