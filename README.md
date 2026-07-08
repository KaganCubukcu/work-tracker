# Work Tracker

A personal work-tracking app. Built to keep daily working hours, days worked streak, break schedules and remaining break time, daily logs/notes, and a to-do list all in one place.

## Features

- **Work Time Tracking** — Clock-in/clock-out times and total hours worked
- **Break Tracking** — Defined break slots with a countdown for remaining break time
- **Daily Log** — Notes/events during the day, with search
- **To-Do List** — Daily task tracking
- **User Settings** — Personal settings such as hire date and working hour ranges

## Tech Stack

**Backend**
- .NET 10 Minimal API
- Entity Framework Core
- SQLite

**Frontend**
- Angular 22 (Signals, zoneless)

**Infrastructure**
- Docker / Nginx (configuration in place, not yet actively used)

## Setup & Running

### Requirements
- .NET 10 SDK
- Node.js (LTS) and npm

### Backend

```bash
cd backend/WorkTracker.Api
dotnet run
```

The API runs at `http://localhost:5055` by default. Database migrations are applied automatically on startup, no extra command needed. The SQLite database file (`worktracker.db`) is created automatically on first run.

### Frontend

```bash
cd frontend/work-tracker-web
npm install
npm start
```

The app runs at `http://localhost:4200`. `/api` requests are automatically proxied to the backend (`localhost:5055`) via `proxy.conf.json`.

## Project Structure
work-tracker/
├── backend/
│   └── WorkTracker.Api/     # .NET Minimal API, EF Core, SQLite
└── frontend/
└── work-tracker-web/    # Angular 22 application