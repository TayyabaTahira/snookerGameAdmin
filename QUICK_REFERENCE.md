# Quick Reference Guide - Snooker Game Management System

## ?? What's Working Now

### ? Dashboard
- Click "+ Add Table" ? Opens create dialog
- Click any table tile ? Opens table details
- Refresh button reloads active sessions
- Loading overlay prevents interaction during load

### ? Create Table Dialog
- **Table Name**: Auto-filled (e.g., "Table #4")
- **Game Type**: Dropdown with white text (readable!)
- **Players**: 
  - Type name ? Click "? Add" or press Enter
  - Click "?" to remove player
  - Shows "No players added yet" when empty
- **Create Button**: Only enabled when game type selected

### ? Table Detail Window
- Shows table name, game type, timer
- Displays players (with placeholder streaks)
- Shows frame count
- Has 3 action buttons (to be implemented)

## ?? Issues Fixed

1. **ComboBox White-on-White Text** ?
   - Custom template with explicit foreground colors
   - Dropdown items have hover effect (red background)

2. **Hidden Players Section** ?
   - Fully visible with add/remove UI
   - Scrollable list
   - Visual feedback

3. **Overlapping Add Button** ?
   - Now first item in WrapPanel
   - Properly aligned with tiles

4. **Dialog Z-Index** ?
   - Proper modal dialogs
   - Owner window set correctly

## ?? How to Test

### Test Flow 1: Create Table
```
1. Run app ? Login (admin/Admin@123)
2. Click "+ Add Table"
3. Select "Single" from Game Type
4. (Optional) Add player: Type "John Doe" ? Enter
5. Click "Create Table"
6. ? New tile appears on dashboard
```

### Test Flow 2: Open Table
```
1. From dashboard, click any table tile
2. ? Detail window opens
3. ? Shows timer, players, frames
4. Click any button (shows TODO for now)
5. Close window
```

### Test Flow 3: Player Management
```
1. Click "+ Add Table"
2. Add multiple players:
   - Type "Player 1" ? Click Add
   - Type "Player 2" ? Press Enter
   - Type "Player 3" ? Click Add
3. ? All players appear in list
4. Click ? on "Player 2"
5. ? Player 2 removed
6. Cancel or Create
```

## ?? Key Files to Know

### For UI Changes
- `Views/DashboardWindow.xaml` - Main dashboard layout
- `Views/CreateSessionDialog.xaml` - Create table dialog
- `Views/TableDetailWindow.xaml` - Table management window

### For Logic Changes
- `ViewModels/DashboardViewModel.cs` - Dashboard logic
- `ViewModels/CreateSessionViewModel.cs` - Dialog logic
- `ViewModels/TableDetailViewModel.cs` - Table detail logic

### For Database Changes
- `Services/SessionService.cs` - Session CRUD
- `Services/CustomerService.cs` - Customer CRUD
- `Services/GameTypeService.cs` - Game type queries

## ?? Color Scheme Reference

```csharp
Primary Background:   #1a1a2e   (Dark blue)
Card Background:      #16213e   (Medium blue)
Darker Background:    #0f3460   (Deep blue)
Border Color:         #3a3a4e   (Gray blue)
Accent/Primary:       #e94560   (Red/Pink)
Text Primary:         White
Text Secondary:       #a0a0b0   (Light gray)
Text Disabled:        #6a6a7e   (Medium gray)
```

## ?? Next Features to Implement

### Priority 1: Frame Management
**File**: `ViewModels/TableDetailViewModel.cs`
```csharp
private async Task NextFrame()
{
    // TODO: Show winner selection dialog
    // TODO: Create new frame in database
    // TODO: Update participants
    // TODO: Refresh view
}
```

### Priority 2: Billing System
**New File**: `Views/BillingDialog.xaml`
```xaml
<!-- Show:
  - Base rate from game_rule
  - Overtime calculation
  - Discount input
  - Total amount
  - Payer selection (Loser/Split/Custom)
  - Pay Now / Credit buttons
-->
```

### Priority 3: Customer Management
**New File**: `Views/CustomerManagementWindow.xaml`
```xaml
<!-- Show:
  - Customer list
  - Balance summary
  - Payment history
  - Add payment button
-->
```

## ?? Common Issues & Solutions

### Issue: ComboBox text not visible
**Solution**: Already fixed in `CreateSessionDialog.xaml` with custom template

### Issue: Players section collapsed
**Solution**: Already fixed - section is fully visible with controls

### Issue: Dialog behind main window
**Solution**: Already fixed - `Owner = Application.Current.MainWindow`

### Issue: Can't click table tiles
**Solution**: Already fixed - `MouseLeftButtonUp="SessionTile_Click"`

## ?? Database Tables Used

Currently:
- ? `session` - For table sessions
- ? `game_type` - For game types
- ? `customer` - Not yet used (add via player management)
- ? `frame` - Not yet created (needs frame start logic)
- ? `frame_participant` - Not yet created
- ? `ledger_charge` - Not yet created (needs billing)
- ? `ledger_payment` - Not yet created

## ?? Controls & Shortcuts

### Keyboard Shortcuts
- **Enter** in player name ? Adds player
- **ESC** in dialog ? Cancels (standard)

### Mouse Actions
- **Click** table tile ? Opens detail
- **Hover** buttons ? Color change
- **Click** ? ? Removes player

## ?? Notes for Development

1. **Always reload relationships** after creating entities:
   ```csharp
   var session = await _context.Sessions
       .Include(s => s.GameType)
       .Include(s => s.Frames)
       .FirstOrDefaultAsync(s => s.Id == sessionId);
   ```

2. **Use RelayCommand** for all commands:
   ```csharp
   AddCommand = new RelayCommand(async _ => await Add(), _ => CanAdd);
   ```

3. **Update UI on main thread**:
   ```csharp
   await Application.Current.Dispatcher.InvokeAsync(() => {
       Sessions.Add(newSession);
   });
   ```

4. **Check for null** before accessing navigation properties:
   ```csharp
   string gameType = session.GameType?.Name ?? "Not Set";
   ```

---

**Ready to Test!** ??

Build Status: ? Successful  
All UI Issues: ? Fixed  
Documentation Flow: ? Followed  
