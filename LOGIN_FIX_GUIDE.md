# ?? LOGIN TROUBLESHOOTING - STEP BY STEP

## ?? Follow These Steps IN ORDER

### STEP 1: Run Diagnostic Script
```cmd
diagnose_login.bat
```

**What it does:**
- Checks MySQL service
- Checks database exists
- Checks tables exist (should be 10+)
- Checks admin user exists
- Checks password hash
- Checks appsettings.json
- Checks if app is built

**Expected output:** All tests should show [PASS]

---

### STEP 2: If Tables Don't Exist - Import Schema
```cmd
"C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe" -u root -proot snooker_club_db < Database\SnookerDB_Schema.sql
```

**Verify import:**
```cmd
"C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe" -u root -proot snooker_club_db -e "SELECT COUNT(*) FROM app_user;"
```

Should show: `1` (one admin user)

---

### STEP 3: Clean Build
```cmd
dotnet clean
dotnet build
```

---

### STEP 4: Run Application
```cmd
dotnet run
```

**OR** press F5 in Visual Studio

---

### STEP 5: Check for Error Messages

When app starts, you should see ONE of these:

#### ? SUCCESS:
- Login window appears
- No error messages
- Debug output shows: "Database connection successful! Users: 1"

#### ? ERROR SCENARIO 1 - "appsettings.json not found"
**Fix:** 
```cmd
copy appsettings.json bin\Debug\net10.0-windows\
dotnet run
```

#### ? ERROR SCENARIO 2 - "Database Connection Failed"
**Fix:**
- Check MySQL is running: `sc query MySQL80`
- If stopped: `net start MySQL80`
- Try again

#### ? ERROR SCENARIO 3 - "Database connected but no users found"
**Fix:**
- Import schema (see STEP 2)
- Restart app

---

### STEP 6: Login with Correct Credentials

```
Username: admin
Password: admin123
```

?? **IMPORTANT**: 
- Password is `admin123` (with THREE at the end)
- NOT `admin12` or `admin`
- Username is case-sensitive

---

### STEP 7: Watch Debug Output

While app is running, check Visual Studio **Output** window:

**View ? Output** (or Ctrl+Alt+O)

Select "Debug" from dropdown

You should see:
```
[App] Testing database connection...
[App] Database connection successful! Found 1 users.
? Database connection successful! Users: 1
```

When you click LOGIN:
```
CanLogin: true (User: 'admin', Pass: 8 chars, Loading: False)
[AuthService] Attempting authentication for user: admin
[AuthService] User found. Hash: $2a$12$LQv3c1yqBWVHx...
[AuthService] Password verification result: True
Login successful!
```

---

## ?? Common Issues & Fixes

### Issue 1: Button Stays Disabled

**Check:**
1. Did you type BOTH username AND password?
2. Check Output window for: `CanLogin: false`

**Fix:**
- Close app completely
- Run: `dotnet clean`
- Run: `dotnet build`
- Run: `dotnet run`

---

### Issue 2: "Invalid username or password"

**Cause:** Either:
- Password is wrong (should be `admin123`)
- Schema not imported
- Password hash is wrong

**Fix:**
```cmd
"C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe" -u root -proot snooker_club_db -e "SELECT username, SUBSTRING(password_hash,1,20) FROM app_user;"
```

Should show:
```
admin | $2a$12$LQv3c1yqBWVH
```

If different, re-import schema.

---

### Issue 3: App Crashes on Startup

**Check:**
- Error message will tell you what's wrong
- Check diagnose_login.bat output

**Common causes:**
- MySQL not running
- Database doesn't exist
- appsettings.json missing

---

## ?? Quick Verification Checklist

Run these commands and check all pass:

```cmd
REM 1. MySQL running?
sc query MySQL80 | find "RUNNING"

REM 2. Database exists?
"C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe" -u root -proot -e "SHOW DATABASES LIKE 'snooker_club_db';"

REM 3. Tables exist?
"C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe" -u root -proot snooker_club_db -e "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema='snooker_club_db';"

REM 4. Admin user exists?
"C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe" -u root -proot snooker_club_db -e "SELECT * FROM app_user;"

REM 5. App built?
dir bin\Debug\net10.0-windows\SnookerGameManagementSystem.exe

REM 6. Config in output?
dir bin\Debug\net10.0-windows\appsettings.json
```

All should succeed.

---

## ?? FINAL TEST

1. Run `diagnose_login.bat` - all [PASS]
2. Run `dotnet run`
3. Type: `admin`
4. Type: `admin123`
5. Click LOGIN
6. Should open dashboard

---

## ?? If Still Not Working

Send me the output of:
1. `diagnose_login.bat`
2. Visual Studio Output window (Debug section)
3. Any error messages you see

---

**START WITH STEP 1!** ??
