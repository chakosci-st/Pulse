import { useMemo, useState } from 'react';
import { Link, NavLink, Outlet, useLocation, useNavigate } from 'react-router-dom';
import clsx from 'clsx';

import { navigationByArea } from '../app/menu';
import { appConfig } from '../app/config';
import { pulseRoutes, routeStatusLabel, shellAreas, type PulseArea } from '../app/routes';
import { useAuth } from '../auth/AuthProvider';

const inferArea = (pathname: string): PulseArea => {
  if (pathname.startsWith('/Admin')) {
    return 'Admin';
  }

  if (pathname.startsWith('/Templates')) {
    return 'Templates';
  }

  if (pathname.startsWith('/Sites')) {
    return 'Sites';
  }

  if (pathname.startsWith('/Projects')) {
    return 'Projects';
  }

  if (pathname.startsWith('/Settings')) {
    return 'Settings';
  }

  if (pathname.startsWith('/Error')) {
    return 'Error';
  }

  return 'Root';
};

const statusCounts = pulseRoutes.reduce<Record<string, number>>((accumulator, currentRoute) => {
  accumulator[currentRoute.status] = (accumulator[currentRoute.status] ?? 0) + 1;
  return accumulator;
}, {});

export function AppShell() {
  const location = useLocation();
  const navigate = useNavigate();
  const { error, hasModule, isAuthenticated, isLoading, refresh, user } = useAuth();
  const [isSidebarOpen, setIsSidebarOpen] = useState(true);
  const [searchValue, setSearchValue] = useState('');

  const activeArea = inferArea(location.pathname);
  const navigationItems = navigationByArea[activeArea] ?? navigationByArea.Root;
  const visibleNavigation = navigationItems.filter((item) => !item.moduleCodes || hasModule(...item.moduleCodes));

  const routeMatches = useMemo(() => {
    const normalizedQuery = searchValue.trim().toLowerCase();

    if (!normalizedQuery) {
      return [];
    }

    return pulseRoutes
      .filter((entry) => {
        const haystack = `${entry.title} ${entry.summary} ${entry.path} ${entry.area} ${entry.section}`.toLowerCase();
        return haystack.includes(normalizedQuery);
      })
      .slice(0, 8);
  }, [searchValue]);

  const initials = `${user?.firstName?.[0] ?? ''}${user?.lastName?.[0] ?? ''}`.toUpperCase() || 'PL';

  return (
    <div className={clsx('shell', { 'shell--collapsed': !isSidebarOpen })}>
      <aside className="shell-sidebar">
        <div className="brand-block">
          <div className="brand-block__logo">P</div>
          <div>
            <div className="brand-block__title">PULSE</div>
            <div className="brand-block__subtitle">React migration</div>
          </div>
        </div>

        <nav className="nav-stack">
          {visibleNavigation.map((item) => {
            if (item.children?.length) {
              const childItems = item.children.filter(Boolean);

              return (
                <div className="nav-group" key={item.title}>
                  <div className="nav-group__label">
                    <i className={`bi ${item.icon}`}></i>
                    <span>{item.title}</span>
                  </div>
                  <div className="nav-subgroup">
                    {childItems.map((child) => (
                      <NavLink
                        className={({ isActive }) => clsx('nav-subgroup__link', { 'is-active': isActive })}
                        key={child.to}
                        to={child.to}
                      >
                        {child.title}
                      </NavLink>
                    ))}
                  </div>
                </div>
              );
            }

            return (
              <NavLink className={({ isActive }) => clsx('nav-link-block', { 'is-active': isActive })} key={item.title} to={item.to ?? '/'}>
                <i className={`bi ${item.icon}`}></i>
                <span>{item.title}</span>
              </NavLink>
            );
          })}
        </nav>
      </aside>

      <div className="shell-main">
        <header className="topbar">
          <div className="topbar__left">
            <button className="icon-button" onClick={() => setIsSidebarOpen((current) => !current)} type="button">
              <i className="bi bi-list"></i>
            </button>
            <div>
              <div className="topbar__eyebrow">{appConfig.appName}</div>
              <div className="topbar__title">{activeArea === 'Root' ? 'Workspace' : activeArea}</div>
            </div>
          </div>

          <div className="topbar__center">
            <label className="route-search" htmlFor="routeSearch">
              <i className="bi bi-search"></i>
              <input
                id="routeSearch"
                onChange={(event) => setSearchValue(event.target.value)}
                placeholder="Search routes, screens, or modules"
                type="search"
                value={searchValue}
              />
            </label>
            {routeMatches.length > 0 ? (
              <div className="search-results">
                {routeMatches.map((match) => (
                  <button
                    className="search-results__item"
                    key={match.path}
                    onClick={() => {
                      navigate(match.path.replace(/:([A-Za-z]+)/g, 'sample'));
                      setSearchValue('');
                    }}
                    type="button"
                  >
                    <strong>{match.title}</strong>
                    <span>{match.path}</span>
                  </button>
                ))}
              </div>
            ) : null}
          </div>

          <div className="topbar__right">
            <button className="btn btn-outline-dark btn-sm" onClick={() => void refresh()} type="button">
              Refresh auth
            </button>
            <div className="profile-chip">
              <div className="profile-chip__avatar">{initials}</div>
              <div>
                <div className="profile-chip__name">{user?.displayName || 'Guest user'}</div>
                <div className="profile-chip__meta">{isAuthenticated ? user?.employeeId || 'Authenticated' : 'Not authenticated'}</div>
              </div>
            </div>
          </div>
        </header>

        <main className="workspace">
          <section className="status-strip">
            <div className="status-card">
              <span>Tracked areas</span>
              <strong>{shellAreas.length}</strong>
            </div>
            <div className="status-card">
              <span>Legacy screens mapped</span>
              <strong>{pulseRoutes.length}</strong>
            </div>
            <div className="status-card">
              <span>{routeStatusLabel['api-backed']}</span>
              <strong>{statusCounts['api-backed'] ?? 0}</strong>
            </div>
            <div className="status-card">
              <span>{routeStatusLabel['legacy-dependent']}</span>
              <strong>{statusCounts['legacy-dependent'] ?? 0}</strong>
            </div>
          </section>

          {!isLoading && !isAuthenticated ? (
            <section className="callout-banner callout-banner--warning">
              <div>
                <strong>Authentication is not initialized.</strong>
                <span>
                  Sign in through the existing Pulse.Web app first, or configure the auth proxy values so the React app can call `/Account/Me` and `/auth/token`.
                </span>
              </div>
              <Link className="btn btn-dark btn-sm" to="/Error/Unauthorized">
                View auth fallback
              </Link>
            </section>
          ) : null}

          {error ? (
            <section className="callout-banner callout-banner--danger">
              <div>
                <strong>Auth bootstrap failed.</strong>
                <span>{error}</span>
              </div>
            </section>
          ) : null}

          <Outlet />
        </main>
      </div>
    </div>
  );
}