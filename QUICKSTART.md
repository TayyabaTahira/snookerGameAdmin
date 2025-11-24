# ?? Quick Start Guide - Snooker Game Management System

## ? 5-Minute Setup

### Step 1: Install MySQL (2 minutes)
```bash
# Download MySQL 8.0+ from:
https://dev.mysql.com/downloads/mysql/

# Install with default settings
# Set root password: YOUR_PASSWORD
```

### Step 2: Create Database (1 minute)
```bash
# Open command prompt/terminal
mysql -u root -p

# Enter your password, then run:
CREATE DATABASE snooker_club_db CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
exit;

# Run the schema script
mysql -u root -p snooker_club_db < Database/SnookerDB_Schema.sql
```

### Step 3: Configure App (30 seconds)
Edit `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "SnookerDb": "Server=localhost;Database=snooker_club_db;User=root;Password=YOUR_PASSWORD;AllowUserVariables=true"
  }
}
```

### Step 4: Run Application (30 seconds)
```bash
# In Visual Studio: Press F5
# Or via command line:
dotnet run
```

### Step 5: Login (30 seconds)
```
Username: admin
Password: admin123
```

## ? You're Done!

---

## ?? What You Can Do Now

1. **Add Tables** - Click the + button to create virtual tables
2. **View Active Sessions** - See all running games
3. **Track Time** - Each session shows elapsed time
4. **Manage Customers** - View player information

---

## ?? Database Verification

```sql
-- Check if database exists
SHOW DATABASES LIKE 'snooker_club_db';

-- Verify tables
USE snooker_club_db;
SHOW TABLES;

-- Check admin user
SELECT username FROM app_user;

-- View game types
SELECT * FROM game_type;

-- View sample customers
SELECT full_name FROM customer;
```

Expected Output:
- ? 10 tables
- ? 1 admin user
- ? 3 game types (Single, Century, Doubles)
- ? 4 sample customers

---

## ?? Common Issues & Fixes

### Issue 1: Can't Connect to MySQL
```bash
# Check if MySQL is running
# Windows: services.msc ? MySQL80
# Linux/Mac: systemctl status mysql

# Test connection
mysql -u root -p
```

### Issue 2: Database Not Found
```sql
CREATE DATABASE IF NOT EXISTS snooker_club_db;
```

### Issue 3: Build Errors
```bash
dotnet clean
dotnet restore
dotnet build
```

### Issue 4: Password Authentication Failed
- Verify password in `appsettings.json`
- Reset MySQL password if needed
- Check user has permissions

---

## ?? UI Overview

### Login Screen
- Modern dark theme
- Username/Password fields
- Error messages
- Loading indicator

### Dashboard
- Header with app name
- Active session tiles showing:
  - Table name
  - Game type
  - Players
  - Timer
  - Frame count
- **+ Add Table** button
- Refresh button
- Reports button (coming soon)
- Customers button (coming soon)

---

## ?? Sample Workflow

### Starting a New Game

1. **Launch App** ? Login with admin/admin123
2. **Click "+ Add Table"** ? Creates "Table #1"
3. **(Coming Soon)** Click table tile ? Add players
4. **(Coming Soon)** Select game type ? Start timer
5. **(Coming Soon)** End game ? Process billing

---

## ?? Security Notes

### Change Default Password
```sql
-- Generate BCrypt hash for new password
-- Use online BCrypt generator with work factor 12
UPDATE app_user 
SET password_hash = '$2a$12$YOUR_NEW_HASH' 
WHERE username = 'admin';
```

### Database Security
```sql
-- Create dedicated app user (recommended)
CREATE USER 'snooker_app'@'localhost' IDENTIFIED BY 'STRONG_PASSWORD';
GRANT ALL PRIVILEGES ON snooker_club_db.* TO 'snooker_app'@'localhost';
FLUSH PRIVILEGES;
```

Then update connection string:
```json
"SnookerDb": "Server=localhost;Database=snooker_club_db;User=snooker_app;Password=STRONG_PASSWORD;..."
```

---

## ?? Package Versions

```xml
<PackageReference Include="Pomelo.EntityFrameworkCore.MySql" Version="8.0.2" />
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="8.0.0" />
```

---

## ?? Testing the Setup

### 1. Test Database Connection
```csharp
// In Package Manager Console:
dotnet ef dbcontext info
```

### 2. Test Authentication
- Login with `admin`/`admin123`
- Should redirect to dashboard

### 3. Test Session Creation
- Click "+ Add Table"
- Should create "Table #1"
- Refresh should show the table

### 4. Test Data Persistence
- Close app
- Reopen and login
- Tables should still be there (if not ended)

---

## ?? Learn More

- **Full Documentation**: See `README.md`
- **Database Schema**: See `Database/SnookerDB_Schema.sql`
- **Database Guide**: See `Database/README.md`

---

## ?? Need Help?

### Check These First:
1. ? MySQL is running
2. ? Database `snooker_club_db` exists
3. ? Connection string is correct
4. ? Password matches MySQL root password
5. ? All NuGet packages restored

### Still Having Issues?
1. Check Visual Studio Output window
2. Review error messages carefully
3. Verify MySQL logs
4. Test database connection manually

---

## ?? Success Checklist

- [ ] MySQL installed and running
- [ ] Database created from schema
- [ ] Connection string configured
- [ ] Application builds successfully
- [ ] Can login with admin credentials
- [ ] Dashboard loads
- [ ] Can add new tables
- [ ] Tables show elapsed time

---

**You're ready to start developing! ??**
