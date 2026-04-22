import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { FiUsers, FiCalendar, FiAward, FiActivity } from 'react-icons/fi';
import { reportsApi } from '../../api/client';
import StatCard from '../../components/StatCard';
import LoadingSpinner from '../../components/LoadingSpinner';
import { format } from 'date-fns';

export default function Dashboard() {
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    reportsApi.getDashboard()
      .then((res) => setData(res.data))
      .catch(console.error)
      .finally(() => setLoading(false));
  }, []);

  if (loading) return <LoadingSpinner />;
  if (!data) return <p>Failed to load dashboard</p>;

  return (
    <div className="page">
      <div className="page-header">
        <h1>Dashboard</h1>
        <p className="page-subtitle">Tennis Club Overview</p>
      </div>

      <div className="stats-grid">
        <StatCard icon={<FiUsers size={24} />} label="Total Players" value={data.totalPlayers} color="#6366f1" />
        <StatCard icon={<FiActivity size={24} />} label="Active Players" value={data.activePlayers} color="#22c55e" />
        <StatCard icon={<FiCalendar size={24} />} label="Upcoming Trainings" value={data.upcomingTrainingSessions} color="#f59e0b" />
        <StatCard icon={<FiAward size={24} />} label="Upcoming Tournaments" value={data.upcomingTournaments} color="#ec4899" />
      </div>

      <div className="dashboard-grid">
        <div className="card">
          <div className="card-header">
            <h3>Recent Players</h3>
            <Link to="/players" className="btn btn-sm btn-outline">View All</Link>
          </div>
          <div className="card-body">
            {data.recentPlayers.length === 0 ? (
              <p className="empty-text">No players yet</p>
            ) : (
              <table className="table">
                <thead>
                  <tr>
                    <th>Name</th>
                    <th>Level</th>
                    <th>Status</th>
                    <th>Trainings Left</th>
                  </tr>
                </thead>
                <tbody>
                  {data.recentPlayers.map((p) => (
                    <tr key={p.id}>
                      <td>
                        <Link to={`/players/${p.id}`} className="text-link">
                          {p.firstName} {p.lastName}
                        </Link>
                      </td>
                      <td><span className="badge badge-level">{p.level || 'N/A'}</span></td>
                      <td><span className={`badge ${p.isActive ? 'badge-success' : 'badge-danger'}`}>{p.isActive ? 'Active' : 'Inactive'}</span></td>
                      <td>{p.trainingsRemaining ?? '-'}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>
        </div>

        <div className="card">
          <div className="card-header">
            <h3>Upcoming Trainings</h3>
            <Link to="/training" className="btn btn-sm btn-outline">View All</Link>
          </div>
          <div className="card-body">
            {data.upcomingTrainings.length === 0 ? (
              <p className="empty-text">No upcoming trainings</p>
            ) : (
              <ul className="list">
                {data.upcomingTrainings.map((t) => (
                  <li key={t.id} className="list-item">
                    <div>
                      <strong>{t.title}</strong>
                      <span className="text-muted"> — {t.courtName || 'TBD'}</span>
                    </div>
                    <div className="text-muted text-sm">
                      {format(new Date(t.scheduledAt), 'MMM dd, yyyy HH:mm')} · {t.currentParticipants}/{t.maxParticipants} spots
                    </div>
                  </li>
                ))}
              </ul>
            )}
          </div>
        </div>

        <div className="card">
          <div className="card-header">
            <h3>Upcoming Tournaments</h3>
            <Link to="/tournaments" className="btn btn-sm btn-outline">View All</Link>
          </div>
          <div className="card-body">
            {data.upcomingTournamentsList.length === 0 ? (
              <p className="empty-text">No upcoming tournaments</p>
            ) : (
              <ul className="list">
                {data.upcomingTournamentsList.map((t) => (
                  <li key={t.id} className="list-item">
                    <div>
                      <strong>{t.name}</strong>
                      {t.category && <span className="badge badge-info ml-2">{t.category}</span>}
                    </div>
                    <div className="text-muted text-sm">
                      {format(new Date(t.startDate), 'MMM dd')} - {format(new Date(t.endDate), 'MMM dd, yyyy')} · {t.location || 'TBD'}
                    </div>
                  </li>
                ))}
              </ul>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
