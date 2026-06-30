var __assign = (this && this.__assign) || function () {
    __assign = Object.assign || function(t) {
        for (var s, i = 1, n = arguments.length; i < n; i++) {
            s = arguments[i];
            for (var p in s) if (Object.prototype.hasOwnProperty.call(s, p))
                t[p] = s[p];
        }
        return t;
    };
    return __assign.apply(this, arguments);
};
import { defineConfig, loadEnv } from 'vite';
import react from '@vitejs/plugin-react';
export default defineConfig(function (_a) {
    var mode = _a.mode;
    var env = loadEnv(mode, process.cwd(), '');
    return {
        plugins: [react()],
        server: {
            port: 5173,
            proxy: {
                '/api': __assign({ target: env.VITE_API_PROXY_TARGET, changeOrigin: true, secure: false }, (env.VITE_API_PROXY_TARGET ? {} : { bypass: function () { return '/api'; } })),
                '/auth': __assign({ target: env.VITE_WEB_PROXY_TARGET, changeOrigin: true, secure: false }, (env.VITE_WEB_PROXY_TARGET ? {} : { bypass: function () { return '/auth'; } })),
                '/Account': __assign({ target: env.VITE_WEB_PROXY_TARGET, changeOrigin: true, secure: false }, (env.VITE_WEB_PROXY_TARGET ? {} : { bypass: function () { return '/Account'; } })),
                '/signalr': __assign({ target: env.VITE_WEB_PROXY_TARGET, changeOrigin: true, ws: true, secure: false }, (env.VITE_WEB_PROXY_TARGET ? {} : { bypass: function () { return '/signalr'; } })),
                '/files': __assign({ target: env.VITE_WEB_PROXY_TARGET, changeOrigin: true, secure: false }, (env.VITE_WEB_PROXY_TARGET ? {} : { bypass: function () { return '/files'; } }))
            }
        }
    };
});
