# Hệ Thống Detach từ Info Panel

## Tổng Quan

Đã hoàn thiện việc lựa chọn piece trong info panel để detach với flow:
1. Click vào carried piece trong info panel
2. Hiển thị các vị trí detach hợp lệ (ô trống xung quanh carrier)
3. Click vào vị trí hợp lệ để execute detach
4. Sau detach, carrier được auto-select để tiếp tục hành động

---

## Files Đã Tạo/Chỉnh Sửa

### 1. SelectingDetachTargetState.cs (MỚI)
**Location:** `Assets/Scripts/CommanderChess/GameState/States/`

**Chức năng:**
- State mới để handle việc chọn vị trí detach
- Hiển thị các ô trống xung quanh carrier (8 directions)
- Execute DetachCommand khi click vào vị trí hợp lệ
- Auto-select carrier nếu click vào carrier hoặc deselect

**Key Methods:**
```csharp
Enter()
  - Tính toán các vị trí adjacent hợp lệ xung quanh carrier
  - Highlight các vị trí đó trên board
  
HandleBoardClick(coord)
  - Validate vị trí click
  - TryExecuteDetach(coord)
  
HandlePieceClick(piece)
  - Click carrier → SelectCarrier() → back to PieceSelectedState
  - Click same passenger → SelectCarrier()
  
HandleCancel()
  - Cancel detach → SelectCarrier()
```

---

### 2. GameStateManager.cs (CẬP NHẬT)

**Thêm State Registration:**
```csharp
void InitializeStates()
{
    // ...
    states[GameState.SelectingDetachTarget] = new SelectingDetachTargetState(stateData, this);
}
```

**Thêm Methods Mới:**

#### `SelectPieceForDetach(BasePiece piece)`
- Được gọi từ GameHUDController khi click carried piece trong panel
- Validate piece có đang được carry không
- Set `stateData.SelectedPiece` và transition tới `SelectingDetachTarget`

#### `SelectPiece(BasePiece piece)`
- Được gọi khi click carrier hoặc standalone piece trong panel
- Validate piece có được phép hành động không (via TurnManager)
- Transition tới `PieceSelected` state

---

### 3. GameHUDController.cs (CẬP NHẬT)

**Cập nhật `OnInfoPanelItemClicked(piece)`:**

**Logic Flow:**
```
Click piece trong info panel
  ↓
if (currentSelectedPiece == piece)  // Click lại chính piece đó
  ↓
  if (isCarried)
    → Select carrier instead
  else
    → Deselect và về Idle
  ↓
else  // Click piece khác
  ↓
  Update UI selection
  ↓
  if (isCarried)
    → gameStateManager.SelectPieceForDetach(piece)
    → Enter SelectingDetachTargetState
  else
    → gameStateManager.SelectPiece(piece)
    → Enter PieceSelectedState
```

**Changes:**
```csharp
void OnInfoPanelItemClicked(BasePiece piece)
{
    // Deselect logic
    if (currentSelectedPiece == piece)
    {
        if (carryingSystem.IsCarried(piece))
        {
            // Go back to carrier
            var carrier = carryingSystem.GetCarrier(piece);
            gameStateManager.SelectPiece(carrier);
        }
        else
        {
            // Deselect completely
            gameStateManager.CancelCurrentAction();
        }
        return;
    }

    // Selection logic
    currentSelectedPiece = piece;
    UpdatePieceSelection();

    if (carryingSystem.IsCarried(piece))
    {
        gameStateManager.SelectPieceForDetach(piece);
    }
    else
    {
        gameStateManager.SelectPiece(piece);
    }
}
```

---

### 4. ExecutingActionState.cs (CẬP NHẬT)

**Auto-Select Carrier After Detach:**

```csharp
private void OnCommandCompleted()
{
    var lastCommand = Manager.CommandManager.GetLastCommand();
    
    // Check if detach command
    if (lastCommand?.GetType().Name.Contains("DetachCommand") == true)
    {
        if (Manager.TurnManager.IsDetachActive)
        {
            // Find carrier at selected position
            if (Manager.Board.TryGetPiece(Data.SelectedPosition, out var carrier))
            {
                // Auto-select carrier
                Data.SelectedPiece = carrier;
                Data.SelectedPosition = carrierPos;
                Manager.ChangeState(GameState.PieceSelected);
                return;
            }
        }
    }

    // Normal flow
    Data.Clear();
    Manager.ChangeState(GameState.Idle);
}
```

---

## Flow Hoạt Động Chi Tiết

### Case 1: Detach Infantry từ Tank

**Initial State:**
```
Tank (at A5)
└── Infantry
```

**Steps:**

1. **User clicks Infantry trong info panel**
```
GameHUDController.OnInfoPanelItemClicked(Infantry)
  ↓
Check: Infantry.IsCarried = true
  ↓
gameStateManager.SelectPieceForDetach(Infantry)
  ↓
State: SelectingDetachTarget
```

2. **SelectingDetachTargetState.Enter()**
```
- passengerToDetach = Infantry
- carrier = Tank (from CarryingSystem)
- Calculate adjacent positions around A5:
  [A4, A6, B5, @4, B4, B6, @4, @6]
- Filter empty positions only
- Highlight valid positions (e.g., A4, B5, B6)
```

3. **User clicks B5 (valid position)**
```
HandleBoardClick(B5)
  ↓
Check: B5 in highlightedMoves? Yes
  ↓
TryExecuteDetach(B5)
  ↓
Validate: ActionValidator.ValidateDetach(Tank, Infantry, B5)
  ↓
Create: DetachCommand(A5, B5)
  ↓
Execute command
  ↓
State: ExecutingAction
```

4. **ExecutingActionState completes**
```
OnCommandCompleted()
  ↓
Check: lastCommand is DetachCommand? Yes
  ↓
Check: TurnManager.IsDetachActive? Yes
  ↓
Find carrier at A5 → Tank
  ↓
Auto-select Tank
  ↓
State: PieceSelected (with Tank selected)
```

**Result:**
```
Infantry (at B5 - detached)
Tank (at A5 - selected, ready for next action)
```

---

### Case 2: Click lại chính piece đó để deselect

**Scenario:** User clicks Infantry trong panel, sau đó click Infantry lại

```
First click Infantry
  ↓
State: SelectingDetachTarget (waiting for position)
  ↓
Second click Infantry trong panel
  ↓
OnInfoPanelItemClicked(Infantry)
  ↓
Check: currentSelectedPiece == Infantry? Yes
  ↓
Check: Infantry.IsCarried? Yes
  ↓
Get carrier = Tank
  ↓
Select Tank instead
  ↓
gameStateManager.SelectPiece(Tank)
  ↓
State: PieceSelected (Tank selected)
```

---

### Case 3: Cancel detach bằng ESC hoặc click carrier

**Option A: Click Tank trong panel**
```
State: SelectingDetachTarget (Infantry waiting for position)
  ↓
Click Tank trong info panel
  ↓
OnInfoPanelItemClicked(Tank)
  ↓
currentSelectedPiece = Tank (different from Infantry)
  ↓
Tank.IsCarried? No
  ↓
gameStateManager.SelectPiece(Tank)
  ↓
State: PieceSelected (Tank selected)
```

**Option B: ESC key or Cancel button**
```
State: SelectingDetachTarget
  ↓
User presses ESC / click cancel
  ↓
gameStateManager.CancelCurrentAction()
  ↓
SelectingDetachTargetState.HandleCancel()
  ↓
SelectCarrier()
  ↓
State: PieceSelected (Tank selected)
```

---

### Case 4: Nested Carrying - Airforce/Tank/Infantry

```
Airforce (A5)
└── Tank
    └── Infantry
```

**Click Infantry trong panel:**
```
OnInfoPanelItemClicked(Infantry)
  ↓
Infantry.IsCarried? Yes
  ↓
gameStateManager.SelectPieceForDetach(Infantry)
  ↓
SelectingDetachTargetState.Enter()
  ↓
carrier = CarryingSystem.GetCarrier(Infantry) → Tank
  ↓
Calculate positions around Tank.Position (A5)
  ↓
Highlight valid positions
```

**Result:** Infantry sẽ detach từ Tank, nhưng vị trí detach vẫn là xung quanh Airforce (vì Tank ở position A5 cùng với Airforce)

**Note:** Nếu muốn detach Infantry, phải detach Tank trước (hoặc logic phức tạp hơn)

---

## Validation & Edge Cases

### 1. **No Valid Detach Positions**
```
State: SelectingDetachTarget
  ↓
Calculate adjacent positions
  ↓
All positions occupied or out of board
  ↓
highlightedMoves.Count == 0
  ↓
Log warning + ChangeState(Idle)
```

### 2. **Detach During End-Condition Pending**
```
User performed Move → endConditionPending = true
  ↓
Try to detach → BLOCKED by TurnManager
  ↓
IsPieceAllowedToAct() returns false
```

### 3. **Multiple Detach in Same Turn**
```
Detach Infantry from Tank
  ↓
Tank auto-selected
  ↓
TurnManager.detachActive = true
  ↓
User can:
  - Detach another piece from Tank
  - Move Tank (trigger end-condition)
```

---

## Testing Checklist

### Basic Detach Flow:
- [ ] Click carried piece in panel → highlight valid positions
- [ ] Click valid position → execute detach
- [ ] After detach → carrier auto-selected
- [ ] Detached piece appears on board at correct position

### Deselection:
- [ ] Click same carried piece twice → go back to carrier
- [ ] Click carrier while in detach mode → cancel detach

### Edge Cases:
- [ ] No valid positions → error message / back to idle
- [ ] Nested carrying (A→B→C) → detach C shows positions around A
- [ ] Multiple detach in turn → allowed until move/capture/boarding

### Integration with Turn System:
- [ ] Detach does NOT trigger end-condition
- [ ] After detach, can move carrier → trigger end-condition
- [ ] Cancel turn after detach → restore carried pieces

---

## Known Issues / Future Improvements

### 1. **Unity Compilation**
- `SelectingDetachTargetState` might show compilation error initially
- **Fix:** Restart Unity Editor hoặc force recompile (Assets → Reimport All)

### 2. **Nested Carrying UX**
- Detaching deeply nested pieces có thể confusing
- **Improvement:** Visual indicator showing carrier hierarchy

### 3. **Position Calculation**
- Hiện tại chỉ check 8 directions adjacent
- **Improvement:** Có thể dùng `passengerToDetach.PossibleMoves` thay vì adjacent cells

### 4. **Animation**
- Detach hiện tại instant, không có animation
- **Future:** Add tween animation khi piece detach

### 5. **Sound Effects**
- Không có SFX cho detach action
- **Future:** Add sound khi enter detach mode và execute

---

## API Summary

### GameStateManager
```csharp
void SelectPieceForDetach(BasePiece piece)
void SelectPiece(BasePiece piece)
```

### GameHUDController
```csharp
void OnInfoPanelItemClicked(BasePiece piece)
  // Handles carried vs non-carried piece selection
```

### SelectingDetachTargetState
```csharp
void Enter()
  // Calculate and highlight valid detach positions
  
void HandleBoardClick(BoardCoord coord)
  // Execute detach command
  
void HandlePieceClick(BasePiece piece)
  // Handle carrier/passenger clicks
```

---

## Kết Luận

Hệ thống detach từ info panel đã hoàn thiện với:
- ✅ Click carried piece → enter detach mode
- ✅ Highlight valid positions (adjacent to carrier)
- ✅ Execute detach command
- ✅ Auto-select carrier after detach
- ✅ Deselect bằng click lại piece
- ✅ Integration với turn management system

**Ready for testing in Unity Editor!** 🚀
