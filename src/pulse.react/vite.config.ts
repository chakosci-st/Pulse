import { defineConfig, loadEnv } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '');

  return {
    plugins: [react()],
    server: {
      port: 5173,
      proxy: {
        '/api': {
          target: env.VITE_API_PROXY_TARGET,
          changeOrigin: true,
          secure: false,
          ...(env.VITE_API_PROXY_TARGET ? {} : { bypass: () => '/api' })
        },
        '/auth': {
          target: env.VITE_WEB_PROXY_TARGET,
          changeOrigin: true,
          secure: false,
          ...(env.VITE_WEB_PROXY_TARGET ? {} : { bypass: () => '/auth' })
        },
        '/Account': {
          target: env.VITE_WEB_PROXY_TARGET,
          changeOrigin: true,
          secure: false,
          ...(env.VITE_WEB_PROXY_TARGET ? {} : { bypass: () => '/Account' })
        },
        '/signalr': {
          target: env.VITE_WEB_PROXY_TARGET,
          changeOrigin: true,
          ws: true,
          secure: false,
          ...(env.VITE_WEB_PROXY_TARGET ? {} : { bypass: () => '/signalr' })
        },
        '/files': {
          target: env.VITE_WEB_PROXY_TARGET,
          changeOrigin: true,
          secure: false,
          ...(env.VITE_WEB_PROXY_TARGET ? {} : { bypass: () => '/files' })
        }
      }
    }
  };
});