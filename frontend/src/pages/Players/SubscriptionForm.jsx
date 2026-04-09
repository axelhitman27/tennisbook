import { useState } from 'react';
import { subscriptionsApi } from '../../api/client';

export default function SubscriptionForm({ playerId, subscription, onSuccess }) {
  const [form, setForm] = useState({
    planName: subscription?.planName || 'Basic',
    trainingsPerWeek: subscription?.trainingsPerWeek || 2,
    totalTrainingsPerMonth: subscription?.totalTrainingsPerMonth || 8,
    monthlyPrice: subscription?.monthlyPrice || 50,
    amountPaid: subscription?.amountPaid || 0,
    startDate: subscription?.startDate
      ? new Date(subscription.startDate).toISOString().split('T')[0]
      : new Date().toISOString().split('T')[0],
  });
  const [error, setError] = useState('');
  const [saving, setSaving] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setSaving(true);
    try {
      if (subscription) {
        await subscriptionsApi.update(subscription.id, {
          planName: form.planName,
          trainingsPerWeek: Number(form.trainingsPerWeek),
          totalTrainingsPerMonth: Number(form.totalTrainingsPerMonth),
          monthlyPrice: Number(form.monthlyPrice),
          amountPaid: Number(form.amountPaid),
        });
      } else {
        await subscriptionsApi.create({
          playerId,
          planName: form.planName,
          trainingsPerWeek: Number(form.trainingsPerWeek),
          totalTrainingsPerMonth: Number(form.totalTrainingsPerMonth),
          monthlyPrice: Number(form.monthlyPrice),
          startDate: form.startDate,
        });
      }
      onSuccess();
    } catch (err) {
      setError(err.response?.data?.title || 'Failed to save subscription');
    } finally {
      setSaving(false);
    }
  };

  const handleChange = (e) => {
    setForm({ ...form, [e.target.name]: e.target.value });
  };

  return (
    <form onSubmit={handleSubmit} className="form">
      {error && <div className="alert alert-danger">{error}</div>}

      <div className="form-group">
        <label>Plan Name</label>
        <select name="planName" value={form.planName} onChange={handleChange}>
          <option value="Basic">Basic</option>
          <option value="Standard">Standard</option>
          <option value="Premium">Premium</option>
          <option value="Pro">Pro</option>
        </select>
      </div>

      <div className="form-row">
        <div className="form-group">
          <label>Trainings per Week</label>
          <input name="trainingsPerWeek" type="number" min="1" max="7" value={form.trainingsPerWeek} onChange={handleChange} />
        </div>
        <div className="form-group">
          <label>Total per Month</label>
          <input name="totalTrainingsPerMonth" type="number" min="1" max="31" value={form.totalTrainingsPerMonth} onChange={handleChange} />
        </div>
      </div>

      <div className="form-row">
        <div className="form-group">
          <label>Monthly Price (€)</label>
          <input name="monthlyPrice" type="number" min="0" step="0.01" value={form.monthlyPrice} onChange={handleChange} />
        </div>
        <div className="form-group">
          <label>Amount Paid (€)</label>
          <input name="amountPaid" type="number" min="0" step="0.01" value={form.amountPaid} onChange={handleChange} />
        </div>
      </div>

      {!subscription && (
        <div className="form-group">
          <label>Start Date</label>
          <input name="startDate" type="date" value={form.startDate} onChange={handleChange} />
        </div>
      )}

      <div className="form-actions">
        <button type="submit" className="btn btn-primary" disabled={saving}>
          {saving ? 'Saving...' : (subscription ? 'Update Subscription' : 'Create Subscription')}
        </button>
      </div>
    </form>
  );
}
