import { useState } from 'react';
import { playersApi } from '../../api/client';

export default function PlayerForm({ player, onSuccess }) {
  const [form, setForm] = useState({
    firstName: player?.firstName || '',
    lastName: player?.lastName || '',
    email: player?.email || '',
    phone: player?.phone || '',
    dateOfBirth: player?.dateOfBirth ? new Date(player.dateOfBirth).toISOString().split('T')[0] : '',
    level: player?.level || 'Beginner',
  });
  const [error, setError] = useState('');
  const [saving, setSaving] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setSaving(true);
    try {
      if (player) {
        await playersApi.update(player.id, form);
      } else {
        await playersApi.create(form);
      }
      onSuccess();
    } catch (err) {
      setError(err.response?.data?.title || 'Failed to save player');
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

      <div className="form-row">
        <div className="form-group">
          <label>First Name *</label>
          <input name="firstName" value={form.firstName} onChange={handleChange} required />
        </div>
        <div className="form-group">
          <label>Last Name *</label>
          <input name="lastName" value={form.lastName} onChange={handleChange} required />
        </div>
      </div>

      <div className="form-group">
        <label>Email *</label>
        <input name="email" type="email" value={form.email} onChange={handleChange} required />
      </div>

      <div className="form-row">
        <div className="form-group">
          <label>Phone</label>
          <input name="phone" value={form.phone} onChange={handleChange} />
        </div>
        <div className="form-group">
          <label>Date of Birth *</label>
          <input name="dateOfBirth" type="date" value={form.dateOfBirth} onChange={handleChange} required />
        </div>
      </div>

      <div className="form-group">
        <label>Level</label>
        <select name="level" value={form.level} onChange={handleChange}>
          <option value="Beginner">Beginner</option>
          <option value="Intermediate">Intermediate</option>
          <option value="Advanced">Advanced</option>
          <option value="Pro">Pro</option>
        </select>
      </div>

      <div className="form-actions">
        <button type="submit" className="btn btn-primary" disabled={saving}>
          {saving ? 'Saving...' : (player ? 'Update Player' : 'Create Player')}
        </button>
      </div>
    </form>
  );
}
