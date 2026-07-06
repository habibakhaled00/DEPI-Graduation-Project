# NeighborHelp - Community Help Platform

Full Stack .NET 8 Capstone Project

## Quick Start
1. Open `NeighborHelp.sln` in Visual Studio 2022
2. Package Manager Console: `Update-Database`
3. Ctrl+F5 to run
4. Default admin: admin@neighborhelp.com / Admin123!

## Tech Stack
- ASP.NET Core 8 MVC + Web API
- Entity Framework Core 8 + SQL Server LocalDB
- ASP.NET Core Identity + JWT
- SignalR (real-time notifications)
- Bootstrap 5.3 + Bootstrap Icons
- Chart.js (admin dashboard)
- xUnit + Moq (testing)

## Features
- User auth (register, login, logout, password reset, roles)
- Help requests (CRUD, categories, search, filter, pagination)
- Volunteer system (apply, accept, reject, complete)
- Messaging (conversations, unread badges)
- Ratings & reviews
- Notifications (real-time via SignalR)
- Admin dashboard (charts, user management, logs)
- RESTful API endpoints with JWT auth
- Location support (ready for Google Maps)
