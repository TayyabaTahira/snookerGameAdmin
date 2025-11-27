# 🎱 Snooker Game Management System

A modern WPF Desktop Application for managing snooker clubs built with **.NET 10** and **MySQL**.

## ✨ Features

- 🔐 Secure Authentication
- 📊 Dynamic Dashboard - Manage multiple tables
- ⚡ Session Management - Track games
- 👥 Customer Management - Track players & balances
- 💰 Billing System - Flexible payment modes
- 💳 Payment Processing - FIFO allocation
- 🕐 Real-Time Timer

## 🛠 Tech Stack

- **Framework**: .NET 10
- **UI**: WPF (MVVM Architecture)
- **ORM**: Entity Framework Core 8.0
- **Database**: MySQL 8.0+
- **MySQL Provider**: Pomelo.EntityFrameworkCore.MySql 8.0.2
- **Authentication**: BCrypt.Net-Next 4.0.3

## 📦 Prerequisites

1. **Visual Studio 2022** or later with .NET 10 SDK
2. **MySQL Server 8.0+**

## 🚀 Quick Start

### 1. Database Setup

```bash
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

## 🎯 Quick Usage Guide

### Creating a Session
1. Click "+ Add Table"
2. **Select Game Type** (Single/Century/Doubles)
3. Enter table name
4. Select/Create 2+ players from dropdown
5. Click "Create Table"

### Playing
1. Click on session tile
2. Click "Next Frame" → Select winner
3. Repeat for multiple frames
4. Click "End Game" → Enter billing details

### Processing Payments
1. Go to Customers
2. Click "💰 Pay" next to customer
3. Enter amount
4. Click "Process Payment"

## 🐛 Troubleshooting

### Connection Error
1. Check MySQL is running: `sc query MySQL80`
2. Verify connection string in `appsettings.json`
3. Test: `mysql -u root -p`

### Database Not Found
```sql
CREATE DATABASE snooker_club_db;
SOURCE Database/SnookerDB_Schema.sql;
```

## 📄 License

This project is for educational and commercial use.

---

**Built with ❤️ using .NET 10 and WPF**
