import { useState } from 'react';
import { tournamentsApi } from '../../api/client';

export default function TournamentForm({ tournament, onSuccess }) {
  const [form, setForm] = useState({
    name: tournament?.name || '',
    description: tournament?.description || '',
    location: tournament?.location || '',
    startDate: tournament?.startDate
      ? new Date(tournament.startDate).toISOString().split('T')[0]
      : '',
    endDate: tournament?.endDate
      ? new Date(tournament.endDate).toISOString().split('T')[0]
      : '',
    maxParticipants: tournament?.maxParticipants || 32,
    category: tournament?.category || 'Singles',
  });
  const [error, setError] = useState('');
  const [saving, setSaving] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setSaving(true);
    try {
      const payload = { ...form, maxParticipants: Number(form.maxParticipants) };
      if (tournament) {
        await tournamentsApi.update(tournament.id, payload);
      } else {
        await tournamentsApi.create(payload);
      }
      onSuccess();
    } catch (err) {
      setError(err.response?.data?.title || 'Failed to save tournament');
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
        <label>Tournament Name *</label>
        <input name="name" value={form.name} onChange={handleChange} required />
      </div>

      <div className="form-group">
        <label>Description</label>
        <textarea name="description" value={form.description} onChange={handleChange} rows={3} />
      </div>

      <div className="form-group">
        <label>Location</label>
        <input name="location" value={form.location} onChange={handleChange} />
      </div>

      <div className="form-row">
        <div className="form-group">
          <label>Start Date *</label>
          <input name="startDate" type="date" value={form.startDate} onChange={handleChange} required />
        </div>
        <div className="form-group">
          <label>End Date *</label>
          <input name="endDate" type="date" value={form.endDate} onChange={handleChange} required />
        </div>
      </div>

      <div className="form-row">
        <div className="form-group">
          <label>Max Participants</label>
          <input name="maxParticipants" type="number" min="2" value={form.maxParticipants} onChange={handleChange} />
        </div>
        <div className="form-group">
          <label>Category</label>
          <select name="category" value={form.category} onChange={handleChange}>
            <option value="Singles">Singles</option>
            <option value="Doubles">Doubles</option>
            <option value="Mixed">Mixed Doubles</option>
          </select>
        </div>
      </div>

      <div className="form-actions">
        <button type="submit" className="btn btn-primary" disabled={saving}>
          {saving ? 'Saving...' : (tournament ? 'Update Tournament' : 'Create Tournament')}
        </button>
      </div>
    </form>
  );
}
