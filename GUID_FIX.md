# GUID to String Conversion Fix

## Problem
The application was throwing `InvalidCastException: Unable to cast object of type 'System.Guid' to type 'System.String'` during authentication.

## Root Cause
MySQL with Pomelo.EntityFrameworkCore.MySql was storing `CHAR(36)` columns as actual GUID binary values instead of strings. When EF Core tried to read these values into C# `string` properties, it failed with a cast exception.

## Solution
Created a custom `ValueConverter` that properly converts between `Guid` (database) and `string` (C# model):

### 1. Created `GuidToStringConverter.cs`
```csharp
public class GuidToStringConverter : ValueConverter<string, Guid>
{
    public GuidToStringConverter() 
        : base(
            v => Guid.Parse(v),           // Convert string to Guid for database
            v => v.ToString())            // Convert Guid to string for C#
    {
    }
}
```

### 2. Applied to All Entities
Updated `SnookerDbContext.cs` to use the converter for ALL `char(36)` columns:
- `AppUser.Id`
- `GameType.Id`  
- `GameRule.Id` and `GameRule.GameTypeId`
- `Customer.Id`
- `Session.Id` and `Session.GameTypeId`
- `Frame.Id`, `Frame.SessionId`, `Frame.WinnerCustomerId`, `Frame.LoserCustomerId`
- `FrameParticipant.Id`, `FrameParticipant.FrameId`, `FrameParticipant.CustomerId`
- `LedgerCharge.Id`, `LedgerCharge.CustomerId`, `LedgerCharge.FrameId`
- `LedgerPayment.Id`, `LedgerPayment.CustomerId`
- `PaymentAllocation.Id`, `PaymentAllocation.PaymentId`, `PaymentAllocation.ChargeId`

### 3. Fixed App.xaml.cs Syntax Errors
Removed duplicate/malformed configuration lines in `ConfigureServices` method.

## Files Changed
1. **NEW**: `Data/GuidToStringConverter.cs` - Custom value converter
2. **MODIFIED**: `Data/SnookerDbContext.cs` - Applied converter to all entities
3. **MODIFIED**: `App.xaml.cs` - Fixed syntax errors

## How It Works
1. **Reading from DB**: When EF Core reads a `Guid` from MySQL, the converter transforms it to a `string`
2. **Writing to DB**: When EF Core writes a `string`, the converter parses it to a `Guid` for MySQL
3. **Type Safety**: C# models use `string`, MySQL uses `Guid` binary format, converter handles translation

## Testing
? **Build**: Successful
? **Compilation**: No errors
? **Ready to Run**: Yes

## Next Steps
1. **Rebuild the application**:
   ```bash
   dotnet clean
   dotnet build
   dotnet run
   ```

2. **Test Login**:
   - Username: `admin`
   - Password: `admin123`

The login should now work properly without any cast exceptions!

## Why This Approach?
- **Database Independent**: MySQL stores GUIDs, C# uses strings - converter bridges the gap
- **Type Safe**: Strong typing on both sides
- **Explicit**: Clear intent in code that conversion is happening
- **Maintainable**: Single converter class used everywhere
