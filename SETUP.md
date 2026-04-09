# Tennisbook - Local Development Setup Guide

Follow these steps **in order** to run Tennisbook locally on your machine.

---

## Prerequisites

Make sure you have these installed before starting:

| Tool | Version | Check command | Install link |
|------|---------|---------------|--------------|
| .NET SDK | 8.0+ | `dotnet --version` | https://dotnet.microsoft.com/download/dotnet/8.0 |
| Node.js | 18+ | `node --version` | https://nodejs.org |
| npm | 9+ | `npm --version` | Comes with Node.js |
| PostgreSQL | 14+ | `psql --version` | https://www.postgresql.org/download |

---

## Step 1 — Clone the Repository

```bash
git clone https://github.com/axelhitman27/tennisbook.git
cd tennisbook
```

---

## Step 2 — Set Up PostgreSQL

Start the PostgreSQL service if it's not already running:

```bash
# macOS (Homebrew)
brew services start postgresql@16

# Linux (systemd)
sudo systemctl start postgresql

# Windows — use pgAdmin or Services panel
```

Then create the database and user. Open a terminal and run:

```bash
psql -U postgres
```

Inside the `psql` shell, run these SQL commands:

```sql
CREATE USER tennisbook WITH PASSWORD 'tennisbook123' SUPERUSER;
CREATE DATABASE tennisbook OWNER tennisbook;
\q
```

> **Verify it works:** Run `psql -U tennisbook -d tennisbook -c "SELECT 1;"` — you should see a result of `1`.

If your PostgreSQL requires a different host, port, or credentials, edit the connection string in:
`backend/Tennisbook.API/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=tennisbook;Username=tennisbook;Password=tennisbook123"
  }
}
```

---

## Step 3 — Install EF Core CLI Tools (one-time)

This is needed to manage database migrations from the command line:

```bash
dotnet tool install --global dotnet-ef
```

If already installed, you can update it:

```bash
dotnet tool update --global dotnet-ef
```

> Make sure `~/.dotnet/tools` is in your PATH. The installer will tell you if it isn't.

---

## Step 4 — Run the Backend

```bash
cd backend/Tennisbook.API

# Restore NuGet packages
dotnet restore

# Build to check for errors
dotnet build

# Run the API (migrations apply automatically on startup)
dotnet run
```

You should see output like:

```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5191
```

**Verify the backend is running:**

- Open **http://localhost:5191/swagger** in your browser — you'll see the Swagger UI with all API endpoints
- Or run: `curl http://localhost:5191/api/reports/dashboard`

> **Leave this terminal running.** The backend must stay active while you use the app.

---

## Step 5 — Run the Frontend

Open a **new terminal window** (keep the backend running in the first one):

```bash
cd frontend

# Install npm dependencies
npm install

# Start the development server
npm run dev
```

You should see:

```
  VITE v8.x.x  ready in XXX ms

  ➜  Local:   http://localhost:5173/
```

**Open http://localhost:5173 in your browser** — you'll see the Tennisbook dashboard.

---

## Step 6 — Quick Smoke Test

With both servers running, try this flow to verify everything works end-to-end:

### 1. Create a Player
- Go to **Players** in the sidebar
- Click **Add Player**
- Fill in: First Name, Last Name, Email, Date of Birth, Level
- Click **Create Player**

### 2. Add a Subscription
- Click on the player you just created
- Click **Add Subscription**
- Choose a plan (e.g., Standard: 3x/week, 12/month, €60)
- Click **Create Subscription**
- You should see the subscription card with remaining trainings

### 3. Create a Training Session
- Go to **Training** in the sidebar
- Click **New Session**
- Fill in title, date/time (use a future date), court, trainer
- Click **Create Session**

### 4. Check In via QR Scanner
- Go to **QR Scanner** in the sidebar
- Select the training session you created
- Go back to the player's detail page and copy their QR code value
- Paste it into the QR code input and click **Check In Player**
- You should see a green success message with remaining trainings count

### 5. View Reports
- Go to **Reports** in the sidebar
- Select the player from the dropdown
- You'll see their training stats, charts, and history

---

## Quick Reference — Running Both Servers

Once setup is complete, you only need two commands in two terminals:

**Terminal 1 — Backend:**
```bash
cd backend/Tennisbook.API && dotnet run
```

**Terminal 2 — Frontend:**
```bash
cd frontend && npm run dev
```

| Service | URL |
|---------|-----|
| Frontend (React) | http://localhost:5173 |
| Backend API | http://localhost:5191 |
| Swagger API Docs | http://localhost:5191/swagger |

---

## Troubleshooting

### "Connection refused" on port 5432
PostgreSQL isn't running. Start it with `brew services start postgresql@16` (macOS) or `sudo systemctl start postgresql` (Linux).

### "role tennisbook does not exist"
You need to run Step 2 to create the database user. Connect as `psql -U postgres` first.

### "CORS error" in browser console
Make sure the backend is running on port 5191. The CORS policy is configured to allow `http://localhost:5173` and `http://localhost:3000`.

### Frontend shows "Failed to load dashboard"
The backend API isn't reachable. Check that `dotnet run` is active in the other terminal and listening on port 5191.

### Port 5191 is already in use
Kill the existing process: `lsof -ti:5191 | xargs kill -9` (macOS/Linux), then re-run `dotnet run`.

### Database migration errors
If you need to reset the database:
```bash
psql -U postgres -c "DROP DATABASE tennisbook;"
psql -U postgres -c "CREATE DATABASE tennisbook OWNER tennisbook;"
cd backend/Tennisbook.API && dotnet run
```
Migrations re-apply automatically on startup.
