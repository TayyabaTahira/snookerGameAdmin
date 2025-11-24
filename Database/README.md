# Database Setup Instructions

## Prerequisites
1. Install MySQL Server (8.0 or higher)
2. Install MySQL Workbench (optional, for GUI management)

## Setup Steps

### 1. Install MySQL
Download from: https://dev.mysql.com/downloads/mysql/

### 2. Create Database
Open MySQL command line or Workbench and run:

```bash
mysql -u root -p < SnookerDB_Schema.sql
```

Or manually execute the script in MySQL Workbench.

### 3. Update Connection String
Update the connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "SnookerDb": "Server=localhost;Database=snooker_club_db;User=root;Password=YOUR_PASSWORD;"
  }
}
```

### 4. Default Admin Credentials
- **Username**: `admin`
- **Password**: `admin123`

?? **Change this password in production!**

## Database Structure

### Tables
- `app_user` - Admin authentication
- `game_type` - Game types (Single, Century, Doubles)
- `game_rule` - Pricing rules per game type
- `customer` - Players/Customers
- `session` - Active game sessions (virtual tables)
- `frame` - Individual game rounds
- `frame_participant` - Players in each frame
- `ledger_charge` - Customer charges
- `ledger_payment` - Customer payments
- `payment_allocation` - FIFO payment allocation

## Connection Details
- **Host**: localhost
- **Port**: 3306 (default)
- **Database**: snooker_club_db
- **Default User**: root
- **Charset**: utf8mb4

## Testing Connection
Run this query to verify:
```sql
SELECT * FROM app_user;
```
