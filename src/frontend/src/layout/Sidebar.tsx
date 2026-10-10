import { Link, useLocation } from 'react-router-dom';
import homeIcon from '../assets/home.svg';
import logo from '../assets/SUPERINTENDENCIA_DE_BANCOS.png';
import type { UserRole } from '../types/api';
import { isNavigationItemActive, visibleSections } from './navigation';

interface SidebarProps {
  role: UserRole | undefined;
  isOpen: boolean;
  onNavigate: () => void;
}

export function Sidebar({ role, isOpen, onNavigate }: SidebarProps) {
  const { pathname } = useLocation();

  return (
    <aside id="main-sidebar" className={`sidebar ${isOpen ? 'sidebar--open' : ''}`.trim()}>
      <img className="sidebar__logo" src={logo} alt="Superintendencia de Bancos - República Dominicana" />
      <nav aria-label="Menú principal">
        {visibleSections(role).map((section) => (
          <div key={section.title ?? 'inicio'} className="sidebar__section">
            {section.title && <p className="sidebar__section-title">{section.title}</p>}
            <ul className="sidebar__list">
              {section.items.map((item) => {
                const isActive = isNavigationItemActive(item, section, pathname);
                return (
                  <li key={item.to}>
                    <Link
                      to={item.to}
                      onClick={onNavigate}
                      aria-current={isActive ? 'page' : undefined}
                      className={['sidebar__link', item.isHome ? 'sidebar__link--home' : '', isActive ? 'active' : '']
                        .filter(Boolean)
                        .join(' ')}
                    >
                      {item.isHome && <img className="sidebar__icon" src={homeIcon} alt="" aria-hidden="true" />}
                      {item.label}
                    </Link>
                  </li>
                );
              })}
            </ul>
          </div>
        ))}
      </nav>
    </aside>
  );
}
