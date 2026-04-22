import { useState } from 'react';
import { tournamentsApi } from '../../api/client';

export default function TournamentForm({ tournament, onSuccess }) {
  const [form, setForm] = useState({
    name: tournament?.name || '',
    description: tournament?.description || '',
    location: tournament?.location || '',
    startDate: tournament?.startDate ? new Date(tournament.startDate).toISOString().split('T')[0] : '',
    endDate: tournament?.endDate ? new Date(tournament.endDate).toISOString().split('T')[0] : '',
    maxParticipants: tournament?.maxParticipants || 16,
    category: tournament?.category || 'Singles',
    format: tournament?.format || 'SingleElimination',
    surface: tournament?.surface || 'Hard',
    setsToWin: tournament?.setsToWin || 2,
  });
  const [error, setError] = useState('');
  const [saving, setSaving] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setSaving(true);
    try {
      const payload = { ...form, maxParticipants: Number(form.maxParticipants), setsToWin: Number(form.setsToWin) };
      if (tournament) await tournamentsApi.update(tournament.id, payload);
      else await tournamentsApi.create(payload);
      onSuccess();
    } catch (err) {
      setError(err.response?.data?.title || 'Failed to save tournament');
    } finally {
      setSaving(false);
    }
  };

  const handleChange = (e) => setForm({ ...form, [e.target.name]: e.target.value });

  return (
    <form onSubmit={handleSubmit} className="form">
      {error && <div className="alert alert-danger">{error}</div>}

      <div className="form-group">
        <label>Tournament Name *</label>
        <input name="name" value={form.name} onChange={handleChange} required />
      </div>

      <div className="form-group">
        <label>Description</label>
        <textarea name="description" value={form.description} onChange={handleChange} rows={2} />
      </div>

      <div className="form-row">
        <div className="form-group">
          <label>Location</label>
          <input name="location" value={form.location} onChange={handleChange} />
        </div>
        <div className="form-group">
          <label>Surface</label>
          <select name="surface" value={form.surface} onChange={handleChange}>
            <option value="Hard">Hard Court</option>
            <option value="Clay">Clay</option>
            <option value="Grass">Grass</option>
            <option value="Indoor">Indoor</option>
          </select>
        </div>
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
          <label>Category</label>
          <select name="category" value={form.category} onChange={handleChange}>
            <option value="Singles">Singles</option>
            <option value="Doubles">Doubles</option>
            <option value="Mixed">Mixed Doubles</option>
          </select>
        </div>
        <div className="form-group">
          <label>Format</label>
          <select name="format" value={form.format} onChange={handleChange}>
            <option value="SingleElimination">Single Elimination</option>
            <option value="RoundRobin">Round Robin</option>
          </select>
        </div>
      </div>

      <div className="form-row">
        <div className="form-group">
          <label>Max Participants</label>
          <select name="maxParticipants" value={form.maxParticipants} onChange={handleChange}>
            {[4, 8, 16, 32, 64].map((n) => <option key={n} value={n}>{n} players</option>)}
          </select>
        </div>
        <div className="form-group">
          <label>Match Format</label>
          <select name="setsToWin" value={form.setsToWin} onChange={handleChange}>
            <option value={2}>Best of 3 sets</option>
            <option value={3}>Best of 5 sets</option>
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
