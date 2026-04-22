import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { FiPlus, FiSearch, FiTrash2, FiEdit } from 'react-icons/fi';
import { playersApi } from '../../api/client';
import Modal from '../../components/Modal';
import LoadingSpinner from '../../components/LoadingSpinner';
import PlayerForm from './PlayerForm';

export default function PlayersList() {
  const [players, setPlayers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [showCreate, setShowCreate] = useState(false);

  const loadPlayers = () => {
    setLoading(true);
    playersApi.getAll()
      .then((res) => setPlayers(res.data))
      .catch(console.error)
      .finally(() => setLoading(false));
  };

  useEffect(() => { loadPlayers(); }, []);

  const handleDelete = async (id) => {
    if (!window.confirm('Are you sure you want to delete this player?')) return;
    await playersApi.delete(id);
    loadPlayers();
  };

  const filtered = players.filter((p) =>
    `${p.firstName} ${p.lastName} ${p.email}`.toLowerCase().includes(search.toLowerCase())
  );

  if (loading) return <LoadingSpinner />;

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1>Players</h1>
          <p className="page-subtitle">{players.length} total players</p>
        </div>
        <button className="btn btn-primary" onClick={() => setShowCreate(true)}>
          <FiPlus /> Add Player
        </button>
      </div>

      <div className="search-bar">
        <FiSearch className="search-icon" />
        <input
          type="text"
          placeholder="Search players..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
      </div>

      <div className="card">
        <table className="table">
          <thead>
            <tr>
              <th>Name</th>
              <th>Email</th>
              <th>Level</th>
              <th>Status</th>
              <th>Trainings Left</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {filtered.length === 0 ? (
              <tr><td colSpan={6} className="empty-text">No players found</td></tr>
            ) : (
              filtered.map((p) => (
                <tr key={p.id}>
                  <td>
                    <Link to={`/players/${p.id}`} className="text-link">
                      {p.firstName} {p.lastName}
                    </Link>
                  </td>
                  <td>{p.email}</td>
                  <td><span className="badge badge-level">{p.level || 'N/A'}</span></td>
                  <td>
                    <span className={`badge ${p.isActive ? 'badge-success' : 'badge-danger'}`}>
                      {p.isActive ? 'Active' : 'Inactive'}
                    </span>
                  </td>
                  <td>{p.trainingsRemaining ?? '-'}</td>
                  <td className="actions">
                    <Link to={`/players/${p.id}`} className="btn btn-sm btn-outline">
                      <FiEdit />
                    </Link>
                    <button className="btn btn-sm btn-danger" onClick={() => handleDelete(p.id)}>
                      <FiTrash2 />
                    </button>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      <Modal isOpen={showCreate} onClose={() => setShowCreate(false)} title="Add New Player">
        <PlayerForm onSuccess={() => { setShowCreate(false); loadPlayers(); }} />
      </Modal>
    </div>
  );
}
