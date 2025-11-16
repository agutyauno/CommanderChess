# Hero System & Win Condition Implementation

## Overview
Implementation of hero promotion system and win condition checking for CommanderChess. System checks win conditions after each command execution, then hero conditions.

## Architecture

### Services Created

1. **GameStatsTracker** - Tracks unit losses per team
   - Location: `Assets/Scripts/CommanderChess/Services/GameStatsTracker.cs`
   - Subscribes to: `PieceCapturedEvent`, `PieceDestroyedEvent`
   - Tracks: Navy, Airforce, Infantry, Tank, Artillery, Commander losses

2. **WinConditionChecker** - Checks all win conditions with priority
   - Location: `Assets/Scripts/CommanderChess/Services/WinConditionChecker.cs`
   - Priority: Commander Death > Navy Loss = Airforce Loss = Ground Units Loss
   - Handles: `Surrender()` method for manual surrender
   - Publishes: `GameWonEvent` when condition met

3. **HeroConditionChecker** - Checks hero promotion conditions
   - Location: `Assets/Scripts/CommanderChess/Services/HeroConditionChecker.cs`
   - Conditions:
     1. Can attack commander without danger zones (commander in `PossibleAttacks` AND `PathChecker` returns `PathResult.None`)
     2. Last piece besides Commander and HQ
   - Publishes: `HeroBecameEvent` when piece promoted

### Events Created

1. **GameWonEvent** - Fired when game ends
   - Properties: `Winner`, `Loser`, `WinCondition`
   - WinCondition enum: `CommanderKilled`, `AllNavyLost`, `AllAirforceLost`, `AllGroundUnitsLost`, `Surrender`

2. **HeroBecameEvent** - Fired when piece becomes hero
   - Properties: `Piece`, `HeroCondition`
   - HeroCondition enum: `CanAttackCommander`, `LastPiece`

3. **PieceDestroyedEvent** - New event for pieces destroyed (e.g., carrier in ROF zone)
   - Properties: `DestroyedPiece`, `Position`

## Integration with TurnManager

Modified `TurnManager.RecordCommand()` to check conditions after each command:

```csharp
public void RecordCommand(ICommand command)
{
    // 1. Record command
    currentTurnSnapshot.Commands.Add(command);
    
    // 2. Check win conditions FIRST (highest priority)
    bool gameWon = winConditionChecker.CheckWinConditions(currentTurn);
    if (gameWon) return; // Game ended
    
    // 3. Check hero conditions
    heroConditionChecker.CheckAllHeroConditions();
    
    // 4. Evaluate end-turn conditions (existing logic)
    // ...
}
```

Added `Surrender()` method to TurnManager for manual surrender.

## Hero Conditions Details

### Condition 1: Can Attack Commander Safely
- Commander must be in piece's `PossibleAttacks`
- Path to commander must return `PathResult.None` (no danger zones)
- Checked after each move
- Example: Infantry can reach enemy commander without crossing danger zones

### Condition 2: Last Piece
- Team has only Commander + HQ + 1 other piece → that piece becomes hero
- Example: Red team has Commander + 2 HQ + 1 Tank → Tank becomes hero
- Only pieces except Commander and HQ can become heroes

## Win Conditions Details

### Priority 1: Commander Death (Instant Win)
Checked first, ends game immediately if:
- Commander captured
- Commander destroyed (e.g., Airforce carrying Commander destroyed in ROF zone)

### Priority 2: Unit Losses (Equal Priority)
Game ends if team loses:
- **All Navy** - No navy pieces remaining on board
- **All Airforce** - No airforce pieces remaining on board
- **All Ground Units** - No Infantry AND no Tank AND no Artillery remaining

### Multiple Conditions Example
If Red team simultaneously:
- Loses all navy
- Blue commander dies

Result: Red wins (Commander Death has higher priority)

## VContainer Registration

Added to `GameLifeTimeScope.cs`:
```csharp
builder.Register<GameStatsTracker>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
builder.Register<WinConditionChecker>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
builder.Register<HeroConditionChecker>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
```

## Data Structures

### UnitLosses Class
```csharp
public class UnitLosses
{
    public int NavyLost { get; set; }
    public int AirforceLost { get; set; }
    public int InfantryLost { get; set; }
    public int TankLost { get; set; }
    public int ArtilleryLost { get; set; }
    public bool CommanderLost { get; set; }
}
```

## Existing Piece Property

`BasePiece` already has `IsHero` property:
```csharp
public bool IsHero { get; set; } = false;
```

## Future Implementation

### GameOver State
- Display UI showing winner and win condition
- Show buttons: Menu, Rematch, etc.
- Block all input except navigation
- Will be implemented later

### Surrender Button
- Add surrender button to HUD
- Call `turnManager.Surrender()` when clicked
- In online mode, surrender requires manual confirmation

## Testing Scenarios

### Win Condition Tests
1. Capture enemy commander → Immediate win
2. Destroy all navy pieces → Check win
3. Destroy all airforce pieces → Check win
4. Destroy all Infantry + Tank + Artillery → Check win
5. Press surrender button → Opponent wins

### Hero Condition Tests
1. Move piece to position where it can attack commander without danger zones → Becomes hero
2. Lose all pieces except Commander + HQ + 1 piece → That piece becomes hero
3. Hero piece ignores danger zones in `PathChecker`

## Notes

- Win condition check happens BEFORE hero check (priority)
- Hero status persists for the entire game
- Heroes are immune to danger zones (checked in `PathChecker.CheckPath()`)
- All checks happen synchronously after each command execution
- System is extensible for online play and statistics tracking
