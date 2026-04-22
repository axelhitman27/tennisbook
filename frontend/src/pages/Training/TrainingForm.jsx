import { useState } from 'react';
import { trainingsApi } from '../../api/client';

const DAYS = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];

export default function TrainingForm({ session, onSuccess }) {
  const [form, setForm] = useState({
    title: session?.title || '',
    description: session?.description || '',
    scheduledAt: session?.scheduledAt
      ? new Date(session.scheduledAt).toISOString().slice(0, 16)
      : '',
    durationMinutes: session?.durationMinutes || 60,
    courtName: session?.courtName || '',
    trainerName: session?.trainerName || '',
    maxParticipants: session?.maxParticipants || 10,
    isRecurring: session?.isRecurring || false,
    recurrenceDay: session?.recurrenceDay || '',
    recurrenceTime: session?.recurrenceTime || '',
  });
  const [error, setError] = useState('');
  const [saving, setSaving] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setSaving(true);
    try {
      const payload = {
        ...form,
        durationMinutes: Number(form.durationMinutes),
        maxParticipants: Number(form.maxParticipants),
        recurrenceDay: form.isRecurring && form.recurrenceDay !== ''
          ? Number(form.recurrenceDay)
          : null,
        recurrenceTime: form.isRecurring ? form.recurrenceTime || null : null,
      };
      if (session) {
        await trainingsApi.update(session.id, payload);
      } else {
        await trainingsApi.create(payload);
      }
      onSuccess();
    } catch (err) {
      setError(err.response?.data?.title || 'Failed to save session');
    } finally {
      setSaving(false);
    }
  };

  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    setForm({ ...form, [name]: type === 'checkbox' ? checked : value });
  };

  return (
    <form onSubmit={handleSubmit} className="form">
      {error && <div className="alert alert-danger">{error}</div>}

      <div className="form-group">
        <label>Title *</label>
        <input name="title" value={form.title} onChange={handleChange} required />
      </div>

      <div className="form-group">
        <label>Description</label>
        <textarea name="description" value={form.description} onChange={handleChange} rows={3} />
      </div>

      <div className="form-row">
        <div className="form-group">
          <label>Scheduled At *</label>
          <input name="scheduledAt" type="datetime-local" value={form.scheduledAt} onChange={handleChange} required />
        </div>
        <div className="form-group">
          <label>Duration (min)</label>
          <input name="durationMinutes" type="number" min="15" value={form.durationMinutes} onChange={handleChange} />
        </div>
      </div>

      <div className="form-row">
        <div className="form-group">
          <label>Court Name</label>
          <input name="courtName" value={form.courtName} onChange={handleChange} />
        </div>
        <div className="form-group">
          <label>Trainer Name</label>
          <input name="trainerName" value={form.trainerName} onChange={handleChange} />
        </div>
      </div>

      <div className="form-group">
        <label>Max Participants</label>
        <input name="maxParticipants" type="number" min="1" value={form.maxParticipants} onChange={handleChange} />
      </div>

      <div className="form-group checkbox-group">
        <label className="checkbox-label">
          <input name="isRecurring" type="checkbox" checked={form.isRecurring} onChange={handleChange} />
          <span>Recurring weekly session</span>
        </label>
      </div>

      {form.isRecurring && (
        <div className="form-row recurring-fields">
          <div className="form-group">
            <label>Day of Week *</label>
            <select name="recurrenceDay" value={form.recurrenceDay} onChange={handleChange} required>
              <option value="">Select day...</option>
              {DAYS.map((day, i) => (
                <option key={day} value={i}>{day}</option>
              ))}
            </select>
          </div>
          <div className="form-group">
            <label>Time *</label>
            <input name="recurrenceTime" type="time" value={form.recurrenceTime} onChange={handleChange} required />
          </div>
        </div>
      )}

      <div className="form-actions">
        <button type="submit" className="btn btn-primary" disabled={saving}>
          {saving ? 'Saving...' : (session ? 'Update Session' : 'Create Session')}
        </button>
      </div>
    </form>
  );
}
