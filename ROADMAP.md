# ??? Development Roadmap

## Phase 1: Foundation ? COMPLETED

### ? Database Setup
- [x] MySQL schema with all tables
- [x] Seed data (admin, game types, rules, sample customers)
- [x] Indexes and foreign keys
- [x] Views for reporting

### ? Core Infrastructure
- [x] Entity Framework Core setup
- [x] DbContext with all entities
- [x] Dependency injection
- [x] Configuration management
- [x] BCrypt authentication

### ? Basic Services
- [x] AuthService (login)
- [x] SessionService (CRUD operations)
- [x] CustomerService (CRUD operations)
- [x] GameRuleService (read operations)

### ? UI Foundation
- [x] Login window with modern design
- [x] Dashboard window with tiles
- [x] MVVM architecture
- [x] Value converters
- [x] Responsive layout

---

## Phase 2: Session & Frame Management ?? NEXT

### 2.1 Session Popup Window
**Priority: HIGH**

**Tasks:**
- [ ] Create `SessionPopupWindow.xaml`
- [ ] Create `SessionPopupViewModel.cs`
- [ ] Design UI with:
  - Game type dropdown
  - Player selection/input
  - Timer display (HH:MM:SS)
  - Start/Pause buttons
  - End Game button
  - Next Frame button
  - Quit Session button

**Service Layer:**
- [ ] Create `FrameService.cs`
  - `CreateFrameAsync(sessionId, gameTypeId, players)`
  - `EndFrameAsync(frameId, winnerId, loserId)`
  - `GetFramesBySessionAsync(sessionId)`
  - `CalculateOvertimeAsync(frameId)`

**Database Operations:**
```csharp
// Example implementation
public async Task<Frame> CreateFrameAsync(string sessionId, string gameTypeId, List<string> playerIds)
{
    var rule = await GetRuleByGameTypeId(gameTypeId);
    var frame = new Frame
    {
        SessionId = sessionId,
        BaseRatePk = rule.BaseRatePk,
        StartedAt = DateTime.Now
    };
    
    // Add participants
    foreach (var playerId in playerIds)
    {
        frame.Participants.Add(new FrameParticipant
        {
            CustomerId = playerId,
            FrameId = frame.Id
        });
    }
    
    await _context.Frames.AddAsync(frame);
    await _context.SaveChangesAsync();
    return frame;
}
```

---

### 2.2 Timer Implementation
**Priority: HIGH**

**Tasks:**
- [ ] Create `TimerService.cs`
- [ ] Implement `DispatcherTimer` in ViewModel
- [ ] Update UI every second
- [ ] Persist timer state in database
- [ ] Handle app restart (resume timer)

**Code Snippet:**
```csharp
public class TimerService
{
    private DispatcherTimer _timer;
    private DateTime _startTime;
    
    public event EventHandler<TimeSpan>? TimerTick;
    
    public void Start(DateTime startTime)
    {
        _startTime = startTime;
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += (s, e) => TimerTick?.Invoke(this, GetElapsed());
        _timer.Start();
    }
    
    public TimeSpan GetElapsed() => DateTime.Now - _startTime;
}
```

---

### 2.3 Player Selection
**Priority: MEDIUM**

**Tasks:**
- [ ] Create `PlayerSelectionControl.xaml` (reusable UserControl)
- [ ] AutoComplete textbox for existing customers
- [ ] "Add New" customer inline
- [ ] Support Singles (2 players) and Doubles (4 players)
- [ ] Team selection for doubles (Team A, Team B)

**UI Components:**
```xaml
<UserControl x:Class="...PlayerSelectionControl">
    <StackPanel>
        <ComboBox ItemsSource="{Binding Customers}"
                  DisplayMemberPath="FullName"
                  IsEditable="True"
                  Text="{Binding SelectedPlayerName}"/>
        <Button Content="Add New Customer" Command="{Binding AddNewCustomerCommand}"/>
    </StackPanel>
</UserControl>
```

---

## Phase 3: Billing System ?? IMPORTANT

### 3.1 Billing Calculation Service
**Priority: HIGH**

**Tasks:**
- [ ] Create `BillingService.cs`
- [ ] Implement overtime calculation logic
- [ ] Support all payer modes
- [ ] Apply discounts

**Implementation:**
```csharp
public class BillingService
{
    public async Task<BillingDetails> CalculateBillAsync(Frame frame, GameRule rule)
    {
        var elapsed = (frame.EndedAt ?? DateTime.Now) - frame.StartedAt;
        var elapsedMinutes = (int)Math.Ceiling(elapsed.TotalMinutes);
        
        decimal overtimeAmount = 0;
        int overtimeMinutes = 0;
        
        if (rule.TimeLimitMinutes.HasValue && elapsedMinutes > rule.TimeLimitMinutes.Value)
        {
            overtimeMinutes = elapsedMinutes - rule.TimeLimitMinutes.Value;
            
            overtimeAmount = rule.OvertimeMode switch
            {
                OvertimeMode.PER_MINUTE => overtimeMinutes * (rule.OvertimeRatePkMin ?? 0),
                OvertimeMode.LUMP_SUM => rule.OvertimeLumpSumPk ?? 0,
                _ => 0
            };
        }
        
        var total = rule.BaseRatePk + overtimeAmount - frame.DiscountPk;
        
        return new BillingDetails
        {
            BaseRate = rule.BaseRatePk,
            OvertimeMinutes = overtimeMinutes,
            OvertimeAmount = overtimeAmount,
            Discount = frame.DiscountPk,
            TotalAmount = total
        };
    }
}
```

---

### 3.2 Billing Popup Window
**Priority: HIGH**

**Tasks:**
- [ ] Create `BillingWindow.xaml`
- [ ] Create `BillingViewModel.cs`
- [ ] Display:
  - Base rate
  - Overtime breakdown
  - Discount input
  - Total amount
  - Payer mode selection
  - Payment method
- [ ] Actions:
  - Pay Now button
  - Credit button
  - Cancel button

**UI Layout:**
```
???????????????????????????????????
?       Billing Summary           ?
???????????????????????????????????
? Base Rate:         Rs 500       ?
? Overtime (5 min):  Rs 25        ?
? Discount:          Rs 0    [??] ?
???????????????????????????????????
? Total:             Rs 525       ?
???????????????????????????????????
? Payer Mode: ? Loser             ?
?             ? Split              ?
?             ? Custom             ?
???????????????????????????????????
? [?? Pay Now]  [?? Credit]       ?
???????????????????????????????????
```

---

### 3.3 Ledger & Payment Service
**Priority: HIGH**

**Tasks:**
- [ ] Create `LedgerService.cs`
- [ ] Implement `CreateChargeAsync(customerId, frameId, amount)`
- [ ] Implement `CreatePaymentAsync(customerId, amount)`
- [ ] Implement FIFO allocation logic
- [ ] Update frame pay status

**FIFO Allocation Algorithm:**
```csharp
public async Task AllocatePaymentAsync(string paymentId, decimal paymentAmount)
{
    var payment = await _context.LedgerPayments.FindAsync(paymentId);
    
    // Get unpaid charges (oldest first)
    var unpaidCharges = await _context.LedgerCharges
        .Where(c => c.CustomerId == payment.CustomerId)
        .OrderBy(c => c.CreatedAt)
        .ToListAsync();
    
    decimal remaining = paymentAmount;
    
    foreach (var charge in unpaidCharges)
    {
        if (remaining <= 0) break;
        
        // Calculate already paid amount for this charge
        var alreadyPaid = await _context.PaymentAllocations
            .Where(a => a.ChargeId == charge.Id)
            .SumAsync(a => a.AllocatedAmountPk);
        
        var chargeRemaining = charge.AmountPk - alreadyPaid;
        
        if (chargeRemaining > 0)
        {
            var allocateAmount = Math.Min(remaining, chargeRemaining);
            
            // Create allocation
            _context.PaymentAllocations.Add(new PaymentAllocation
            {
                PaymentId = paymentId,
                ChargeId = charge.Id,
                AllocatedAmountPk = allocateAmount
            });
            
            remaining -= allocateAmount;
            
            // Update frame pay status if needed
            await UpdateFramePayStatusAsync(charge.FrameId);
        }
    }
    
    await _context.SaveChangesAsync();
}
```

---

## Phase 4: Customer Management ??

### 4.1 Customer List Window
**Priority: MEDIUM**

**Tasks:**
- [ ] Create `CustomerListWindow.xaml`
- [ ] Display customers in DataGrid
- [ ] Show balance, total games, credit
- [ ] Search/filter functionality
- [ ] Edit customer details
- [ ] View customer history

---

### 4.2 Customer Details Window
**Priority: MEDIUM**

**Tasks:**
- [ ] Create `CustomerDetailsWindow.xaml`
- [ ] Show comprehensive history
- [ ] Display all frames played
- [ ] Show payment timeline
- [ ] Outstanding balance
- [ ] Accept payment button

---

### 4.3 Payment Recording
**Priority: MEDIUM**

**Tasks:**
- [ ] Create `RecordPaymentWindow.xaml`
- [ ] Enter payment amount
- [ ] Select payment method
- [ ] Auto-allocate using FIFO
- [ ] Show allocation breakdown
- [ ] Update balances

---

## Phase 5: Reporting & Analytics ??

### 5.1 Daily Reports
- [ ] Daily revenue summary
- [ ] Tables usage
- [ ] Top players
- [ ] Outstanding credits

### 5.2 Monthly Reports
- [ ] Monthly revenue trends
- [ ] Customer activity
- [ ] Peak hours analysis
- [ ] Payment collection rate

### 5.3 Customer Reports
- [ ] Win/loss streaks
- [ ] Most frequent players
- [ ] Credit history
- [ ] Payment patterns

---

## Phase 6: Advanced Features ??

### 6.1 Streak Tracking
- [ ] Real-time streak display
- [ ] Streak badges
- [ ] Leaderboard
- [ ] Historical streaks

### 6.2 Invoice Generation
- [ ] PDF generation
- [ ] Email invoices
- [ ] Print receipts
- [ ] Invoice templates

### 6.3 Settings & Configuration
- [ ] Game rules editor
- [ ] User management
- [ ] Backup/restore
- [ ] Theme selection

---

## Phase 7: Optional Cloud Sync ??

### 7.1 API Development
- [ ] REST API for sync
- [ ] Authentication
- [ ] Data endpoints
- [ ] Conflict resolution

### 7.2 Sync Service
- [ ] Background sync worker
- [ ] Connectivity check
- [ ] Sync status indicator
- [ ] Manual sync button

---

## ?? Implementation Order (Recommended)

### Week 1-2: Session Management
1. Session popup window
2. Timer implementation
3. Player selection
4. Frame creation

### Week 3-4: Billing
1. Billing calculation service
2. Billing popup window
3. Ledger service
4. FIFO allocation
5. Payment recording

### Week 5-6: Customer Management
1. Customer list window
2. Customer details window
3. Payment recording
4. Balance tracking

### Week 7-8: Reporting
1. Daily reports
2. Monthly reports
3. Customer reports
4. Export functionality

### Week 9-10: Polish & Testing
1. Bug fixes
2. UI improvements
3. Performance optimization
4. Testing

---

## ?? Testing Checklist

### Unit Tests
- [ ] Service layer methods
- [ ] Billing calculations
- [ ] FIFO allocation
- [ ] Authentication

### Integration Tests
- [ ] Database operations
- [ ] End-to-end workflows
- [ ] Payment processing

### UI Tests
- [ ] Navigation
- [ ] Form validation
- [ ] Data binding
- [ ] Error handling

---

## ?? Documentation To Create

- [ ] API documentation (XML comments)
- [ ] User manual
- [ ] Admin guide
- [ ] Troubleshooting guide
- [ ] Deployment guide

---

## ?? Success Metrics

- ? Zero data loss
- ? < 100ms UI response time
- ? 99.9% uptime
- ? < 5 seconds to create session
- ? < 3 seconds billing calculation

---

**Start with Phase 2.1 - Session Popup Window! ??**
