using UnityEngine;
using VContainer;
using CommanderChess.Domain;
using CommanderChess.Services;
using CommanderChess.GameState;

/// <summary>
/// GameManager - Entry point và initialization
/// </summary>
public class GameManager : MonoBehaviour
{
    [Inject] readonly Board board;
    [Inject] readonly TurnManager turnManager;
    [Inject] readonly GameStateManager gameStateManager;
    [Inject] readonly PieceSpawner pieceSpawner;

    void Start()
    {
        InitializeGame();
    }

    void InitializeGame()
    {
        Debug.Log("=== Initializing Commander Chess ===");

        // 1. Initialize board
        board.Init();
        Debug.Log("Board initialized");

        // 2. Setup pieces (TODO: Load from configuration)
        SetupPieces();
        Debug.Log("Pieces setup completed");

        // 3. Reset turn to Red
        turnManager.ResetTurn();
        Debug.Log("Turn manager initialized");

        // 4. Subscribe to events
        SubscribeToEvents();
        Debug.Log("Event subscriptions completed");

        Debug.Log("=== Game Ready ===");
    }

    void SetupPieces()
    {
        pieceSpawner.Spawn();        
    }

    void SubscribeToEvents()
    {
        // Subscribe to turn changes
        turnManager.OnTurnChanged += OnTurnChanged;
        turnManager.OnTurnStarted += OnTurnStarted;

        // Subscribe to state changes
        gameStateManager.OnStateChanged += OnStateChanged;
        gameStateManager.OnPieceSelected += OnPieceSelected;
        gameStateManager.OnPieceDeselected += OnPieceDeselected;
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (turnManager != null)
        {
            turnManager.OnTurnChanged -= OnTurnChanged;
            turnManager.OnTurnStarted -= OnTurnStarted;
        }

        if (gameStateManager != null)
        {
            gameStateManager.OnStateChanged -= OnStateChanged;
            gameStateManager.OnPieceSelected -= OnPieceSelected;
            gameStateManager.OnPieceDeselected -= OnPieceDeselected;
        }
    }

    #region Event Handlers

    void OnTurnChanged(Team newTurn)
    {
        Debug.Log($">>> Turn changed to: {newTurn}");
        // TODO: Update UI to show current turn
    }

    void OnTurnStarted(Team team)
    {
        Debug.Log($">>> {team}'s turn started");
        // TODO: Play turn start animation/sound
    }

    void OnStateChanged(GameState oldState, GameState newState)
    {
        Debug.Log($">>> State: {oldState} -> {newState}");
        // TODO: Update UI to show current state
    }

    void OnPieceSelected(BasePiece piece)
    {
        Debug.Log($">>> Selected: {piece.Team} {piece.Type} at {piece.Position.ToLabel()}");
        // TODO: Show piece info panel
    }

    void OnPieceDeselected()
    {
        Debug.Log($">>> Piece deselected");
        // TODO: Hide piece info panel
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
        Debug.Log("Restarting game...");
        // TODO: Implement game restart logic
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    #endregion
}