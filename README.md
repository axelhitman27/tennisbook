# Tennisbook - Tennis Club Management System

A comprehensive web application for tennis clubs to manage players, training sessions, tournaments, subscriptions, and attendance tracking via QR codes.

## Architecture

- **Frontend:** React.js (Vite) — Modern SPA with responsive UI
- **Backend:** C# ASP.NET Core 8 Web API
- **Database:** PostgreSQL with Entity Framework Core

## Features

- **Player Management:** Create, update, and manage player profiles
- **QR Code System:** Each player gets a unique QR code for session check-ins
- **Subscription Tracking:** Plans with weekly/monthly training limits, payment tracking, auto-renewal dates
- **Training Sessions:** Schedule sessions with court, trainer, capacity, and status management
- **Tournament Management:** Create tournaments, register players, track results and placements
- **QR Check-In:** Trainers scan player QR codes to deduct from monthly training allowance
- **Reporting & Analytics:** Per-player reports with training history, tournament results, and charts
- **Dashboard:** Club-wide overview with stats, upcoming events, and recent activity

## Getting Started

### Prerequisites

- .NET 8 SDK
- Node.js 18+
- PostgreSQL 16+

### Database Setup

```bash
# Create the database
psql -U postgres -c "CREATE USER tennisbook WITH PASSWORD 'tennisbook123' SUPERUSER;"
psql -U postgres -c "CREATE DATABASE tennisbook OWNER tennisbook;"
```

### Backend

```bash
cd backend/Tennisbook.API

# Restore and run (migrations apply automatically on startup)
dotnet run
```

The API runs on `http://localhost:5191` with Swagger UI at `/swagger`.

### Frontend

```bash
cd frontend

npm install
npm run dev
```

The UI runs on `http://localhost:5173`.

## API Endpoints

| Resource | Endpoints |
|---|---|
| Players | `GET/POST /api/players`, `GET/PUT/DELETE /api/players/{id}`, `GET /api/players/{id}/qr-image` |
| Subscriptions | `GET/POST /api/subscriptions`, `GET/PUT /api/subscriptions/{id}`, `GET /api/subscriptions/player/{playerId}` |
| Trainings | `GET/POST /api/trainings`, `GET/PUT/DELETE /api/trainings/{id}`, `POST /api/trainings/{id}/check-in?qrCode=...` |
| Tournaments | `GET/POST /api/tournaments`, `GET/PUT/DELETE /api/tournaments/{id}`, `POST /api/tournaments/register` |
| Reports | `GET /api/reports/player/{playerId}`, `GET /api/reports/dashboard` |

## Project Structure

```
tennisbook/
├── backend/
│   └── Tennisbook.API/
│       ├── Controllers/     # API endpoints
│       ├── Data/             # EF Core DbContext
│       ├── DTOs/             # Data transfer objects
│       ├── Migrations/       # Database migrations
│       ├── Models/           # Entity models
│       └── Services/         # Business logic
├── frontend/
│   └── src/
│       ├── api/              # API client (Axios)
│       ├── components/       # Shared UI components
│       └── pages/            # Feature pages
│           ├── Dashboard/
│           ├── Players/
│           ├── Training/
│           ├── Tournaments/
│           ├── Reports/
│           └── QrScanner/
```
