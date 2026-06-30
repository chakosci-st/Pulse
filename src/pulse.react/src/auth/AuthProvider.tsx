import {
  createContext,
  PropsWithChildren,
  useContext,
  useEffect,
  useMemo,
  useState
} from 'react';

import { appConfig, getAuthUrl } from '../app/config';
import { authStorageKey } from '../services/apiClient';

type AuthUser = {
  userName: string;
  employeeId: string;
  email: string;
  firstName: string;
  lastName: string;
  displayName: string;
  userGroups: string;
  moduleCodes: string;
};

type AuthState = {
  isLoading: boolean;
  isAuthenticated: boolean;
  user: AuthUser | null;
  token: string | null;
  error: string | null;
  refresh: () => Promise<void>;
  hasModule: (...codes: string[]) => boolean;
};

const AuthContext = createContext<AuthState | undefined>(undefined);

const parseCsv = (value: string | null | undefined) =>
  (value ?? '')
    .split(',')
    .map((entry) => entry.trim().toUpperCase())
    .filter(Boolean);

const mapUser = (payload: Record<string, string>): AuthUser => ({
  userName: payload.UserName ?? '',
  employeeId: payload.EmployeeId ?? '',
  email: payload.Email ?? '',
  firstName: payload.FirstName ?? '',
  lastName: payload.LastName ?? '',
  displayName: payload.DisplayName ?? '',
  userGroups: payload.UserGroups ?? '',
  moduleCodes: payload.ModuleCodes ?? ''
});

const fetchJson = async <T,>(url: string) => {
  const response = await fetch(url, {
    credentials: 'include'
  });

  if (response.status === 401) {
    return null;
  }

  if (!response.ok) {
    throw new Error(`Request failed: ${response.status}`);
  }

  return (await response.json()) as T;
};

export function AuthProvider({ children }: PropsWithChildren) {
  const [isLoading, setIsLoading] = useState(true);
  const [user, setUser] = useState<AuthUser | null>(null);
  const [token, setToken] = useState<string | null>(() => window.sessionStorage.getItem(authStorageKey));
  const [error, setError] = useState<string | null>(null);

  const refresh = async () => {
    setIsLoading(true);
    setError(null);

    try {
      const mePayload = await fetchJson<Record<string, string>>(getAuthUrl(appConfig.authMePath));

      if (!mePayload) {
        setUser(null);
        setToken(null);
        window.sessionStorage.removeItem(authStorageKey);
        return;
      }

      setUser(mapUser(mePayload));

      const tokenPayload = await fetchJson<{ access_token: string }>(getAuthUrl(appConfig.authTokenPath));
      const accessToken = tokenPayload?.access_token ?? null;

      setToken(accessToken);

      if (accessToken) {
        window.sessionStorage.setItem(authStorageKey, accessToken);
      } else {
        window.sessionStorage.removeItem(authStorageKey);
      }
    } catch (refreshError) {
      setUser(null);
      setToken(null);
      window.sessionStorage.removeItem(authStorageKey);
      setError(refreshError instanceof Error ? refreshError.message : 'Unable to initialize auth state.');
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    void refresh();
  }, []);

  const value = useMemo<AuthState>(() => {
    const modules = new Set(parseCsv(user?.moduleCodes));

    return {
      isLoading,
      isAuthenticated: Boolean(user),
      user,
      token,
      error,
      refresh,
      hasModule: (...codes: string[]) => {
        if (codes.length === 0) {
          return true;
        }

        return codes.some((code) => modules.has(code.toUpperCase()));
      }
    };
  }, [error, isLoading, token, user]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export const useAuth = () => {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider.');
  }

  return context;
};