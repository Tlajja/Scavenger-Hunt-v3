# Scavenger-Hunt
PS inzinerija 1

## About
Photo scavenger hunt. I.e. a specific task for what kind of photo to capture is provided and people have a limited amount of time to upload such photo. Photo with the most votes wins.

Tech stack: C#, .NET, React, SignalR, Cloudinary, SQLite.

## Goals
10 main features (funkcionalumai):
1. Photo function - store & upload, delete (using Cloudinary)
2. Generating task(s)
3. User writing and submitting their own tasks
4. Authentication - Create/delete user profile, view user's challenges
5. Voting on photos
6. Comments (on submissions in voting stage)
7. Leaderboard - there's a leaderboard after a challenge and a hall of fame (win count)
8. Timers - game and voting times, tasks also have timers.
9. User activity - how many people are using the website at the moment (still buggy)
10. Create/join challenge - private or public

## Getting Started

### Prerequisites
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js](https://nodejs.org/) (v18 or higher)
- Git

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd Scavenger-Hunt
   ```

2. **Setup Backend**
   ```bash
   # Restore .NET packages
   dotnet restore

   # Apply database migrations (creates PhotoScavengerHunt.db)
   dotnet ef database update

   # Build the project
   dotnet build
   ```

3. **Setup Frontend**
   ```bash
   # Navigate to Client directory
   cd Client

   # Install npm packages
   npm install

   # Return to root directory
   cd ..
   ```

### Running the Application

You need to run both the backend and frontend simultaneously:

**Terminal 1 - Backend API:**
```bash
dotnet run
```
The backend will start on `http://localhost:5248`

**Terminal 2 - Frontend:**
```bash
cd Client
npm run dev
```
The frontend will start on `http://localhost:5173`

**Access the application:** Open your browser and navigate to `http://localhost:5173`

### Running Tests

```bash
dotnet test
```

## Database

This project uses SQLite for local development. The database file (`PhotoScavengerHunt.db`) is created automatically when you run migrations.

### Reset Database
```bash
# Delete the database file
rm PhotoScavengerHunt.db

# Recreate it with migrations
dotnet ef database update
```

### View Database
You can use any SQLite browser tool like:
- [DB Browser for SQLite](https://sqlitebrowser.org/)
- [SQLite Viewer (VS Code Extension)](https://marketplace.visualstudio.com/items?itemName=alexcvzz.vscode-sqlite)

## Troubleshooting

### CORS Errors
- Ensure both backend (`http://localhost:5248`) and frontend (`http://localhost:5173`) are running
- Check that no other applications are using these ports

### Database Locked Error
- Stop all running instances of the application
- Close any database browser tools
- Restart the application

### Migration Errors
```bash
# Drop and recreate the database
dotnet ef database drop
dotnet ef database update
```

## Development Workflow

1. Pull latest changes: `git pull`
2. Update dependencies: `dotnet restore` and `cd Client && npm install`
3. Apply new migrations: `dotnet ef database update`
4. Start backend: `dotnet run`
5. Start frontend: `cd Client && npm run dev`
