# Snooker Game Management System - License & Database Configuration

This document explains how to configure the MAC address-based licensing and hybrid database connectivity features.

## 1. MAC Address-Based Licensing

### Overview
The application uses MAC address     to ensure it only runs on licensed machines. This prevents unauthorized distribution of your application.

### How It Works
- On startup, the application reads the licensed MAC address from `appsettings.json`
- It compares it with the current machine's MAC addresses
- If no match is found, the application prevents login

### Configuration Steps

#### Step 1: Get Client's MAC Address
On the client machine, you can:
1. Run the application once with an empty `License:MacAddress` in `appsettings.json`
2. The app will show the machine's MAC addresses in an error dialog
3. Share one of those MAC addresses with you

OR use Windows command:
```powershell
ipconfig /all
```
Look for "Physical Address" under your active network adapter.

#### Step 2: Configure License
Edit `appsettings.json`:
```json
{
  "License": {
    "MacAddress": "AA:BB:CC:DD:EE:FF"
  }
}
```

Replace `AA:BB:CC:DD:EE:FF` with the client's actual MAC address.

#### Step 3: Build and Distribute
1. Update `appsettings.json` with the client's MAC address
2. Build the application in Release mode
3. Distribute the entire output folder to the client
4. The application will only run on machines matching that MAC address

### Development Mode
- Leave `License:MacAddress` empty or remove it for development
- The application will allow access without validation

### Supported MAC Address Formats
- `AA:BB:CC:DD:EE:FF`
- `AA-BB-CC-DD-EE-FF`
- `AABBCCDDEEFF`

## 2. Hybrid Database Connectivity (Remote + Local Fallback)

### Overview
The application can automatically switch between a remote database and a local database based on internet connectivity. This ensures the application continues working even when the remote server is unavailable.

### How It Works
1. **Primary**: Application tries to connect to the remote database
2. **Fallback**: If remote fails, it automatically uses the local database
3. **Auto-Sync**: When connection is restored, local changes can be synced to remote

### Configuration Steps

#### Step 1: Setup Local Database
Install MySQL locally on the client machine:
```sql
CREATE DATABASE snooker_club_db;
-- Import your schema
mysql -u root -p snooker_club_db < schema.sql
```

#### Step 2: Setup Remote Database
On your remote server:
1. Install MySQL
2. Create the same database structure
3. Configure firewall to allow remote connections
4. Note the server IP/hostname, username, and password

#### Step 3: Configure Connection Strings
Edit `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "SnookerDb": "Server=localhost;Database=snooker_club_db;User=root;Password=root;AllowUserVariables=true;UseAffectedRows=false",
    "RemoteDb": "Server=your-server.com;Database=snooker_club_db;User=remote_user;Password=remote_password;AllowUserVariables=true;UseAffectedRows=false"
  }
}
```

**Connection String Parameters:**
- `SnookerDb`: Local database connection (fallback)
- `RemoteDb`: Remote database connection (primary)

#### Step 4: Test Connectivity
1. Start the application - it should show which database mode it's using
2. Disconnect from internet - application should fallback to local
3. Reconnect - application should switch back to remote

### Database Modes
- **Remote**: Connected to remote server (best for centralized data)
- **Local**: Connected to local database (offline mode)
- **Offline**: Cannot connect to either (error state)

### Current Status Display
The application shows the connection mode on startup:
```
? Connected to Remote Database
Users found: 5
Ready to use!
```

### Future Sync Implementation
The current implementation includes a placeholder for synchronization. To implement full sync:

1. Add sync tracking columns to your tables:
```sql
ALTER TABLE session ADD COLUMN sync_status VARCHAR(20) DEFAULT 'synced';
ALTER TABLE frame ADD COLUMN sync_status VARCHAR(20) DEFAULT 'synced';
-- Repeat for other tables
```

2. Mark records as 'unsynced' when created in local mode
3. Implement `SyncLocalToRemoteAsync()` method in `DatabaseSyncService.cs`
4. Sync unsynced records when remote connection is restored

## 3. Distribution Checklist

When distributing to a client:

### Files to Include
- [ ] Application executable and all DLLs
- [ ] `appsettings.json` with client's MAC address
- [ ] `appsettings.json` with correct connection strings
- [ ] Database schema SQL file (for local setup)
- [ ] This README file

### Configuration Template
```json
{
  "ConnectionStrings": {
    "SnookerDb": "Server=localhost;Database=snooker_club_db;User=root;Password=root;AllowUserVariables=true;UseAffectedRows=false",
    "RemoteDb": "Server=CLIENT_REMOTE_SERVER;Database=snooker_club_db;User=CLIENT_USER;Password=CLIENT_PASSWORD;AllowUserVariables=true;UseAffectedRows=false"
  },
  "License": {
    "MacAddress": "CLIENT_MAC_ADDRESS"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  }
}
```

### Installation Instructions for Client
1. Extract all files to a folder (e.g., `C:\SnookerGame\`)
2. Install MySQL Server locally
3. Import database schema: `mysql -u root -p < schema.sql`
4. Verify `appsettings.json` has correct MAC address
5. Update remote database connection string if needed
6. Run the application

## 4. Troubleshooting

### License Validation Fails
**Error**: "License Validation Failed"
**Solution**: 
- Verify the MAC address in `appsettings.json` matches client's MAC
- Check format: `AA:BB:CC:DD:EE:FF`
- Ensure you're using the primary network adapter's MAC

### Cannot Connect to Database
**Error**: "Database Connection Failed"
**Solution**:
- Verify MySQL is running: `sc query MySQL80`
- Check local connection string credentials
- For remote: verify firewall allows MySQL port (3306)
- Test remote connectivity: `telnet server_ip 3306`

### Application Won't Start
1. Check `appsettings.json` exists in the same folder as the .exe
2. Verify JSON syntax is correct
3. Review error messages carefully

## 5. Security Recommendations

### Production Deployment
1. **Strong Passwords**: Use strong passwords for database users
2. **Encrypted Connection**: Use SSL for remote database connections
3. **Firewall Rules**: Only allow specific IPs to access remote database
4. **Regular Backups**: Backup both local and remote databases regularly
5. **License Protection**: Don't share MAC addresses publicly

### Remote Database SSL
To enable SSL for remote connections:
```json
"RemoteDb": "Server=server.com;Database=snooker_club_db;User=user;Password=pass;SslMode=Required;AllowUserVariables=true"
```

## 6. Support

For issues or questions:
- Check the Debug output window in Visual Studio
- Review `[App]` log messages
- Contact: [Your Support Email/Details]
