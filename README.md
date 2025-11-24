# 🎱 Snooker Game Management System

A modern WPF Desktop Application for managing snooker clubs built with **.NET 10** and **MySQL**.

## ✨ Features

- 🔐 **Secure Authentication** - BCrypt password hashing
- 📊 **Dynamic Dashboard** - Manage multiple tables simultaneously
- ⚡ **Session Management** - Track game sessions
- 👥 **Customer Management** - Track players
- 💰 **Billing System** - Flexible payment modes
- 🕐 **Timer System** - Track game time with overtime charges

## 🛠 Tech Stack

- **Framework**: .NET 10
- **UI**: WPF (Windows Presentation Foundation)
- **Architecture**: MVVM (Model-View-ViewModel)
- **ORM**: Entity Framework Core 8.0
- **Database**: MySQL 8.0+
- **MySQL Provider**: Pomelo.EntityFrameworkCore.MySql
- **Authentication**: BCrypt.Net

## 📦 Prerequisites

1. **Visual Studio 2022** or later with .NET 10 SDK
2. **MySQL Server 8.0+** ([Download](https://dev.mysql.com/downloads/mysql/))

## 🚀 Quick Start

### 1. Database Setup

```bash
# Create database
mysql -u root -p
CREATE DATABASE snooker_club_db;
USE snooker_club_db;
SOURCE Database/SnookerDB_Schema.sql;
```

### 2. Configure Connection

Edit `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "SnookerDb": "Server=localhost;Database=snooker_club_db;User=root;Password=YOUR_PASSWORD;AllowUserVariables=true;UseAffectedRows=false"
  }
}
```

### 3. Build and Run

```bash
dotnet restore
dotnet build
dotnet run
```

## 🔑 Default Login

```
Username: admin
Password: admin123
```

⚠️ **Change this password in production!**

## 📁 Project Structure

```
SnookerGameManagementSystem/
├── Models/              # Entity classes
├── Data/                # Database context
├── Services/            # Business logic
├── ViewModels/          # MVVM ViewModels
├── Views/               # WPF Windows
├── Converters/          # Value converters
├── Database/            # Database scripts
└── appsettings.json    # Configuration
```

## 🐛 Troubleshooting

### Connection Error

1. Verify MySQL is running: `sc query MySQL80` (Windows)
2. Check connection string in `appsettings.json`
3. Test connection: `mysql -u root -p`

### Database Not Found

```sql
CREATE DATABASE snooker_club_db;
SOURCE Database/SnookerDB_Schema.sql;
```

## 📄 License

This project is for educational and commercial use.

---

**Built with ❤️ using .NET 10 and WPF**
