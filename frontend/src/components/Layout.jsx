import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { FiHome, FiUsers, FiCalendar, FiAward, FiBarChart2, FiCamera, FiUser, FiLogOut } from 'react-icons/fi';
import { useAuth } from '../context/AuthContext';

const adminNav = [
  { to: '/', icon: <FiHome />, label: 'Dashboard' },
  { to: '/players', icon: <FiUsers />, label: 'Players' },
  { to: '/training', icon: <FiCalendar />, label: 'Training' },
  { to: '/tournaments', icon: <FiAward />, label: 'Tournaments' },
  { to: '/reports', icon: <FiBarChart2 />, label: 'Reports' },
  { to: '/qr-scanner', icon: <FiCamera />, label: 'QR Scanner' },
];

const playerNav = [
  { to: '/my-profile', icon: <FiUser />, label: 'My Profile' },
  { to: '/training', icon: <FiCalendar />, label: 'Training Schedule' },
  { to: '/tournaments', icon: <FiAward />, label: 'Tournaments' },
];

export default function Layout() {
  const { user, isAdmin, logout } = useAuth();
  const navigate = useNavigate();
  const navItems = isAdmin ? adminNav : playerNav;

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <div className="app-layout">
      <aside className="sidebar">
        <div className="sidebar-brand">
          <span className="brand-icon">🎾</span>
          <h1>Tennisbook</h1>
        </div>
        <nav className="sidebar-nav">
          {navItems.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              className={({ isActive }) => `nav-item ${isActive ? 'active' : ''}`}
              end={item.to === '/' || item.to === '/my-profile'}
            >
              <span className="nav-icon">{item.icon}</span>
              <span className="nav-label">{item.label}</span>
            </NavLink>
          ))}
        </nav>
        <div className="sidebar-footer">
          <div className="sidebar-user">
            <span className="nav-label">{user?.email}</span>
            <span className="badge badge-role">{user?.role}</span>
          </div>
          <button className="nav-item logout-btn" onClick={handleLogout}>
            <span className="nav-icon"><FiLogOut /></span>
            <span className="nav-label">Logout</span>
          </button>
        </div>
      </aside>
      <main className="main-content">
        <Outlet />
      </main>
    </div>
  );
}
