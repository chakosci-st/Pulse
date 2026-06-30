# pulse.react

This project is the React migration workspace for Pulse.Web.

## What is included

- Vite + React + TypeScript project scaffold.
- Shared application shell that mirrors the current Pulse.Web navigation model.
- Route catalog covering the current Pulse.Web page surface across root, Admin, Templates, Projects, Sites, and Settings.
- Cookie-aware auth bootstrap against `/Account/Me` and JWT acquisition from `/auth/token`.
- Axios API client preconfigured for the existing Pulse.Api endpoints.
- Migration-oriented placeholder screens for the legacy MVC views so feature work can move route by route.

## Current migration posture

This is a route-complete React foundation, not a finished parity rewrite. The shell, navigation, route inventory, auth integration, and API wiring are in place so the MVC app can be replaced incrementally without guessing the page map.

The highest-risk legacy areas still needing direct feature migration are:

- jQuery/DataTables-heavy list pages.
- Razor partial composition for builders, drawers, and modals.
- Classic ASP.NET SignalR chat flows.
- File upload and download flows tied to existing endpoints.

## Development

1. Copy `.env.example` to `.env` and fill in the existing Pulse.Web and Pulse.Api URLs if you are not using Vite proxying.
2. If you want the React app to proxy the legacy backend during local development, set:
   - `VITE_WEB_PROXY_TARGET` to the Pulse.Web URL.
   - `VITE_API_PROXY_TARGET` to the Pulse.Api URL.
3. Install dependencies with `npm install`.
4. Start the app with `npm run dev`.

## Migration approach

1. Keep legacy routes visible in the React router so the information architecture stays stable.
2. Replace placeholder screens with feature modules one route group at a time.
3. Reuse Pulse.Api wherever possible and reserve new backend work for gaps where Pulse.Web still renders server-only data.
4. Treat chat, upload/download, and builder screens as dedicated migration tracks because they rely on the most MVC-era assumptions.