import { useState, useEffect } from 'react';
import { FiCamera, FiCheckCircle, FiXCircle } from 'react-icons/fi';
import { trainingsApi } from '../../api/client';
import LoadingSpinner from '../../components/LoadingSpinner';

export default function QrScannerPage() {
  const [sessions, setSessions] = useState([]);
  const [selectedSessionId, setSelectedSessionId] = useState('');
  const [manualQrCode, setManualQrCode] = useState('');
  const [result, setResult] = useState(null);
  const [loading, setLoading] = useState(true);
  const [scanning, setScanning] = useState(false);

  useEffect(() => {
    trainingsApi.getAll()
      .then((res) => {
        const upcoming = res.data.filter((s) => s.status === 'Scheduled' || s.status === 'InProgress');
        setSessions(upcoming);
      })
      .catch(console.error)
      .finally(() => setLoading(false));
  }, []);

  const handleManualCheckIn = async () => {
    if (!selectedSessionId || !manualQrCode.trim()) {
      alert('Please select a session and enter a QR code');
      return;
    }
    setScanning(true);
    setResult(null);
    try {
      const res = await trainingsApi.checkIn(selectedSessionId, manualQrCode.trim());
      setResult(res.data);
    } catch (err) {
      setResult(err.response?.data || { success: false, message: 'Check-in failed' });
    } finally {
      setScanning(false);
    }
  };

  if (loading) return <LoadingSpinner />;

  return (
    <div className="page">
      <div className="page-header">
        <h1>QR Scanner Check-In</h1>
        <p className="page-subtitle">Scan player QR codes to check them into training sessions</p>
      </div>

      <div className="scanner-layout">
        <div className="card scanner-card">
          <div className="card-header"><h3><FiCamera /> Check-In Station</h3></div>
          <div className="card-body">
            <div className="form-group">
              <label>Select Training Session *</label>
              <select value={selectedSessionId} onChange={(e) => setSelectedSessionId(e.target.value)}>
                <option value="">Choose a session...</option>
                {sessions.map((s) => (
                  <option key={s.id} value={s.id}>{s.title} — {new Date(s.scheduledAt).toLocaleDateString()}</option>
                ))}
              </select>
            </div>

            <div className="form-group">
              <label>Player QR Code *</label>
              <input
                type="text"
                placeholder="Enter or scan QR code value..."
                value={manualQrCode}
                onChange={(e) => setManualQrCode(e.target.value)}
                onKeyDown={(e) => e.key === 'Enter' && handleManualCheckIn()}
              />
              <p className="text-sm text-muted mt-1">
                Use a barcode scanner or enter the QR code manually
              </p>
            </div>

            <button
              className="btn btn-primary btn-lg w-full"
              onClick={handleManualCheckIn}
              disabled={scanning || !selectedSessionId || !manualQrCode.trim()}
            >
              {scanning ? 'Checking In...' : 'Check In Player'}
            </button>
          </div>
        </div>

        {result && (
          <div className={`card result-card ${result.success ? 'result-success' : 'result-error'}`}>
            <div className="result-icon">
              {result.success ? <FiCheckCircle size={48} /> : <FiXCircle size={48} />}
            </div>
            <h3>{result.success ? 'Check-In Successful!' : 'Check-In Failed'}</h3>
            <p className="result-message">{result.message}</p>
            {result.playerName && <p className="result-player">{result.playerName}</p>}
            {result.trainingsRemaining !== null && result.trainingsRemaining !== undefined && (
              <p className="result-remaining">
                <strong>{result.trainingsRemaining}</strong> trainings remaining this month
              </p>
            )}
          </div>
        )}
      </div>
    </div>
  );
}
