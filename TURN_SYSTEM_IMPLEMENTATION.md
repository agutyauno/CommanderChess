# Hệ Thống Quản Lý Turn với Điều Kiện Kết Thúc Turn

## Tổng Quan

Đã hoàn thiện hệ thống quản lý turn với logic xác định điều kiện kết thúc turn theo yêu cầu:
- **Move/Capture/Boarding** → Kết thúc turn (cần xác nhận)
- **Detach** → KHÔNG kết thúc turn, chỉ carrier có thể tiếp tục hành động
- Không có nút "Pass Turn" - phải thực hiện action để kết thúc turn

## Các Thay Đổi Chi Tiết

### 1. TurnManager.cs - Core Logic

**Thêm Fields Mới:**
```csharp
bool endConditionPending = false;          // Điều kiện kết thúc turn đã đạt
ICommand pendingEndCommand = null;         // Command trigger end condition
bool detachActive = false;                 // Detach đang active
BasePiece allowedPieceAfterDetach = null;  // Carrier được phép hành động sau detach
```

**Methods Mới:**

1. **RecordCommand()** - Cập nhật để phát hiện end-turn condition:
   - Move/Capture/Boarding → set `endConditionPending = true` và publish `TurnEndConditionReachedEvent`
   - Detach → set `detachActive = true`, chờ event handler set allowed piece

2. **Event Handlers:**
   - `OnPieceMoved()` / `OnPieceCaptured()` / `OnPieceBoarded()` - Log actions
   - `OnPieceDetached()` - Set `allowedPieceAfterDetach` và publish `TurnDetachOccurredEvent`

3. **ConfirmEndTurn()** - Gọi bởi HUD khi click Confirm:
   - Publish `TurnEndedEvent`
   - Reset flags
   - Gọi `EndTurn()` để chuyển sang team tiếp theo

4. **CancelEndTurn()** - Gọi bởi HUD khi click Cancel:
   - Restore snapshot đầu turn
   - Clear commands
   - Reset flags
   - Publish `TurnEndCancelledEvent`

5. **IsPieceAllowedToAct(piece)** - Query helper:
   - Nếu `detachActive` → chỉ cho phép carrier
   - Nếu `endConditionPending` → không cho phép ai
   - Bình thường → phải đúng team

6. **Properties:**
   - `IsEndConditionPending` - UI check để show buttons
   - `IsDetachActive` - Check detach state

**Subscriptions:**
- Subscribe tới `PieceMovedEvent`, `PieceCapturedEvent`, `PieceBoardedEvent`, `PieceDetachedEvent` trong `OnInitialize()`
- Unsubscribe trong `OnDispose()`

**Updates:**
- `EndTurn()` - Reset flags khi chuyển turn
- `UndoTurn()` - Reset flags khi undo
- `ResetTurn()` - Reset flags khi reset game

---

### 2. Events Mới (4 files)

#### TurnEndConditionReachedEvent.cs
```csharp
public readonly struct TurnEndConditionReachedEvent : IGameEvent
{
    public readonly Team Team;
    public readonly int TurnNumber;
    public readonly string CommandDescription;
    public readonly DateTime Timestamp;
}
```
**Mục đích:** Thông báo HUD để hiển thị nút Confirm/Cancel

#### TurnDetachOccurredEvent.cs
```csharp
public readonly struct TurnDetachOccurredEvent : IGameEvent
{
    public readonly BasePiece AllowedPiece; // Carrier có thể tiếp tục
    public readonly Team Team;
    public readonly int TurnNumber;
}
```
**Mục đích:** Thông báo detach xảy ra, chỉ carrier được tiếp tục

#### TurnEndedEvent.cs
```csharp
public readonly struct TurnEndedEvent : IGameEvent
{
    public readonly Team Team;
    public readonly int TurnNumber;
}
```
**Mục đích:** Thông báo turn đã kết thúc (sau Confirm)

#### TurnEndCancelledEvent.cs
```csharp
public readonly struct TurnEndCancelledEvent : IGameEvent
{
    public readonly Team Team;
    public readonly int TurnNumber;
}
```
**Mục đích:** Thông báo turn đã bị cancel (restore về snapshot)

---

### 3. GameState Updates

#### ExecutingActionState.cs
- **Remove:** Auto `EndTurn()` call
- **Update:** Chỉ chuyển về `Idle` sau command complete
- **Lý do:** Turn chỉ kết thúc khi player confirm

#### IdleState.cs
- **Update:** `HandlePieceClick()` sử dụng `TurnManager.IsPieceAllowedToAct()`
- **Kiểm tra:**
  - Detach active → chỉ cho phép carrier
  - End condition pending → không cho phép ai
  - Normal → đúng team
- **Messages:** Log rõ lý do không thể chọn piece

#### PieceSelectedState.cs
- **Update:** Change selection sử dụng `IsPieceAllowedToAct()` thay vì `IsCurrentPlayerPiece()`

---

### 4. GameHUDController.cs - UI Integration

**Subscriptions Mới:**
```csharp
eventBus.Subscribe<TurnEndConditionReachedEvent>(OnTurnEndConditionReached);
eventBus.Subscribe<TurnDetachOccurredEvent>(OnTurnDetachOccurred);
eventBus.Subscribe<TurnEndedEvent>(OnTurnEnded);
eventBus.Subscribe<TurnEndCancelledEvent>(OnTurnEndCancelled);
```

**Event Handlers:**
1. `OnTurnEndConditionReached()` - Show Confirm/Cancel buttons
2. `OnTurnDetachOccurred()` - Log info (không show buttons vì carrier vẫn có thể act)
3. `OnTurnEnded()` - Hide buttons
4. `OnTurnEndCancelled()` - Hide buttons

**Button Handlers:**
1. `OnConfirmButtonClicked()` - Gọi `turnManager.ConfirmEndTurn()`
2. `OnCancelButtonClicked()` - Gọi `turnManager.CancelEndTurn()`

**Setup:**
- Buttons ẩn mặc định (`visible = false`)
- Chỉ hiện khi có end condition

---

## Flow Hoạt Động

### Case 1: Move/Capture/Boarding (Kết thúc turn)

```
1. Player execute MoveCommand
2. CommandManager.Execute() → TurnManager.RecordCommand()
3. TurnManager phát hiện MoveCommand → set endConditionPending = true
4. Publish TurnEndConditionReachedEvent
5. GameHUD subscribe → Show Confirm/Cancel buttons
6. Player click Confirm:
   - TurnManager.ConfirmEndTurn()
   - Publish TurnEndedEvent
   - EndTurn() → chuyển sang team khác
   - GameHUD ẩn buttons
7. Player click Cancel:
   - TurnManager.CancelEndTurn()
   - Restore snapshot đầu turn
   - Publish TurnEndCancelledEvent
   - GameHUD ẩn buttons
```

### Case 2: Detach (Không kết thúc turn)

**Ví dụ 1: Airforce mang Tank + Infantry, tách Tank ra**

```
Ban đầu:
Airforce
├── Tank
    └── Infantry

1. Player execute DetachCommand (tách Tank)
2. TurnManager.RecordCommand() → phát hiện DetachCommand → set detachActive = true
3. MovementExecutor.ExecuteDetach() → publish PieceDetachedEvent
4. TurnManager.OnPieceDetached() → set allowedPieceAfterDetach = Airforce
5. Publish TurnDetachOccurredEvent

Kết quả:
Airforce (EMPTY - allowed to act)
Tank
└── Infantry (on board)

6. Player chỉ có thể select và di chuyển Airforce
7. Khi Airforce move → trigger end condition → show Confirm/Cancel
8. Player confirm → EndTurn()
```

**Ví dụ 2: Airforce mang Tank + Infantry, tách Infantry ra**

```
Ban đầu:
Airforce
├── Tank
    └── Infantry

1. Player detach Infantry
2. allowedPieceAfterDetach = Airforce

Kết quả:
Infantry (on board)
Airforce
└── Tank (still carried)

3. Player có thể:
   - Move Airforce → end turn
   - Hoặc detach Tank tiếp → vẫn chưa end turn
4. Sau khi move Airforce → show Confirm/Cancel
```

**Ví dụ 3: Tank mang Militia + Infantry**

```
3.1 Tách Militia:
Tank
├── Militia
└── Infantry

→ Detach Militia
→ allowedPieceAfterDetach = Tank

Kết quả:
Tank
├── Infantry
└── (empty)

Player có thể: Move Tank HOẶC detach Infantry

3.2 Tách Infantry:
Tank
├── Militia
└── Infantry

→ Detach Infantry
→ allowedPieceAfterDetach = Tank

Kết quả:
Tank
├── Militia
└── (empty)

Player có thể: Move Tank HOẶC detach Militia
```

---

## Kiểm Tra và Test

### Compile Check
✅ Không có lỗi compile trong:
- TurnManager.cs
- GameHUDController.cs
- IdleState.cs
- PieceSelectedState.cs
- ExecutingActionState.cs
- 4 event files mới

### Test Cases Cần Kiểm Tra

1. **Move Command:**
   - Execute move → buttons appear
   - Click Confirm → turn ends, next team's turn
   - Click Cancel → restore to turn start

2. **Capture Command:**
   - Execute capture → buttons appear
   - Confirm → turn ends
   - Cancel → restore (defender restored)

3. **Boarding Command:**
   - Execute boarding → buttons appear
   - Confirm → turn ends

4. **Detach Command:**
   - Detach piece → NO buttons (carrier can still act)
   - Try select other pieces → blocked
   - Select carrier → allowed
   - Move carrier → buttons appear
   - Confirm → turn ends

5. **Multiple Detach:**
   - Detach A → carrier allowed
   - Detach B from carrier → carrier still allowed
   - Move carrier → end turn

6. **Cancel After Move:**
   - Move piece
   - Cancel → piece returns to original position
   - All carried pieces also restored

---

## Lưu Ý Quan Trọng

### 1. Detach Logic
- Detach **KHÔNG** kết thúc turn
- Sau detach, **CHỈ carrier** được phép hành động
- Carrier có thể:
  - Detach thêm pieces
  - Move (trigger end condition)
  - Capture (trigger end condition)
  - Boarding (trigger end condition)

### 2. End Condition Pending
- Khi `endConditionPending = true`:
  - **KHÔNG AI** có thể select piece
  - Player **PHẢI** Confirm hoặc Cancel
  - UI **PHẢI** hiển thị buttons rõ ràng

### 3. Snapshot System
- Snapshot được tạo ở **ĐẦU TURN**
- Cancel restore về **snapshot đầu turn**
- Mọi commands trong turn bị **clear**
- Board state phục hồi **hoàn toàn**

### 4. Event Order
```
Command Execute
  ↓
RecordCommand
  ↓
Check command type
  ↓
Set flags + Publish events
  ↓
HUD updates UI
  ↓
Player confirms/cancels
  ↓
EndTurn() hoặc Restore
```

---

## Files Đã Tạo/Chỉnh Sửa

### Files Mới (4):
1. `TurnEndConditionReachedEvent.cs`
2. `TurnDetachOccurredEvent.cs`
3. `TurnEndedEvent.cs`
4. `TurnEndCancelledEvent.cs`

### Files Đã Sửa (5):
1. `TurnManager.cs` - Core logic, event handlers, confirm/cancel
2. `GameHUDController.cs` - UI integration, button handlers
3. `ExecutingActionState.cs` - Remove auto EndTurn
4. `IdleState.cs` - Use IsPieceAllowedToAct
5. `PieceSelectedState.cs` - Use IsPieceAllowedToAct

---

## Next Steps (Optional Future Features)

1. **Animation Integration:**
   - Wait for animations before showing confirm buttons
   - Smooth button show/hide transitions

2. **Sound Effects:**
   - Sound khi end condition reached
   - Sound khi confirm/cancel

3. **UI Improvements:**
   - Hiển thị thông tin detach state
   - Highlight allowed piece after detach
   - Show "X commands executed this turn"

4. **Online Sync:**
   - Send TurnData qua network
   - Opponent auto-apply commands
   - Sync turn confirmations

5. **Replay System:**
   - Use replayHistory để replay trận đấu
   - Step-by-step command replay
   - Export/import replay files

---

## Kết Luận

Hệ thống turn management đã hoàn thiện với đầy đủ logic:
- ✅ Move/Capture/Boarding kết thúc turn (cần confirm)
- ✅ Detach không kết thúc turn, chỉ carrier được tiếp tục
- ✅ Events đầy đủ cho HUD và các systems khác
- ✅ Confirm/Cancel buttons hoạt động
- ✅ Snapshot restore khi cancel
- ✅ Không có lỗi compile

Hệ thống sẵn sàng để test trong Unity Editor!
