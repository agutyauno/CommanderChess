# Commander Chess

A Unity-based chess game with a unique Commander piece and special abilities.

## Overview

Commander Chess is a variant of traditional chess that introduces the **Commander** piece - a powerful unit with special abilities that can turn the tide of battle.

## Game Modes

### Standard Chess Mode
- Traditional chess rules
- All standard pieces: King, Queen, Rooks, Bishops, Knights, and Pawns

### Commander Mode
- Each player has a **Commander** that replaces one Knight
- Commanders have unique movement and special abilities

## Pieces

### Standard Pieces
- **King**: Moves one square in any direction. Must be protected at all costs.
- **Queen**: Moves any number of squares horizontally, vertically, or diagonally.
- **Rook**: Moves any number of squares horizontally or vertically.
- **Bishop**: Moves any number of squares diagonally.
- **Knight**: Moves in an "L" shape (2+1 squares).
- **Pawn**: Moves forward, captures diagonally. Can move two squares on first move.

### Commander (Unique to Commander Mode)
- **Movement**: Can move like a Knight OR one square in any direction (like a King)
- **Special Abilities**:
  - **Rally**: Boosts nearby allied pieces for the current turn
  - **Charge**: Move up to 3 extra squares in a straight line
  - **Shield**: Protect adjacent pieces from capture for one turn
  - **Tactics**: Swap positions with a friendly piece within 2 squares
  - **Inspire**: Give an adjacent friendly piece an extra move this turn

## Project Structure

```
Assets/
├── Scripts/
│   ├── Core/           # Core game enums and data structures
│   │   ├── GameEnums.cs
│   │   ├── BoardPosition.cs
│   │   └── Move.cs
│   ├── Pieces/         # Chess piece implementations
│   │   ├── ChessPiece.cs (base class)
│   │   ├── Pawn.cs
│   │   ├── Rook.cs
│   │   ├── Knight.cs
│   │   ├── Bishop.cs
│   │   ├── Queen.cs
│   │   ├── King.cs
│   │   └── Commander.cs
│   ├── Board/          # Board management
│   │   └── ChessBoard.cs
│   ├── Commands/       # Commander abilities
│   │   └── CommanderAbilityHandler.cs
│   ├── Managers/       # Game management
│   │   ├── GameManager.cs
│   │   └── MoveValidator.cs
│   ├── UI/             # User interface
│   │   ├── GameUIController.cs
│   │   ├── BoardRenderer.cs
│   │   ├── InputHandler.cs
│   │   └── CommanderAbilityUI.cs
│   └── Utils/          # Utility classes
│       ├── FENParser.cs
│       └── MoveNotation.cs
└── Scenes/
    └── SampleScene.unity
```

## Controls

### Mouse
- **Left Click**: Select a piece or move to a square
- **Right Click**: Deselect current piece

### Keyboard (Commander Mode)
- **1**: Use Rally ability
- **2**: Use Charge ability
- **3**: Use Shield ability
- **4**: Use Tactics ability
- **5**: Use Inspire ability
- **Escape**: Pause/Resume game

## Technical Details

- **Unity Version**: 6000.2.2f1
- **Render Pipeline**: Universal Render Pipeline (URP)
- **Input System**: New Input System

## Getting Started

1. Open the project in Unity 6000.2.2f1 or later
2. Open `Assets/Scenes/SampleScene.unity`
3. Add the `GameManager` component to a GameObject in the scene
4. Set up the UI and board renderer components
5. Press Play to start the game

## Events

The GameManager provides several events for UI integration:
- `OnGameStateChanged`: Fired when game state changes
- `OnTurnChanged`: Fired when turn changes
- `OnPieceSelected`: Fired when a piece is selected
- `OnPieceDeselected`: Fired when selection is cleared
- `OnMoveMade`: Fired after a move is executed
- `OnPieceCaptured`: Fired when a piece is captured
- `OnAbilityUsed`: Fired when a Commander ability is used

## License

This project is provided for educational purposes.

