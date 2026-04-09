import { useState, useEffect } from 'react';
import { FiCamera, FiCheckCircle, FiXCircle, FiDollarSign } from 'react-icons/fi';
import { trainingsApi, paymentsApi } from '../../api/client';
import LoadingSpinner from '../../components/LoadingSpinner';
import Modal from '../../components/Modal';

export default function QrScannerPage() {
  const [sessions, setSessions] = useState([]);
  const [selectedSessionId, setSelectedSessionId] = useState('');
  const [manualQrCode, setManualQrCode] = useState('');
  const [result, setResult] = useState(null);
  const [loading, setLoading] = useState(true);
  const [scanning, setScanning] = useState(false);
  const [showPayment, setShowPayment] = useState(false);
  const [paymentAmount, setPaymentAmount] = useState('');
  const [paymentDesc, setPaymentDesc] = useState('');
  const [processingPayment, setProcessingPayment] = useState(false);

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
    setShowPayment(false);
    try {
      const res = await trainingsApi.checkIn(selectedSessionId, manualQrCode.trim());
      setResult(res.data);
      if (res.data.requiresPayment) {
        setShowPayment(true);
        setPaymentAmount('');
        setPaymentDesc('Training session payment');
      }
    } catch (err) {
      setResult(err.response?.data || { success: false, message: 'Check-in failed' });
    } finally {
      setScanning(false);
    }
  };

  const handlePayment = async () => {
    if (!paymentAmount || !result?.playerId) return;
    setProcessingPayment(true);
    try {
      const payment = await paymentsApi.create({
        playerId: result.playerId,
        trainingAttendanceId: result.attendanceId,
        amount: Number(paymentAmount),
        description: paymentDesc || 'Training session payment',
      });
      await paymentsApi.markPaid(payment.data.id);
      setShowPayment(false);
      setResult((prev) => ({
        ...prev,
        message: 'Check-in successful — payment recorded',
        requiresPayment: false,
      }));
    } catch (err) {
      alert('Failed to process payment');
    } finally {
      setProcessingPayment(false);
    }
  };

  const resetScan = () => {
    setManualQrCode('');
    setResult(null);
    setShowPayment(false);
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
            </div>

            <div className="form-actions" style={{ justifyContent: 'stretch', gap: '8px' }}>
              <button
                className="btn btn-primary btn-lg w-full"
                onClick={handleManualCheckIn}
                disabled={scanning || !selectedSessionId || !manualQrCode.trim()}
              >
                {scanning ? 'Checking In...' : 'Check In Player'}
              </button>
              {result && (
                <button className="btn btn-outline btn-lg" onClick={resetScan}>
                  Next Player
                </button>
              )}
            </div>
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
            {result.trainingsRemaining !== null && result.trainingsRemaining !== undefined && !result.requiresPayment && (
              <p className="result-remaining">
                <strong>{result.trainingsRemaining}</strong> trainings remaining this month
              </p>
            )}
            {result.requiresPayment && (
              <div className="payment-prompt">
                <FiDollarSign size={24} />
                <p>This player needs to pay for this session</p>
                <button className="btn btn-primary" onClick={() => setShowPayment(true)}>
                  Record Payment
                </button>
              </div>
            )}
          </div>
        )}
      </div>

      <Modal isOpen={showPayment} onClose={() => setShowPayment(false)} title="Record Session Payment">
        <div className="form">
          <div className="payment-header-info">
            <p><strong>Player:</strong> {result?.playerName}</p>
          </div>
          <div className="form-group">
            <label>Amount (EUR) *</label>
            <input
              type="number"
              min="0"
              step="0.01"
              value={paymentAmount}
              onChange={(e) => setPaymentAmount(e.target.value)}
              placeholder="e.g. 15.00"
              autoFocus
            />
          </div>
          <div className="form-group">
            <label>Description</label>
            <input
              type="text"
              value={paymentDesc}
              onChange={(e) => setPaymentDesc(e.target.value)}
              placeholder="Training session payment"
            />
          </div>
          <div className="form-actions">
            <button className="btn btn-outline" onClick={() => setShowPayment(false)}>Cancel</button>
            <button
              className="btn btn-primary"
              onClick={handlePayment}
              disabled={processingPayment || !paymentAmount}
            >
              {processingPayment ? 'Processing...' : `Confirm Payment €${paymentAmount || '0'}`}
            </button>
          </div>
        </div>
      </Modal>
    </div>
  );
}
