import { createBrowserRouter, Navigate } from 'react-router-dom';

import { PageView } from '../components/PageView';
import { AppShell } from '../layout/AppShell';
import { pulseRoutes, type PulseRoute } from './routes';

const buildRouteEntries = (route: PulseRoute) => {
  const entries = [
    {
      path: route.path === '/' ? undefined : route.path.slice(1),
      element: <PageView route={route} />
    }
  ];

  if (route.aliases?.length) {
    for (const alias of route.aliases) {
      entries.push({
        path: alias.slice(1),
        element: <Navigate replace to={route.path.replace(/:([A-Za-z]+)/g, 'sample')} />
      });
    }
  }

  return entries;
};

const childRoutes = pulseRoutes.flatMap(buildRouteEntries);

export const router = createBrowserRouter([
  {
    path: '/',
    element: <AppShell />,
    children: [
      {
        index: true,
        element: <PageView route={pulseRoutes[0]} />
      },
      ...childRoutes.filter((entry) => entry.path !== undefined),
      {
        path: '*',
        element: <Navigate replace to="/Error/NotFound" />
      }
    ]
  }
]);