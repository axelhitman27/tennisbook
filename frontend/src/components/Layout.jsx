import { NavLink, Outlet } from 'react-router-dom';
import { FiHome, FiUsers, FiCalendar, FiAward, FiBarChart2, FiCamera } from 'react-icons/fi';

const navItems = [
  { to: '/', icon: <FiHome />, label: 'Dashboard' },
  { to: '/players', icon: <FiUsers />, label: 'Players' },
  { to: '/training', icon: <FiCalendar />, label: 'Training' },
  { to: '/tournaments', icon: <FiAward />, label: 'Tournaments' },
  { to: '/reports', icon: <FiBarChart2 />, label: 'Reports' },
  { to: '/qr-scanner', icon: <FiCamera />, label: 'QR Scanner' },
];

export default function Layout() {
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
              end={item.to === '/'}
            >
              <span className="nav-icon">{item.icon}</span>
              <span className="nav-label">{item.label}</span>
            </NavLink>
          ))}
        </nav>
      </aside>
      <main className="main-content">
        <Outlet />
      </main>
    </div>
  );
}
