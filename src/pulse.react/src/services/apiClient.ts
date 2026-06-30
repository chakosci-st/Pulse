import axios from 'axios';

import { appConfig } from '../app/config';

export const authStorageKey = 'pulse.react.access-token';

export const apiClient = axios.create({
  baseURL: appConfig.apiRoot || undefined,
  withCredentials: true
});

apiClient.interceptors.request.use((request) => {
  const token = window.sessionStorage.getItem(authStorageKey);

  if (token) {
    request.headers.Authorization = `Bearer ${token}`;
  }

  return request;
});