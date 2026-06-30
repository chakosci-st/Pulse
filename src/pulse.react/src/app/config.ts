const trimTrailingSlash = (value: string) => value.replace(/\/+$/, '');

const appRoot = trimTrailingSlash(import.meta.env.VITE_APP_ROOT ?? '');
const authRoot = trimTrailingSlash(import.meta.env.VITE_AUTH_ROOT ?? appRoot);
const apiRoot = trimTrailingSlash(import.meta.env.VITE_API_ROOT ?? '');

export const appConfig = {
  appName: import.meta.env.VITE_APP_NAME || 'PULSE React',
  appRoot,
  authRoot,
  apiRoot,
  authMePath: '/Account/Me',
  authTokenPath: '/auth/token'
};

export const getAbsoluteUrl = (root: string, path: string) => {
  if (!root) {
    return path;
  }

  if (path.startsWith('http://') || path.startsWith('https://')) {
    return path;
  }

  return `${root}${path.startsWith('/') ? path : `/${path}`}`;
};

export const getLegacyUrl = (path: string) => getAbsoluteUrl(appConfig.appRoot, path);
export const getAuthUrl = (path: string) => getAbsoluteUrl(appConfig.authRoot, path);
export const getApiUrl = (path: string) => getAbsoluteUrl(appConfig.apiRoot, path);