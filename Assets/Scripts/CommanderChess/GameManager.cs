using UnityEngine;
using VContainer;
using CommanderChess.Domain;
using CommanderChess.Services;
using CommanderChess.GameState;

/// <summary>
/// GameManager - Entry point và initialization
/// REFACTORED: Sử dụng EventBus thay vì trực tiếp subscribe events
/// </summary>
public class GameManager : MonoBehaviour
{
    [Inject] readonly Board board;
    [Inject] readonly TurnManager turnManager;
    [Inject] readonly GameStateManager gameStateManager;
    [Inject] readonly PieceSpawner pieceSpawner;
    [Inject] readonly EventBus eventBus;

    void Start()
    {
        InitializeGame();
    }

    void InitializeGame()
    {
//         Debug.Log("=== Initializing Commander Chess ===");

        // 1. Initialize board
        board.Init();
//         Debug.Log("Board initialized");

        // 2. Setup pieces
        SetupPieces();
//         Debug.Log("Pieces setup completed");

        // 3. Reset turn to Red
        turnManager.ResetTurn();
//         Debug.Log("Turn manager initialized");

        // 4. Subscribe to events VIA EVENTBUS
        SubscribeToEvents();
//         Debug.Log("Event subscriptions completed");

        // 5. ✅ Publish game initialized event
        eventBus.Publish(new GameInitializedEvent());

//         Debug.Log("=== Game Ready ===");
    }

    void SetupPieces()
    {
        pieceSpawner.Spawn();
    }

    #region Event Subscription via EventBus

    void SubscribeToEvents()
    {
        // ✅ Subscribe thông qua EventBus thay vì trực tiếp
        eventBus.Subscribe<TurnChangedEvent>(OnTurnChanged);
        eventBus.Subscribe<TurnStartedEvent>(OnTurnStarted);
        eventBus.Subscribe<PieceSelectedEvent>(OnPieceSelected);
        eventBus.Subscribe<PieceDeselectedEvent>(OnPieceDeselected);
        
        // Subscribe to movement events for feedback
        eventBus.Subscribe<PieceMovedEvent>(OnPieceMoved);
        eventBus.Subscribe<PieceCapturedEvent>(OnPieceCaptured);
    }

    void OnDestroy()
    {
        // ✅ Unsubscribe tất cả
        UnsubscribeFromEvents();
    }

    void UnsubscribeFromEvents()
    {
        if (eventBus == null) return;

        eventBus.Unsubscribe<TurnChangedEvent>(OnTurnChanged);
        eventBus.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
        eventBus.Unsubscribe<PieceSelectedEvent>(OnPieceSelected);
        eventBus.Unsubscribe<PieceDeselectedEvent>(OnPieceDeselected);
        eventBus.Unsubscribe<PieceMovedEvent>(OnPieceMoved);
        eventBus.Unsubscribe<PieceCapturedEvent>(OnPieceCaptured);
    }

    #endregion

    #region Event Handlers

    void OnTurnChanged(TurnChangedEvent evt)
    {
//         Debug.Log($">>> Turn changed: {evt.PreviousTurn} -> {evt.NewTurn} (Turn {evt.TurnNumber})");
        // TODO: Update UI to show current turn
        // TODO: Play turn change sound/animation
    }

    void OnTurnStarted(TurnStartedEvent evt)
    {
//         Debug.Log($">>> {evt.Team}'s turn started (Turn {evt.TurnNumber})");
        // TODO: Play turn start animation/sound
        // TODO: Show turn indicator
    }

    void OnPieceSelected(PieceSelectedEvent evt)
    {
//         Debug.Log($">>> Selected: {evt.Piece.Team} {evt.Piece.Type} at {evt.Piece.Position.ToLabel()}");
        // TODO: Show piece info panel
        // TODO: Play selection sound
        // TODO: Highlight piece sprite
    }

    void OnPieceDeselected(PieceDeselectedEvent evt)
    {
//         Debug.Log($">>> Piece deselected");
        // TODO: Hide piece info panel
        // TODO: Clear highlights
    }

    void OnPieceMoved(PieceMovedEvent evt)
    {
//         Debug.Log($">>> Piece moved: {evt.Piece.Type} from {evt.From.ToLabel()} to {evt.To.ToLabel()}");
        // TODO: Play movement sound
        // TODO: Update minimap
    }

    void OnPieceCaptured(PieceCapturedEvent evt)
    {
//         Debug.Log($">>> Piece captured: {evt.Attacker.Type} captured {evt.Defender.Type}");
        // TODO: Play capture sound/animation
        // TODO: Update captured pieces display
        // TODO: Show score update
    }

    #endregion

    #region Public API (for UI buttons)

    public void OnUndoButtonClicked()
    {
        gameStateManager.UndoLastMove();
    }

    public void OnEndTurnButtonClicked()
    {
        if (gameStateManager.CurrentState == GameState.Idle)
        {
            turnManager.EndTurn();
        }
        else
        {
            Debug.LogWarning("Cannot end turn - action in progress");
        }
    }

    public void OnRestartGameButtonClicked()
    {
//         Debug.Log("Restarting game...");
        // Reload scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    #endregion

    #region Game End Detection (Future Feature)

    // Để lại cấu trúc cho việc detect game end
    
    /*
    void CheckWinCondition()
    {
        ///Check if commander is captured
        ///Check if no valid moves remain
        etc.
        
        Team? winner = DetectWinner();
        
        if (winner.HasValue)
        {
            eventBus.Publish(new GameEndedEvent(
                winner: winner.Value,
                reason: "Commander captured"
            ));
        }
    }
    
    Team? DetectWinner()
    {
        // Implementation
        return null;
    }
    */

    #endregion
}
