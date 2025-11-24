# ?? MySQL Connection Troubleshooting Guide

## ?? Quick Diagnostics

### Test 1: Check MySQL Service Status
**Windows:**
```bash
# Open Services (services.msc)
# Look for MySQL80 service
# Status should be "Running"

# Or via command line:
sc query MySQL80
```

**Linux/Mac:**
```bash
sudo systemctl status mysql
# or
sudo service mysql status
```

**Expected Output:**
```
SERVICE_NAME: MySQL80
STATE: 4 RUNNING
```

---

### Test 2: Test MySQL Connection
```bash
mysql -u root -p
```

**If successful:**
```
Welcome to the MySQL monitor...
mysql>
```

**If failed:**
```
ERROR 2003 (HY000): Can't connect to MySQL server on 'localhost' (10061)
```

---

### Test 3: Verify Database Exists
```sql
SHOW DATABASES LIKE 'snooker_club_db';
```

**Expected Output:**
```
+-----------------------------+
| Database                    |
+-----------------------------+
| snooker_club_db            |
+-----------------------------+
```

---

## ? Common Errors & Solutions

### Error 1: Service Not Running
```
ERROR 2003 (HY000): Can't connect to MySQL server
```

**Solution:**
```bash
# Windows
net start MySQL80

# Linux/Mac
sudo systemctl start mysql
sudo service mysql start
```

---

### Error 2: Access Denied
```
ERROR 1045 (28000): Access denied for user 'root'@'localhost'
```

**Solutions:**

**A. Reset Root Password:**
```bash
# Stop MySQL service
net stop MySQL80

# Start in safe mode (Windows)
mysqld --skip-grant-tables

# In new terminal:
mysql -u root

# Reset password
FLUSH PRIVILEGES;
ALTER USER 'root'@'localhost' IDENTIFIED BY 'new_password';
```

**B. Verify Username:**
```sql
SELECT User, Host FROM mysql.user;
```

---

### Error 3: Database Not Found
```
ERROR 1049 (42000): Unknown database 'snooker_club_db'
```

**Solution:**
```bash
# Run schema script
mysql -u root -p < Database/SnookerDB_Schema.sql

# Or manually:
mysql -u root -p
CREATE DATABASE snooker_club_db;
USE snooker_club_db;
SOURCE Database/SnookerDB_Schema.sql;
```

---

### Error 4: Wrong Connection String
```
Unable to connect to any of the specified MySQL hosts
```

**Check appsettings.json:**
```json
{
  "ConnectionStrings": {
    "SnookerDb": "Server=localhost;Database=snooker_club_db;User=root;Password=YOUR_PASSWORD;AllowUserVariables=true;UseAffectedRows=false"
  }
}
```

**Common mistakes:**
- ? `Server=127.0.0.1` instead of `localhost`
- ? Wrong password
- ? Wrong database name (case-sensitive on Linux)
- ? Missing port (default: 3306)

**Try with port explicitly:**
```json
"SnookerDb": "Server=localhost;Port=3306;Database=snooker_club_db;User=root;Password=YOUR_PASSWORD;AllowUserVariables=true"
```

---

### Error 5: Firewall Blocking
```
ERROR 2003: Can't connect to MySQL server (10061)
```

**Solution (Windows):**
1. Open Windows Firewall
2. Add inbound rule for port 3306
3. Allow MySQL.exe through firewall

**Solution (Linux):**
```bash
sudo ufw allow 3306/tcp
sudo firewall-cmd --add-port=3306/tcp --permanent
```

---

### Error 6: SSL/TLS Issues
```
SSL connection error
```

**Solution - Disable SSL (for local development):**
```json
"SnookerDb": "Server=localhost;Database=snooker_club_db;User=root;Password=YOUR_PASSWORD;SslMode=None"
```

---

### Error 7: Character Set Issues
```
Incorrect string value
```

**Solution:**
```sql
ALTER DATABASE snooker_club_db 
CHARACTER SET utf8mb4 
COLLATE utf8mb4_unicode_ci;
```

---

## ?? Detailed Diagnostics

### Check MySQL Configuration
```bash
mysql -u root -p -e "SHOW VARIABLES LIKE '%version%';"
mysql -u root -p -e "SHOW VARIABLES LIKE '%port%';"
mysql -u root -p -e "SHOW VARIABLES LIKE '%socket%';"
```

### Check Connection Limits
```sql
SHOW VARIABLES LIKE 'max_connections';
SHOW PROCESSLIST;
```

### Check User Permissions
```sql
SHOW GRANTS FOR 'root'@'localhost';
```

**Expected:**
```
GRANT ALL PRIVILEGES ON *.* TO 'root'@'localhost' WITH GRANT OPTION
```

---

## ??? Fix Corrupted Installation

### Reinstall MySQL (Windows)
```bash
# Uninstall via Control Panel
# Delete C:\ProgramData\MySQL
# Delete C:\Program Files\MySQL
# Reinstall from https://dev.mysql.com/downloads/mysql/
```

### Repair MySQL (Linux)
```bash
sudo apt-get remove --purge mysql-server mysql-client mysql-common
sudo apt-get autoremove
sudo apt-get autoclean
sudo apt-get install mysql-server
```

---

## ?? Test Connection from App

### Create Test Console App
```csharp
using MySql.Data.MySqlClient;

var connectionString = "Server=localhost;Database=snooker_club_db;User=root;Password=YOUR_PASSWORD;";

try
{
    using var connection = new MySqlConnection(connectionString);
    connection.Open();
    Console.WriteLine("? Connection successful!");
    
    var command = new MySqlCommand("SELECT COUNT(*) FROM app_user", connection);
    var count = command.ExecuteScalar();
    Console.WriteLine($"? Found {count} users");
}
catch (Exception ex)
{
    Console.WriteLine($"? Error: {ex.Message}");
}
```

---

## ?? Pre-Deployment Checklist

### MySQL Server
- [ ] MySQL service is running
- [ ] Port 3306 is open
- [ ] Root password is known
- [ ] Firewall allows connections

### Database
- [ ] Database `snooker_club_db` exists
- [ ] All 10 tables exist
- [ ] Seed data is present
- [ ] Character set is utf8mb4

### Application
- [ ] `appsettings.json` exists
- [ ] Connection string is correct
- [ ] Password matches MySQL
- [ ] All NuGet packages restored
- [ ] Build is successful

### Testing
- [ ] Can connect via mysql CLI
- [ ] Can query database
- [ ] App launches without errors
- [ ] Login works
- [ ] Dashboard loads

---

## ?? Security Best Practices

### 1. Create Dedicated User
```sql
CREATE USER 'snooker_app'@'localhost' IDENTIFIED BY 'STRONG_PASSWORD';
GRANT ALL PRIVILEGES ON snooker_club_db.* TO 'snooker_app'@'localhost';
FLUSH PRIVILEGES;
```

### 2. Use Strong Password
```
? Bad: admin, password, 123456
? Good: kJ#8mP$2nQ@9xL
```

### 3. Restrict Remote Access (if not needed)
```sql
-- Only allow localhost
CREATE USER 'snooker_app'@'localhost' IDENTIFIED BY 'password';

-- Remove remote root access
DELETE FROM mysql.user WHERE User='root' AND Host NOT IN ('localhost', '127.0.0.1', '::1');
FLUSH PRIVILEGES;
```

### 4. Encrypt Connection String (Production)
```csharp
// Use Protected Configuration in production
// Encrypt sensitive sections of appsettings.json
```

---

## ?? Performance Tuning

### Optimize Connection Pool
```json
"SnookerDb": "Server=localhost;Database=snooker_club_db;User=root;Password=pass;Pooling=true;MinimumPoolSize=5;MaximumPoolSize=100;ConnectionTimeout=30"
```

### Enable Query Logging (Development)
```json
{
  "Logging": {
    "LogLevel": {
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

### Check Slow Queries
```sql
SHOW VARIABLES LIKE 'slow_query_log';
SET GLOBAL slow_query_log = 'ON';
SET GLOBAL long_query_time = 2;
```

---

## ?? Still Not Working?

### Collect Debug Information
```bash
# MySQL version
mysql --version

# Connection test
mysql -u root -p -e "SELECT VERSION();"

# List databases
mysql -u root -p -e "SHOW DATABASES;"

# Check process
netstat -ano | findstr 3306  # Windows
lsof -i :3306                # Linux/Mac
```

### Check Application Logs
```csharp
// Add detailed logging in App.xaml.cs
protected override void OnStartup(StartupEventArgs e)
{
    try
    {
        var connectionString = _configuration.GetConnectionString("SnookerDb");
        Console.WriteLine($"Connection String: {connectionString}");
        
        // Test connection
        using var context = ServiceProvider.GetRequiredService<SnookerDbContext>();
        context.Database.CanConnect();
        Console.WriteLine("? Database connection successful");
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Database Error: {ex.Message}\n\n{ex.StackTrace}", 
                       "Connection Error", 
                       MessageBoxButton.OK, 
                       MessageBoxImage.Error);
    }
}
```

---

## ?? Support Resources

### Official Documentation
- MySQL: https://dev.mysql.com/doc/
- EF Core: https://docs.microsoft.com/ef/core/
- Pomelo: https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql

### Community Help
- Stack Overflow: [mysql] [entity-framework-core]
- MySQL Forums: https://forums.mysql.com/

### Tools
- MySQL Workbench: GUI management
- HeidiSQL: Alternative client
- phpMyAdmin: Web-based management

---

## ? Success Indicators

When everything is working:
1. ? MySQL service shows "Running"
2. ? Can connect via `mysql -u root -p`
3. ? Database `snooker_club_db` exists
4. ? App launches without errors
5. ? Login window appears
6. ? Can login with admin/admin123
7. ? Dashboard loads with "+ Add Table" button

---

**?? Follow this guide step-by-step to resolve any connection issues!**
