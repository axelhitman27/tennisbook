import { useState, useEffect } from 'react';
import QRCode from 'react-qr-code';
import { FiCalendar, FiAward, FiCreditCard } from 'react-icons/fi';
import { playersApi, reportsApi, paymentsApi } from '../../api/client';
import { useAuth } from '../../context/AuthContext';
import LoadingSpinner from '../../components/LoadingSpinner';
import { format } from 'date-fns';

export default function MyProfilePage() {
  const { user } = useAuth();
  const [player, setPlayer] = useState(null);
  const [report, setReport] = useState(null);
  const [payments, setPayments] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const load = async () => {
      try {
        const [pRes, rRes, pmRes] = await Promise.all([
          playersApi.getMe(),
          reportsApi.getPlayerReport(user.playerId),
          paymentsApi.getByPlayer(user.playerId),
        ]);
        setPlayer(pRes.data);
        setReport(rRes.data);
        setPayments(pmRes.data);
      } catch (err) {
        console.error(err);
      } finally {
        setLoading(false);
      }
    };
    load();
  }, [user.playerId]);

  if (loading) return <LoadingSpinner />;
  if (!player) return <p>Profile not found</p>;

  const sub = player.activeSubscription;

  return (
    <div className="page">
      <div className="page-header">
        <h1>My Profile</h1>
      </div>

      <div className="profile-layout">
        <div className="card profile-card">
          <div className="card-body profile-body">
            <div className="profile-qr-section">
              <QRCode value={player.qrCode} size={180} />
              <p className="text-sm text-muted mt-2">Show this QR code to your trainer at each session</p>
            </div>
            <div className="profile-info-section">
              <h2>{player.firstName} {player.lastName}</h2>
              <p className="text-muted">{player.email}</p>
              {player.phone && <p className="text-muted">{player.phone}</p>}
              <div className="player-meta mt-2">
                <span className="badge badge-level">{player.level || 'N/A'}</span>
                <span className={`badge ${player.isActive ? 'badge-success' : 'badge-danger'}`}>
                  {player.isActive ? 'Active' : 'Inactive'}
                </span>
              </div>
            </div>
          </div>
        </div>

        {sub && (
          <div className="card">
            <div className="card-header"><h3><FiCreditCard /> My Subscription</h3></div>
            <div className="card-body">
              <div className="subscription-grid">
                <div className="sub-stat">
                  <span className="sub-stat-value">{sub.planName}</span>
                  <span className="sub-stat-label">Plan</span>
                </div>
                <div className="sub-stat">
                  <span className="sub-stat-value">{sub.trainingsPerWeek}/week</span>
                  <span className="sub-stat-label">Trainings</span>
                </div>
                <div className="sub-stat">
                  <span className="sub-stat-value highlight">{sub.trainingsRemaining}</span>
                  <span className="sub-stat-label">Remaining This Month</span>
                </div>
                <div className="sub-stat">
                  <span className="sub-stat-value">{sub.trainingsUsedThisMonth}/{sub.totalTrainingsPerMonth}</span>
                  <span className="sub-stat-label">Used This Month</span>
                </div>
                <div className="sub-stat">
                  <span className="sub-stat-value">{format(new Date(sub.renewalDate), 'MMM dd, yyyy')}</span>
                  <span className="sub-stat-label">Renewal Date</span>
                </div>
                <div className="sub-stat">
                  <span className={`sub-stat-value ${sub.balanceDue > 0 ? 'text-danger' : 'text-success'}`}>
                    &euro;{sub.balanceDue}
                  </span>
                  <span className="sub-stat-label">Balance Due</span>
                </div>
              </div>
            </div>
          </div>
        )}
      </div>

      {report && (
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
                <span className="stat-label">Tournaments Played</span>
              </div>
            </div>
          </div>

          <div className="detail-grid mt-4">
            <div className="card">
              <div className="card-header"><h3><FiCalendar /> Recent Trainings</h3></div>
              <div className="card-body">
                {report.recentTrainings.length === 0 ? (
                  <p className="empty-text">No training history yet</p>
                ) : (
                  <table className="table">
                    <thead><tr><th>Training</th><th>Date</th><th>Court</th></tr></thead>
                    <tbody>
                      {report.recentTrainings.map((t, i) => (
                        <tr key={i}>
                          <td>{t.title}</td>
                          <td>{format(new Date(t.scheduledAt), 'MMM dd, yyyy HH:mm')}</td>
                          <td>{t.courtName || '-'}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                )}
              </div>
            </div>

            <div className="card">
              <div className="card-header"><h3><FiAward /> Tournament History</h3></div>
              <div className="card-body">
                {report.tournamentHistory.length === 0 ? (
                  <p className="empty-text">No tournament history yet</p>
                ) : (
                  <table className="table">
                    <thead><tr><th>Tournament</th><th>Date</th><th>Result</th></tr></thead>
                    <tbody>
                      {report.tournamentHistory.map((t, i) => (
                        <tr key={i}>
                          <td>{t.tournamentName}</td>
                          <td>{format(new Date(t.startDate), 'MMM dd, yyyy')}</td>
                          <td>{t.result || '-'}</td>
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

      {payments.length > 0 && (
        <div className="card mt-4">
          <div className="card-header"><h3>Payment History</h3></div>
          <div className="card-body">
            <table className="table">
              <thead><tr><th>Date</th><th>Amount</th><th>Description</th><th>Status</th></tr></thead>
              <tbody>
                {payments.map((p) => (
                  <tr key={p.id}>
                    <td>{format(new Date(p.createdAt), 'MMM dd, yyyy')}</td>
                    <td>&euro;{p.amount}</td>
                    <td>{p.description || p.trainingTitle || '-'}</td>
                    <td>
                      <span className={`badge ${p.isPaid ? 'badge-success' : 'badge-warning'}`}>
                        {p.isPaid ? 'Paid' : 'Pending'}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
}
