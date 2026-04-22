import { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { FiArrowLeft, FiEdit, FiCreditCard } from 'react-icons/fi';
import QRCode from 'react-qr-code';
import { playersApi, subscriptionsApi, reportsApi } from '../../api/client';
import Modal from '../../components/Modal';
import LoadingSpinner from '../../components/LoadingSpinner';
import PlayerForm from './PlayerForm';
import SubscriptionForm from './SubscriptionForm';
import { format } from 'date-fns';

export default function PlayerDetail() {
  const { id } = useParams();
  const [player, setPlayer] = useState(null);
  const [report, setReport] = useState(null);
  const [loading, setLoading] = useState(true);
  const [showEdit, setShowEdit] = useState(false);
  const [showSubscription, setShowSubscription] = useState(false);

  const load = async () => {
    setLoading(true);
    try {
      const [pRes, rRes] = await Promise.all([
        playersApi.getById(id),
        reportsApi.getPlayerReport(id),
      ]);
      setPlayer(pRes.data);
      setReport(rRes.data);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, [id]);

  if (loading) return <LoadingSpinner />;
  if (!player) return <p>Player not found</p>;

  const sub = player.activeSubscription;

  return (
    <div className="page">
      <Link to="/players" className="btn btn-outline mb-4">
        <FiArrowLeft /> Back to Players
      </Link>

      <div className="player-detail-header">
        <div className="player-info">
          <h1>{player.firstName} {player.lastName}</h1>
          <p className="text-muted">{player.email} {player.phone && `· ${player.phone}`}</p>
          <div className="player-meta">
            <span className="badge badge-level">{player.level || 'N/A'}</span>
            <span className={`badge ${player.isActive ? 'badge-success' : 'badge-danger'}`}>
              {player.isActive ? 'Active' : 'Inactive'}
            </span>
            {player.dateOfBirth && (
              <span className="text-muted">Born {format(new Date(player.dateOfBirth), 'MMM dd, yyyy')}</span>
            )}
          </div>
        </div>
        <div className="player-qr">
          <QRCode value={player.qrCode} size={120} />
          <p className="text-sm text-muted mt-2">Player QR Code</p>
        </div>
      </div>

      <div className="actions-bar">
        <button className="btn btn-outline" onClick={() => setShowEdit(true)}>
          <FiEdit /> Edit Player
        </button>
        <button className="btn btn-primary" onClick={() => setShowSubscription(true)}>
          <FiCreditCard /> {sub ? 'Update Subscription' : 'Add Subscription'}
        </button>
      </div>

      {sub && (
        <div className="card mt-4">
          <div className="card-header"><h3>Active Subscription</h3></div>
          <div className="card-body">
            <div className="subscription-grid">
              <div className="sub-stat">
                <span className="sub-stat-value">{sub.planName}</span>
                <span className="sub-stat-label">Plan</span>
              </div>
              <div className="sub-stat">
                <span className="sub-stat-value">{sub.trainingsPerWeek}</span>
                <span className="sub-stat-label">Per Week</span>
              </div>
              <div className="sub-stat">
                <span className="sub-stat-value highlight">{sub.trainingsRemaining}</span>
                <span className="sub-stat-label">Remaining This Month</span>
              </div>
              <div className="sub-stat">
                <span className="sub-stat-value">{sub.trainingsUsedThisMonth} / {sub.totalTrainingsPerMonth}</span>
                <span className="sub-stat-label">Used This Month</span>
              </div>
              <div className="sub-stat">
                <span className="sub-stat-value">€{sub.monthlyPrice}</span>
                <span className="sub-stat-label">Monthly Price</span>
              </div>
              <div className="sub-stat">
                <span className="sub-stat-value">€{sub.amountPaid}</span>
                <span className="sub-stat-label">Amount Paid</span>
              </div>
              <div className="sub-stat">
                <span className={`sub-stat-value ${sub.balanceDue > 0 ? 'text-danger' : 'text-success'}`}>
                  €{sub.balanceDue}
                </span>
                <span className="sub-stat-label">Balance Due</span>
              </div>
              <div className="sub-stat">
                <span className="sub-stat-value">{format(new Date(sub.renewalDate), 'MMM dd, yyyy')}</span>
                <span className="sub-stat-label">Renewal Date</span>
              </div>
            </div>
          </div>
        </div>
      )}

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
                <span className="stat-label">Tournaments</span>
              </div>
            </div>
          </div>

          <div className="detail-grid mt-4">
            <div className="card">
              <div className="card-header"><h3>Recent Trainings</h3></div>
              <div className="card-body">
                {report.recentTrainings.length === 0 ? (
                  <p className="empty-text">No training history</p>
                ) : (
                  <table className="table">
                    <thead>
                      <tr><th>Training</th><th>Date</th><th>Court</th><th>Trainer</th></tr>
                    </thead>
                    <tbody>
                      {report.recentTrainings.map((t) => (
                        <tr key={t.trainingSessionId}>
                          <td>{t.title}</td>
                          <td>{format(new Date(t.scheduledAt), 'MMM dd, yyyy HH:mm')}</td>
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
                  <p className="empty-text">No tournament history</p>
                ) : (
                  <table className="table">
                    <thead>
                      <tr><th>Tournament</th><th>Date</th><th>Category</th><th>Result</th></tr>
                    </thead>
                    <tbody>
                      {report.tournamentHistory.map((t) => (
                        <tr key={t.tournamentId}>
                          <td>{t.tournamentName}</td>
                          <td>{format(new Date(t.startDate), 'MMM dd, yyyy')}</td>
                          <td>{t.category || '-'}</td>
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

      <Modal isOpen={showEdit} onClose={() => setShowEdit(false)} title="Edit Player">
        <PlayerForm player={player} onSuccess={() => { setShowEdit(false); load(); }} />
      </Modal>

      <Modal isOpen={showSubscription} onClose={() => setShowSubscription(false)} title={sub ? 'Update Subscription' : 'Create Subscription'}>
        <SubscriptionForm playerId={player.id} subscription={sub} onSuccess={() => { setShowSubscription(false); load(); }} />
      </Modal>
    </div>
  );
}
