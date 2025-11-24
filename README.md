# 🎱 Snooker Game Management System

A modern, feature-rich **WPF Desktop Application** for managing snooker clubs built with **.NET 10** and **MySQL**.

## 📋 Table of Contents
- [Features](#-features)
- [Tech Stack](#-tech-stack)
- [Prerequisites](#-prerequisites)
- [Database Setup](#-database-setup)
- [Application Setup](#-application-setup)
- [Project Structure](#-project-structure)
- [Usage Guide](#-usage-guide)
- [Architecture](#-architecture)

---

## ✨ Features

### ✅ Implemented
- 🔐 **Secure Authentication** - BCrypt password hashing
- 📊 **Dynamic Dashboard** - Manage multiple tables simultaneously
- ⚡ **Session Management** - Create, track, and end game sessions
- 👥 **Customer Management** - Track players and their history
- 💰 **Billing System** - Flexible payment modes (Loser pays, Split, Custom)
- 🕐 **Timer System** - Track game time with overtime charges
- 📈 **FIFO Payment Allocation** - Fair payment distribution
- 🎨 **Modern Responsive UI** - Clean, intuitive interface

### 🚧 Coming Soon
- 📝 Billing popup with overtime calculation
- 🏆 Win/Loss streak tracking
- 📊 Comprehensive reporting
- 💳 Credit management
- 🖨️ Invoice generation
- ☁️ Cloud sync (optional)

---

## 🛠 Tech Stack

| Layer | Technology |
|-------|-----------|
| **Framework** | .NET 10 |
| **UI** | WPF (Windows Presentation Foundation) |
| **Architecture** | MVVM (Model-View-ViewModel) |
| **ORM** | Entity Framework Core 8.0 |
| **Database** | MySQL 8.0+ |
| **MySQL Provider** | Pomelo.EntityFrameworkCore.MySql |
| **Authentication** | BCrypt.Net |
| **DI Container** | Microsoft.Extensions.DependencyInjection |

---

## 📦 Prerequisites

### Required Software
1. **Visual Studio 2022** or later (with .NET 10 SDK)
2. **MySQL Server 8.0+** ([Download](https://dev.mysql.com/downloads/mysql/))
3. **MySQL Workbench** (optional, for GUI management)

### Optional Tools
- **HeidiSQL** or **phpMyAdmin** (alternative MySQL clients)

---

## 🗄 Database Setup

### Step 1: Install MySQL
1. Download MySQL Server from [official website](https://dev.mysql.com/downloads/mysql/)
2. Install with default settings
3. Remember your **root password**

### Step 2: Create Database
Open MySQL Workbench or command line:

```bash
mysql -u root -p
```

Then run the schema script:

```bash
source Database/SnookerDB_Schema.sql
```

Or manually:
```sql
-- Copy contents from Database/SnookerDB_Schema.sql and execute
```

or via command line:
```bash
cd C:\Users\tayya\source\repos\SnookerGameManagementSystem
"C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe" -u root -p snooker_club_db < Database\SnookerDB_Schema.sql
```

### Step 3: Verify Database
```sql
USE snooker_club_db;
SHOW TABLES;
SELECT * FROM app_user;
```

You should see 10 tables and 1 admin user.

---

## 🚀 Application Setup

### Step 1: Clone/Open Project
Open `SnookerGameManagementSystem.sln` in Visual Studio.

### Step 2: Configure Connection String
Edit `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "SnookerDb": "Server=localhost;Database=snooker_club_db;User=root;Password=YOUR_MYSQL_PASSWORD;AllowUserVariables=true;UseAffectedRows=false"
  }
}
```

**Replace `YOUR_MYSQL_PASSWORD` with your actual MySQL root password.**

### Step 3: Restore NuGet Packages
```bash
dotnet restore
```

### Step 4: Build Solution
```bash
dotnet build
```

### Step 5: Run Application
```bash
dotnet run
```

Or press **F5** in Visual Studio.

---

## 🔑 Default Login Credentials

```
Username: admin
Password: admin123
```

⚠️ **IMPORTANT**: Change this password in production!

To change password, update the database:
```sql
-- Generate new hash using BCrypt (work factor 12)
UPDATE app_user 
SET password_hash = '$2a$12$NEW_HASH_HERE' 
WHERE username = 'admin';
```

---

## 📁 Project Structure

```
SnookerGameManagementSystem/
│
├── Models/                      # Entity classes
│   ├── AppUser.cs
│   ├── GameType.cs
│   ├── GameRule.cs
│   ├── Customer.cs
│   ├── Session.cs
│   ├── Frame.cs
│   ├── FrameParticipant.cs
│   ├── LedgerCharge.cs
│   ├── LedgerPayment.cs
│   └── PaymentAllocation.cs
│
├── Data/                        # Database context
│   └── SnookerDbContext.cs
│
├── Services/                    # Business logic
│   ├── AuthService.cs
│   ├── SessionService.cs
│   ├── CustomerService.cs
│   └── GameRuleService.cs
│
├── ViewModels/                  # MVVM ViewModels
│   ├── ViewModelBase.cs
│   ├── LoginViewModel.cs
│   └── DashboardViewModel.cs
│
├── Views/                       # WPF Windows
│   ├── LoginWindow.xaml
│   ├── LoginWindow.xaml.cs
│   ├── DashboardWindow.xaml
│   └── DashboardWindow.xaml.cs
│
├── Converters/                  # Value converters
│   └── ValueConverters.cs
│
├── Database/                    # Database scripts
│   ├── SnookerDB_Schema.sql
│   └── README.md
│
├── App.xaml                     # Application resources
├── App.xaml.cs                  # App startup & DI setup
└── appsettings.json            # Configuration
```

---

## 📖 Usage Guide

### 1️⃣ Login
- Launch application
- Enter credentials (admin/admin123)
- Click **LOGIN**

### 2️⃣ Dashboard
- View all active game sessions (tables)
- Each tile shows:
  - Table name
  - Game type
  - Players
  - Elapsed time
  - Frame count

### 3️⃣ Add New Table
- Click **+ Add Table** button
- New session created automatically
- Table number assigned dynamically

### 4️⃣ Manage Session (Coming Soon)
- Click on any table tile
- Start frame with players
- Track timer
- End game and process billing

### 5️⃣ Billing (Coming Soon)
- Select winner
- Calculate overtime charges
- Choose payer mode
- Pay now or credit

---

## 🏗 Architecture

### MVVM Pattern
```
View (XAML) ←→ ViewModel ←→ Service ←→ DbContext ←→ Database
```

### Data Flow

#### Login Flow
```
LoginWindow → LoginViewModel → AuthService → SnookerDbContext → MySQL
```

#### Session Management Flow
```
DashboardWindow → DashboardViewModel → SessionService → SnookerDbContext → MySQL
```

### Dependency Injection
All services and ViewModels are registered in `App.xaml.cs`:

```csharp
services.AddDbContext<SnookerDbContext>();
services.AddScoped<AuthService>();
services.AddScoped<SessionService>();
services.AddTransient<LoginViewModel>();
services.AddTransient<DashboardViewModel>();
```

---

## 🗃 Database Schema

### Core Tables

| Table | Purpose |
|-------|---------|
| `app_user` | Admin authentication |
| `game_type` | Game types (Single, Century, Doubles) |
| `game_rule` | Pricing rules per game type |
| `customer` | Players/Customers |
| `session` | Virtual tables (game sessions) |
| `frame` | Individual game rounds |
| `frame_participant` | Players in each frame |
| `ledger_charge` | Customer charges/debts |
| `ledger_payment` | Customer payments |
| `payment_allocation` | FIFO payment allocation |

### Key Relationships
```
session (1) ─── (M) frame ─── (M) frame_participant
                           └── (M) ledger_charge ─── (M) payment_allocation ─── (1) ledger_payment
```

---

## 🧮 Business Logic

### Billing Calculation
```
base_rate = game_rule.base_rate
overtime_minutes = max(0, ceil((elapsed_time - time_limit) / 1min))
overtime_amount = per_min_rate × overtime_minutes OR lump_sum_fine
total_amount = base_rate + overtime_amount - discounts
```

### Payment Modes
- **LOSER** - Loser pays full amount
- **SPLIT** - Split equally among all players
- **CUSTOM** - Manual allocation

### FIFO Payment Allocation
When customer makes payment:
1. Sort unpaid charges oldest → newest
2. Apply payment sequentially
3. Allow partial allocation
4. Track in `payment_allocation` table

---

## 🔧 Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
    "SnookerDb": "Server=localhost;Database=snooker_club_db;User=root;Password=YOUR_PASSWORD;AllowUserVariables=true;UseAffectedRows=false"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  }
}
```

---

## 🐛 Troubleshooting

### Connection Error
**Error**: `Unable to connect to MySQL server`

**Solution**:
1. Verify MySQL is running: `services.msc` → MySQL80
2. Check connection string in `appsettings.json`
3. Test connection: `mysql -u root -p`

### Build Errors
**Error**: Package restore failed

**Solution**:
```bash
dotnet clean
dotnet restore
dotnet build
```

### Database Not Found
**Error**: `Unknown database 'snooker_club_db'`

**Solution**:
```sql
CREATE DATABASE snooker_club_db;
USE snooker_club_db;
SOURCE Database/SnookerDB_Schema.sql;
```

---

## 📝 Sample Data

The schema includes sample data:
- 1 admin user
- 3 game types (Single, Century, Doubles)
- 3 game rules
- 4 sample customers

---

## 🔮 Future Enhancements

- [ ] Frame management popup
- [ ] Billing dialog with calculations
- [ ] Customer management window
- [ ] Reports and analytics
- [ ] Win/loss streak tracking
- [ ] Invoice printing
- [ ] WhatsApp notifications
- [ ] Cloud sync
- [ ] Leaderboards
- [ ] Prepaid packages

---

## 📄 License

This project is for educational and commercial use.

---

## 👨‍💻 Development

Built with ❤️ using:
- .NET 10
- WPF
- Entity Framework Core
- MySQL
- MVVM Pattern

---

## 📞 Support

For issues or questions:
1. Check the database connection
2. Verify MySQL is running
3. Check `appsettings.json` configuration
4. Review error logs

---

## 🎯 Next Steps

1. ✅ Install MySQL and create database
2. ✅ Configure connection string
3. ✅ Run application
4. ✅ Login with default credentials
5. 🔄 Start building remaining features!

---

**Happy Coding! 🎱**
