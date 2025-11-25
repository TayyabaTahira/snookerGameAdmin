# Quick Test Guide - Table Detail Window

## ?? What's Now Working

### ? Fixed Issues:
1. Players list now shows data (or empty state)
2. No more `??` symbols - using clear text
3. Scrollbar only when needed
4. All buttons functional
5. Player add/remove works
6. Delete table option available

## ?? Quick Tests

### Test 1: Open Table (5 seconds)
```
Dashboard ? Click any tile ? ? Window opens
```

### Test 2: Add Player (10 seconds)
```
Click "+ Add Player" ? Type name ? Enter ? ? Player added
```

### Test 3: Remove Player (5 seconds)
```
Click "Remove" on player ? Yes ? ? Player removed
```

### Test 4: Quit Session (10 seconds)
```
Click "Quit Session" ? Yes ? ? Window closes, tile removed
```

### Test 5: Delete Table (10 seconds)
```
Click "Delete Table" ? Yes ? ? Success message, window closes
```

## ?? What You Should See Now

### Header Section:
```
Table #3                    00:23:26        [Delete Table]
Century                     Elapsed Time
```

### Players Section:
```
Players                                    [+ Add Player]
??????????????????????????????????????????????????????
? No players added yet. Click 'Add Player'...       ?
??????????????????????????????????????????????????????
```

### After Adding Players:
```
Players                                    [+ Add Player]
??????????????????????????????????????????????????????
? John Doe         Win Streak: 0         [Remove]   ?
? Jane Smith       Win Streak: 0         [Remove]   ?
??????????????????????????????????????????????????????
```

### Frames Section:
```
Frames Played
????????????
?    0     ?
????????????
```

### Session Info (NEW):
```
Session Information
Started At:      1/15/2025 10:30 AM
Game Type:       Century
Status:          In Progress
```

### Action Buttons:
```
[End Game]         [Next Frame]         [Quit Session]
```

## ?? Known Limitations

### TODO - End Game Button
- ? Button works
- ? Shows placeholder dialog
- ? Billing calculation not implemented
- ? Payment entry not implemented

### TODO - Next Frame Button
- ? Button works
- ? Shows placeholder dialog
- ? Winner selection not implemented
- ? Frame creation not implemented

### Current Behavior:
- Clicking "End Game" ? Shows info dialog
- Clicking "Next Frame" ? Shows info dialog
- These will be implemented in Phase 2

## ? Fully Working Features

1. **Window Opens** ?
2. **Timer Display** ?
3. **Player Management** ?
   - Add player
   - Remove player
   - Empty state
4. **Session Info Display** ?
5. **Quit Session** ?
   - Confirmation
   - Ends session
   - Closes window
   - Refreshes dashboard
6. **Delete Table** ?
   - Warning dialog
   - Ends session
   - Success message
   - Closes window
   - Refreshes dashboard

## ?? Visual Checklist

? No emoji/unicode issues  
? Scrollbar only when content overflows  
? Buttons have clear labels  
? Hover effects work  
? Color scheme consistent  
? Spacing and padding proper  
? Responsive layout  
? Modal dialogs centered  

## ?? Debugging Tips

### If players don't show:
1. Check if session has frames
2. Check if frame has participants
3. Debug `LoadPlayers()` method

### If buttons don't work:
1. Check command binding
2. Check CanExecute logic
3. Review error messages

### If dialog doesn't close:
1. Check DialogResult is set
2. Check window owner is set
3. Verify Close() is called

## ?? Test Data Requirements

### For Full Testing:
- ? Active session (table tile)
- ? Players added (via Add Player button)
- ? Frames played (via Next Frame - TODO)

### Current State:
- Sessions created without players
- Frames count = 0
- Players can be added via UI

## ?? Next Phase Features

### Phase 2 (Coming Soon):
1. Billing dialog implementation
2. Winner selection dialog
3. Frame creation logic
4. Win streak calculation
5. Real-time timer updates
6. Frame history view

---

**All Critical Issues Fixed** ?  
**Build Status:** Successful ?  
**Ready for Testing:** YES ??
