# Yoga Studio Leave Request & Attendance Management System

ASP.NET Core MVC application for managing yoga studio staff leave requests, attendance tracking, and employee records.

## Features

- Role-based authentication (Admin, Manager, Staff)
- Employee CRUD with studio role assignment
- Leave request submission, approval, and balance tracking
- Clock in / clock out attendance (via separate Attendance API)
- Dashboard with personal leave balances and upcoming leave

## Tech Stack

- ASP.NET Core 8.0 MVC
- Entity Framework Core
- Oracle Database (Humber server)
- JWT authentication for API communication
- Cookie-based session auth for web

## Prerequisites

- .NET 8.0 SDK
- Oracle database access (Humber VPN required for `calvin.humber.ca`)
- Running instance of the [Attendance API](https://github.com/n01570640/YogaStudioAttendanceAPI) for clock in/out features

## Setup

1. Clone the repository:
```bash
git clone https://github.com/n01570640/YogaStudioLRAManagementSystem.git
cd YogaStudioLRAManagementSystem
```

2. Connect to Humber VPN (required for database access).

3. Create `appsettings.json` in the project root with the following structure:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Jwt": {
    "Key": "YOUR_JWT_SIGNING_KEY"
  },
  "ConnectionStrings": {
    "OracleConnection": "User Id=YOUR_ID;Password=YOUR_PASSWORD;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=calvin.humber.ca)(PORT=1521))(CONNECT_DATA=(SID=grok)))"
  },
  "AllowedHosts": "*"
}
```

> The JWT key must match the one configured in the Attendance API.

4. Restore packages and apply migrations:
```bash
dotnet restore
dotnet ef database update
```

5. Run the application:
```bash
dotnet run
```

The app runs on `https://localhost:7209` and `http://localhost:5130`.

## Running Locally with the API

Both the main app and the Attendance API must be running for clock in / out features to work. Start the API in a separate terminal — see the API repository for setup.

## Default Login

After running migrations, seed data creates an admin account:
- Username: `admin`
- Password: `Admin123`

## Deployment

A deployed version of this project is available, configured on a separate fork: [stephanieBrandon/YogaStudioLRAManagementSystem](https://github.com/stephanieBrandon/YogaStudioLRAManagementSystem). The deployment uses PostgreSQL on Render instead of Humber's Oracle server.

## Team

Built collaboratively as part of Humber College's Computer Programming program (Semester 6, Web Programming course).
