import { useState, useEffect } from 'react';
import { FiSearch } from 'react-icons/fi';
import { playersApi, reportsApi } from '../../api/client';
import LoadingSpinner from '../../components/LoadingSpinner';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, PieChart, Pie, Cell } from 'recharts';
import { format } from 'date-fns';

const COLORS = ['#6366f1', '#22c55e', '#f59e0b', '#ec4899', '#14b8a6'];

export default function ReportsPage() {
  const [players, setPlayers] = useState([]);
  const [selectedPlayerId, setSelectedPlayerId] = useState('');
  const [report, setReport] = useState(null);
  const [loading, setLoading] = useState(true);
  const [reportLoading, setReportLoading] = useState(false);

  useEffect(() => {
    playersApi.getAll()
      .then((res) => setPlayers(res.data))
      .catch(console.error)
      .finally(() => setLoading(false));
  }, []);

  const loadReport = async (playerId) => {
    if (!playerId) { setReport(null); return; }
    setReportLoading(true);
    try {
      const res = await reportsApi.getPlayerReport(playerId);
      setReport(res.data);
    } catch (err) {
      console.error(err);
    } finally {
      setReportLoading(false);
    }
  };

  const handlePlayerChange = (e) => {
    setSelectedPlayerId(e.target.value);
    loadReport(e.target.value);
  };

  if (loading) return <LoadingSpinner />;

  const trainingChartData = report?.recentTrainings
    ?.reduce((acc, t) => {
      const month = format(new Date(t.scheduledAt), 'MMM yyyy');
      const existing = acc.find((a) => a.month === month);
      if (existing) existing.count++;
      else acc.push({ month, count: 1 });
      return acc;
    }, []) || [];

  const tournamentPieData = report?.tournamentHistory
    ?.reduce((acc, t) => {
      const result = t.result || 'Participated';
      const existing = acc.find((a) => a.name === result);
      if (existing) existing.value++;
      else acc.push({ name: result, value: 1 });
      return acc;
    }, []) || [];

  return (
    <div className="page">
      <div className="page-header">
        <h1>Player Reports</h1>
        <p className="page-subtitle">View detailed training and tournament analytics per player</p>
      </div>

      <div className="card">
        <div className="card-body">
          <div className="form-group">
            <label>Select Player</label>
            <select value={selectedPlayerId} onChange={handlePlayerChange}>
              <option value="">Choose a player...</option>
              {players.map((p) => (
                <option key={p.id} value={p.id}>{p.firstName} {p.lastName} — {p.level || 'N/A'}</option>
              ))}
            </select>
          </div>
        </div>
      </div>

      {reportLoading && <LoadingSpinner />}

      {report && !reportLoading && (
        <>
          <div className="stats-grid mt-4">
            <div className="stat-card">
              <div className="stat-info">
                <span className="stat-value">{report.totalTrainings}</span>
                <span className="stat-label">Total Trainings</span>
              </div>
            </div>
            <div className="stat-card">
              <div className="stat-info">
                <span className="stat-value">{report.trainingsThisMonth}</span>
                <span className="stat-label">This Month</span>
              </div>
            </div>
            <div className="stat-card">
              <div className="stat-info">
                <span className="stat-value">{report.totalTournaments}</span>
                <span className="stat-label">Tournaments</span>
              </div>
            </div>
            {report.activeSubscription && (
              <div className="stat-card">
                <div className="stat-info">
                  <span className="stat-value">{report.activeSubscription.trainingsRemaining}</span>
                  <span className="stat-label">Trainings Remaining</span>
                </div>
              </div>
            )}
          </div>

          <div className="charts-grid mt-4">
            {trainingChartData.length > 0 && (
              <div className="card">
                <div className="card-header"><h3>Training History</h3></div>
                <div className="card-body chart-container">
                  <ResponsiveContainer width="100%" height={300}>
                    <BarChart data={trainingChartData}>
                      <CartesianGrid strokeDasharray="3 3" />
                      <XAxis dataKey="month" />
                      <YAxis />
                      <Tooltip />
                      <Bar dataKey="count" fill="#6366f1" radius={[4, 4, 0, 0]} />
                    </BarChart>
                  </ResponsiveContainer>
                </div>
              </div>
            )}

            {tournamentPieData.length > 0 && (
              <div className="card">
                <div className="card-header"><h3>Tournament Results</h3></div>
                <div className="card-body chart-container">
                  <ResponsiveContainer width="100%" height={300}>
                    <PieChart>
                      <Pie data={tournamentPieData} cx="50%" cy="50%" outerRadius={100} dataKey="value" label={({ name, value }) => `${name}: ${value}`}>
                        {tournamentPieData.map((_, i) => (
                          <Cell key={i} fill={COLORS[i % COLORS.length]} />
                        ))}
                      </Pie>
                      <Tooltip />
                    </PieChart>
                  </ResponsiveContainer>
                </div>
              </div>
            )}
          </div>

          <div className="detail-grid mt-4">
            <div className="card">
              <div className="card-header"><h3>Recent Training Sessions</h3></div>
              <div className="card-body">
                {report.recentTrainings.length === 0 ? (
                  <p className="empty-text">No training records</p>
                ) : (
                  <table className="table">
                    <thead>
                      <tr><th>Training</th><th>Date</th><th>Court</th><th>Trainer</th></tr>
                    </thead>
                    <tbody>
                      {report.recentTrainings.map((t, i) => (
                        <tr key={i}>
                          <td>{t.title}</td>
                          <td>{format(new Date(t.scheduledAt), 'MMM dd, yyyy')}</td>
                          <td>{t.courtName || '-'}</td>
                          <td>{t.trainerName || '-'}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                )}
              </div>
            </div>

            <div className="card">
              <div className="card-header"><h3>Tournament History</h3></div>
              <div className="card-body">
                {report.tournamentHistory.length === 0 ? (
                  <p className="empty-text">No tournament records</p>
                ) : (
                  <table className="table">
                    <thead>
                      <tr><th>Tournament</th><th>Date</th><th>Category</th><th>Result</th><th>Placement</th></tr>
                    </thead>
                    <tbody>
                      {report.tournamentHistory.map((t, i) => (
                        <tr key={i}>
                          <td>{t.tournamentName}</td>
                          <td>{format(new Date(t.startDate), 'MMM dd, yyyy')}</td>
                          <td>{t.category || '-'}</td>
                          <td>{t.result || '-'}</td>
                          <td>{t.placement || '-'}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                )}
              </div>
            </div>
          </div>
        </>
      )}
    </div>
  );
}
