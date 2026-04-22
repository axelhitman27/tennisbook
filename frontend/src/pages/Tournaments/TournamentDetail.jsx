import { useState, useEffect } from 'react';
import { FiUserPlus, FiPlay, FiCheck } from 'react-icons/fi';
import { tournamentsApi, playersApi } from '../../api/client';
import { useAuth } from '../../context/AuthContext';
import Modal from '../../components/Modal';
import { format } from 'date-fns';

export default function TournamentDetail({ tournament, onUpdate }) {
  const { isAdmin } = useAuth();
  const [participants, setParticipants] = useState([]);
  const [matches, setMatches] = useState([]);
  const [players, setPlayers] = useState([]);
  const [selectedPlayerId, setSelectedPlayerId] = useState('');
  const [loading, setLoading] = useState(true);
  const [tab, setTab] = useState(tournament.drawGenerated ? 'bracket' : 'settings');
  const [scoreMatch, setScoreMatch] = useState(null);
  const [scoreForm, setScoreForm] = useState({ p1s1: 0, p2s1: 0, p1s2: 0, p2s2: 0, p1s3: 0, p2s3: 0 });
  const [savingScore, setSavingScore] = useState(false);

  const loadData = async () => {
    setLoading(true);
    try {
      const promises = [
        tournamentsApi.getParticipants(tournament.id),
        tournamentsApi.getMatches(tournament.id),
      ];
      if (isAdmin) promises.push(playersApi.getAll());
      const results = await Promise.all(promises);
      setParticipants(results[0].data);
      setMatches(results[1].data);
      if (isAdmin) setPlayers(results[2].data);
    } catch (err) { console.error(err); }
    finally { setLoading(false); }
  };

  useEffect(() => { loadData(); }, [tournament.id]);

  const handleRegister = async () => {
    if (!selectedPlayerId) return;
    try {
      await tournamentsApi.register({ playerId: Number(selectedPlayerId), tournamentId: tournament.id });
      setSelectedPlayerId('');
      loadData();
      onUpdate();
    } catch (err) { alert(err.response?.data || 'Failed'); }
  };

  const handleGenerateDraw = async () => {
    if (!window.confirm(`Generate draw for ${participants.length} players? This will start the tournament.`)) return;
    await tournamentsApi.generateDraw(tournament.id);
    loadData();
    onUpdate();
    setTab('bracket');
  };

  const openScoreModal = (match) => {
    setScoreMatch(match);
    setScoreForm({
      p1s1: match.player1Set1, p2s1: match.player2Set1,
      p1s2: match.player1Set2, p2s2: match.player2Set2,
      p1s3: match.player1Set3, p2s3: match.player2Set3,
    });
  };

  const handleSaveScore = async (winnerId) => {
    setSavingScore(true);
    try {
      await tournamentsApi.updateMatchScore(scoreMatch.id, {
        player1Set1: Number(scoreForm.p1s1), player2Set1: Number(scoreForm.p2s1),
        player1Set2: Number(scoreForm.p1s2), player2Set2: Number(scoreForm.p2s2),
        player1Set3: Number(scoreForm.p1s3), player2Set3: Number(scoreForm.p2s3),
        winnerId,
      });
      setScoreMatch(null);
      loadData();
      onUpdate();
    } catch (err) { alert('Failed to save'); }
    finally { setSavingScore(false); }
  };

  const registeredIds = new Set(participants.map((p) => p.playerId));
  const availablePlayers = players.filter((p) => !registeredIds.has(p.id));

  const rounds = [...new Set(matches.map((m) => m.round))].sort((a, b) => b - a);
  const roundName = (r) => {
    const total = rounds.length;
    const fromFinal = r;
    if (fromFinal === 1) return 'Final';
    if (fromFinal === 2) return 'Semi-Finals';
    if (fromFinal === 3) return 'Quarter-Finals';
    return `Round of ${Math.pow(2, fromFinal)}`;
  };

  return (
    <div>
      <div className="detail-info">
        <p><strong>Dates:</strong> {format(new Date(tournament.startDate), 'MMM dd')} — {format(new Date(tournament.endDate), 'MMM dd, yyyy')}</p>
        <p><strong>Location:</strong> {tournament.location || 'TBD'} {tournament.surface && `(${tournament.surface})`}</p>
        <p><strong>Format:</strong> {tournament.format === 'SingleElimination' ? 'Single Elimination' : 'Round Robin'} — Best of {tournament.setsToWin * 2 - 1} sets</p>
        <p><strong>Category:</strong> {tournament.category} — {tournament.currentParticipants}/{tournament.maxParticipants} players</p>
      </div>

      <div className="tab-bar mt-4">
        <button className={`tab ${tab === 'settings' ? 'active' : ''}`} onClick={() => setTab('settings')}>Players & Settings</button>
        <button className={`tab ${tab === 'bracket' ? 'active' : ''}`} onClick={() => setTab('bracket')}>Bracket & Matches</button>
      </div>

      {tab === 'settings' && (
        <div className="mt-4">
          {isAdmin && !tournament.drawGenerated && (
            <div className="register-section">
              <h4>Register Player</h4>
              <div className="form-row">
                <select value={selectedPlayerId} onChange={(e) => setSelectedPlayerId(e.target.value)} className="flex-grow">
                  <option value="">Select a player...</option>
                  {availablePlayers.map((p) => (
                    <option key={p.id} value={p.id}>{p.firstName} {p.lastName}</option>
                  ))}
                </select>
                <button className="btn btn-primary" onClick={handleRegister} disabled={!selectedPlayerId}>
                  <FiUserPlus /> Register
                </button>
              </div>
            </div>
          )}

          {isAdmin && !tournament.drawGenerated && participants.length >= 2 && (
            <div className="mt-4">
              <button className="btn btn-primary btn-lg" onClick={handleGenerateDraw}>
                <FiPlay /> Generate Draw & Start Tournament
              </button>
            </div>
          )}

          <h4 className="mt-4">Registered Players ({participants.length})</h4>
          {loading ? <p>Loading...</p> : participants.length === 0 ? (
            <p className="empty-text">No players registered yet</p>
          ) : (
            <div className="participants-grid">
              {participants.map((p, i) => (
                <div key={p.id} className="participant-card">
                  <span className="participant-seed">{i + 1}</span>
                  <span className="participant-name">{p.playerName}</span>
                  {p.result && <span className={`badge ${p.placement === 1 ? 'badge-success' : 'badge-info'}`}>{p.result}</span>}
                </div>
              ))}
            </div>
          )}
        </div>
      )}

      {tab === 'bracket' && (
        <div className="mt-4">
          {matches.length === 0 ? (
            <p className="empty-text">Draw not generated yet. Register players and click "Generate Draw".</p>
          ) : (
            <div className="bracket-container">
              {rounds.map((round) => (
                <div key={round} className="bracket-round">
                  <h4 className="round-title">{roundName(round)}</h4>
                  <div className="round-matches">
                    {matches.filter((m) => m.round === round).map((m) => (
                      <div key={m.id} className={`match-card ${m.status === 'Completed' ? 'match-completed' : ''} ${m.status === 'Walkover' ? 'match-walkover' : ''}`}>
                        <div className={`match-player ${m.winnerId === m.player1Id ? 'winner' : ''}`}>
                          <span className="player-name">{m.player1Name || 'TBD'}</span>
                          {m.status === 'Completed' && <span className="set-scores">{m.player1Set1} {m.player1Set2} {m.player1Set3 > 0 ? m.player1Set3 : ''}</span>}
                        </div>
                        <div className={`match-player ${m.winnerId === m.player2Id ? 'winner' : ''}`}>
                          <span className="player-name">{m.player2Name || 'TBD'}</span>
                          {m.status === 'Completed' && <span className="set-scores">{m.player2Set1} {m.player2Set2} {m.player2Set3 > 0 ? m.player2Set3 : ''}</span>}
                        </div>
                        {m.score && m.score !== 'BYE' && <div className="match-score">{m.score}</div>}
                        {m.score === 'BYE' && <div className="match-score bye">BYE</div>}
                        {isAdmin && m.player1Id && m.player2Id && m.status !== 'Completed' && m.status !== 'Walkover' && (
                          <button className="btn btn-sm btn-primary match-score-btn" onClick={() => openScoreModal(m)}>
                            Score
                          </button>
                        )}
                      </div>
                    ))}
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      )}

      <Modal isOpen={!!scoreMatch} onClose={() => setScoreMatch(null)} title="Enter Match Score">
        {scoreMatch && (
          <div className="form">
            <div className="score-header">
              <strong>{scoreMatch.player1Name}</strong> vs <strong>{scoreMatch.player2Name}</strong>
            </div>

            <table className="score-table">
              <thead>
                <tr><th>Player</th><th>Set 1</th><th>Set 2</th><th>Set 3</th></tr>
              </thead>
              <tbody>
                <tr>
                  <td>{scoreMatch.player1Name}</td>
                  <td><input type="number" min="0" max="7" value={scoreForm.p1s1} onChange={(e) => setScoreForm({ ...scoreForm, p1s1: e.target.value })} /></td>
                  <td><input type="number" min="0" max="7" value={scoreForm.p1s2} onChange={(e) => setScoreForm({ ...scoreForm, p1s2: e.target.value })} /></td>
                  <td><input type="number" min="0" max="7" value={scoreForm.p1s3} onChange={(e) => setScoreForm({ ...scoreForm, p1s3: e.target.value })} /></td>
                </tr>
                <tr>
                  <td>{scoreMatch.player2Name}</td>
                  <td><input type="number" min="0" max="7" value={scoreForm.p2s1} onChange={(e) => setScoreForm({ ...scoreForm, p2s1: e.target.value })} /></td>
                  <td><input type="number" min="0" max="7" value={scoreForm.p2s2} onChange={(e) => setScoreForm({ ...scoreForm, p2s2: e.target.value })} /></td>
                  <td><input type="number" min="0" max="7" value={scoreForm.p2s3} onChange={(e) => setScoreForm({ ...scoreForm, p2s3: e.target.value })} /></td>
                </tr>
              </tbody>
            </table>

            <p className="text-sm text-muted mt-2">Select the winner after entering the score:</p>
            <div className="form-actions">
              <button className="btn btn-outline" onClick={() => setScoreMatch(null)}>Cancel</button>
              <button className="btn btn-primary" disabled={savingScore} onClick={() => handleSaveScore(scoreMatch.player1Id)}>
                <FiCheck /> {scoreMatch.player1Name} Wins
              </button>
              <button className="btn btn-primary" disabled={savingScore} onClick={() => handleSaveScore(scoreMatch.player2Id)}>
                <FiCheck /> {scoreMatch.player2Name} Wins
              </button>
            </div>
          </div>
        )}
      </Modal>
    </div>
  );
}
