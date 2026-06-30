import { Link, useParams } from 'react-router-dom';

import { getLegacyUrl } from '../app/config';
import { PulseRoute, routeStatusLabel } from '../app/routes';

type PageViewProps = {
  route: PulseRoute;
};

const statusClassName: Record<PulseRoute['status'], string> = {
  'ready-for-build': 'text-bg-primary',
  'api-backed': 'text-bg-success',
  'legacy-dependent': 'text-bg-warning'
};

export function PageView({ route }: PageViewProps) {
  const params = useParams();
  const paramEntries = Object.entries(params).filter(([, value]) => Boolean(value));

  return (
    <div className="content-stack">
      <section className="hero-panel">
        <div>
          <div className="page-eyebrow">{route.area} / {route.section}</div>
          <h1 className="page-title">{route.title}</h1>
          <p className="page-summary">{route.summary}</p>
        </div>
        <div className="hero-panel__actions">
          <span className={`badge ${statusClassName[route.status]}`}>{routeStatusLabel[route.status]}</span>
          <a className="btn btn-outline-light" href={getLegacyUrl(route.path)} target="_blank" rel="noreferrer">
            Open legacy page
          </a>
        </div>
      </section>

      <section className="panel-grid panel-grid--two">
        <article className="panel-card">
          <h2>Route context</h2>
          <div className="detail-list">
            <div>
              <span>Primary route</span>
              <strong>{route.path}</strong>
            </div>
            <div>
              <span>Aliases</span>
              <strong>{route.aliases?.join(', ') || 'None'}</strong>
            </div>
            <div>
              <span>Area</span>
              <strong>{route.area}</strong>
            </div>
            <div>
              <span>Section</span>
              <strong>{route.section}</strong>
            </div>
          </div>
        </article>

        <article className="panel-card">
          <h2>Migration notes</h2>
          <ul className="info-list">
            <li>Replace this placeholder with a feature module when the React implementation is ready.</li>
            <li>Keep the current URL shape unless a coordinated backend routing change is planned.</li>
            <li>Use the listed API resources first before adding new MVC-only endpoints.</li>
          </ul>
        </article>
      </section>

      {paramEntries.length > 0 ? (
        <section className="panel-card">
          <h2>Active route parameters</h2>
          <div className="token-list">
            {paramEntries.map(([key, value]) => (
              <span className="token-pill" key={key}>
                {key}: {value}
              </span>
            ))}
          </div>
        </section>
      ) : null}

      <section className="panel-grid panel-grid--two">
        <article className="panel-card">
          <h2>Backend resources</h2>
          {route.apiResources?.length ? (
            <div className="token-list">
              {route.apiResources.map((resource) => (
                <span className="token-pill token-pill--accent" key={resource}>
                  {resource}
                </span>
              ))}
            </div>
          ) : (
            <p className="muted-copy">No API resource has been mapped yet for this screen.</p>
          )}
        </article>

        <article className="panel-card">
          <h2>Next conversion step</h2>
          <p className="muted-copy">
            Start by extracting the data contract and interactions for this screen, then replace the placeholder with a feature slice under the same route.
          </p>
          <Link className="btn btn-dark" to="/">
            Return to migration dashboard
          </Link>
        </article>
      </section>
    </div>
  );
}