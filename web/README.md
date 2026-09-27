# MediLedger web (Phase 3)

Angular 19 counter UI for the Pharmacy Management System.

**UI/UX:** follow repo store doc `phase-3-ui-design-system.md` (teal primary `#0F766E`, Inter, shared components — no per-page invented styles).

## Run

API must be up first (default `http://127.0.0.1:5329`).

```bash
cd web
npm install
npm start
```

App: http://127.0.0.1:43123  
Login: `admin` / `Admin@12345` (Development seed)

`environment.apiBaseUrl` defaults to `http://127.0.0.1:5329`. CORS allows that Angular origin. Optional `proxy.conf.json` is wired for `/api` if you prefer same-origin proxying.

## Build

```bash
npm run build
```
