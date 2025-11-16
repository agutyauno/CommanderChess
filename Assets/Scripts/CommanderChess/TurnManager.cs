using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using CommanderChess.Domain;
using CommanderChess.Services;
using CommanderChess.CommandSystem;

/// <summary>
/// TurnManager - Quản lý lượt chơi và turn-level backup/undo trong game
/// REFACTORED: 
/// - Sử dụng EventBus thay vì C# events
/// - Quản lý turn-level snapshots cho undo và replay
/// - Hỗ trợ online sync và replay system
/// </summary>
public class TurnManager : BaseService
{
    #region Dependencies
    [Inject] readonly StateBackupService backupService;
    [Inject] readonly Board board;
    #endregion

    #region Turn State
    Team currentTurn = Team.Red;
    int turnNumber = 1;
    #endregion

    #region Turn History & Snapshots
    // Turn snapshot: lưu state đầu mỗi turn
    public class TurnSnapshot
    {
        public int TurnNumber;
        public Team Team;
        public StateBackupService.Snapshot GameState;
        public List<ICommand> Commands; // Commands executed trong turn này
        public DateTime Timestamp;
        
        public TurnSnapshot(int turnNumber, Team team, StateBackupService.Snapshot gameState)
        {
            TurnNumber = turnNumber;
            Team = team;
            GameState = gameState;
            Commands = new List<ICommand>();
            Timestamp = DateTime.Now;
        }
    }

    // History của các turns (cho undo và replay)
    private readonly Stack<TurnSnapshot> turnHistory = new Stack<TurnSnapshot>();
    private TurnSnapshot currentTurnSnapshot;
    
    // Replay data (lưu toàn bộ game từ đầu đến cuối)
    private readonly List<TurnSnapshot> replayHistory = new List<TurnSnapshot>();
    
    private const int maxTurnHistory = 20; // Giới hạn số turn có thể undo
    // End-turn evaluation state
    bool endConditionPending = false;

    // Detach-specific state: when a detach occurs, only the carrier may continue actions
    bool detachActive = false;
    BasePiece allowedPieceAfterDetach = null;
    #endregion

    #region Properties
    public Team CurrentTurn => currentTurn;
    public int TurnNumber => turnNumber;
    public bool CanUndo => turnHistory.Count > 0;
    public int TurnHistoryCount => turnHistory.Count;
    #endregion

    #region Public API

    /// <summary>
    /// Override Initialize để tạo snapshot đầu tiên
    /// Gọi sau khi board setup xong
    /// </summary>
    protected override void OnInitialize()
    {
        base.OnInitialize();
        Debug.Log("[TurnManager] Initializing with turn-level backup system");
        
        // Subscribe to movement events
        eventBus.Subscribe<PieceMovedEvent>(OnPieceMoved);
        eventBus.Subscribe<PieceCapturedEvent>(OnPieceCaptured);
        eventBus.Subscribe<PieceBoardedEvent>(OnPieceBoarded);
        eventBus.Subscribe<PieceDetachedEvent>(OnPieceDetached);
        
        SaveCurrentTurnState();
    }

    protected override void OnDispose()
    {
        base.OnDispose();
        
        // Unsubscribe from events
        eventBus.Unsubscribe<PieceMovedEvent>(OnPieceMoved);
        eventBus.Unsubscribe<PieceCapturedEvent>(OnPieceCaptured);
        eventBus.Unsubscribe<PieceBoardedEvent>(OnPieceBoarded);
        eventBus.Unsubscribe<PieceDetachedEvent>(OnPieceDetached);
    }

    /// <summary>
    /// Kiểm tra xem piece có thuộc về người chơi hiện tại không
    /// </summary>
    public bool IsCurrentPlayerPiece(BasePiece piece)
    {
        if (piece == null) return false;
        return piece.Team == currentTurn;
    }

    /// <summary>
    /// Ghi nhận một command được thực thi trong turn hiện tại
    /// Được gọi bởi CommandManager sau khi execute thành công
    /// </summary>
    public void RecordCommand(ICommand command)
    {
        if (currentTurnSnapshot != null)
        {
            currentTurnSnapshot.Commands.Add(command);
            Debug.Log($"[TurnManager] Recorded command: {command.Description} (Total: {currentTurnSnapshot.Commands.Count})");
            
            // Evaluate end-turn conditions based on command type
            if (!endConditionPending)
            {
                var typeName = command.GetType().Name;

                // These commands trigger end-turn condition
                if (typeName.Contains("MoveCommand") || typeName.Contains("CaptureCommand") || typeName.Contains("BoardingCommand"))
                {
                    endConditionPending = true;

                    // Publish event so HUD can show confirm/cancel
                    eventBus.Publish(new TurnEndConditionReachedEvent(
                        team: currentTurn,
                        turnNumber: turnNumber,
                        commandDescription: command.Description,
                        timestamp: command.Timestamp
                    ));
                }
                else if (typeName.Contains("DetachCommand"))
                {
                    // Detach does not end turn — the detach event handler will set allowed piece
                    detachActive = true;
                    // HUD shouldn't show confirm yet; allowed piece will be published by PieceDetachedEvent handler
                }
            }
        }
    }

    #endregion

    #region Event Handlers - Movement Events

    private void OnPieceMoved(PieceMovedEvent evt)
    {
        // Movement already recorded via RecordCommand, just ensure logic consistency
        Debug.Log($"[TurnManager] OnPieceMoved: {evt.Piece.Type} {evt.From.ToLabel()}→{evt.To.ToLabel()}");
    }

    private void OnPieceCaptured(PieceCapturedEvent evt)
    {
        Debug.Log($"[TurnManager] OnPieceCaptured: {evt.Attacker.Type} captured {evt.Defender.Type}");
    }

    private void OnPieceBoarded(PieceBoardedEvent evt)
    {
        Debug.Log($"[TurnManager] OnPieceBoarded: {evt.Passenger.Type} boarded {evt.Carrier.Type}");
    }

    private void OnPieceDetached(PieceDetachedEvent evt)
    {
        // After a detach, only the carrier at carrierPos is allowed to continue actions
        Debug.Log($"[TurnManager] OnPieceDetached: {evt.Passenger.Type} detached");
        
        if (board.Pieces.ContainsKey(evt.CarrierPosition))
        {
            allowedPieceAfterDetach = board.Pieces[evt.CarrierPosition];
            detachActive = true;
            eventBus.Publish(new TurnDetachOccurredEvent(allowedPieceAfterDetach, currentTurn, turnNumber));
        }
    }

    #endregion

    #region Turn Confirmation/Cancellation

    /// <summary>
    /// Called by HUD when player confirms end-turn (Confirm button)
    /// </summary>
    public void ConfirmEndTurn()
    {
        if (!endConditionPending)
        {
            Debug.LogWarning("[TurnManager] ConfirmEndTurn called but no end condition pending");
            return;
        }

        Debug.Log($"[TurnManager] Turn {turnNumber} confirmed by {currentTurn}");

        // Publish TurnEnded event BEFORE EndTurn
        eventBus.Publish(new TurnEndedEvent(currentTurn, turnNumber));

        // Reset flags
        endConditionPending = false;
        detachActive = false;
        allowedPieceAfterDetach = null;

        // Finalize end turn (switches to next team)
        EndTurn();
    }

    /// <summary>
    /// Called by HUD when player cancels the end-turn confirmation (Cancel button).
    /// This will revert the current turn back to the snapshot saved at turn start.
    /// </summary>
    public void CancelEndTurn()
    {
        if (!endConditionPending && !detachActive)
        {
            Debug.LogWarning("[TurnManager] CancelEndTurn called but nothing to cancel");
            return;
        }

        if (currentTurnSnapshot == null)
        {
            Debug.LogError("[TurnManager] No current turn snapshot available to cancel");
            return;
        }

        Debug.Log($"[TurnManager] Cancelling turn {turnNumber} for {currentTurn}");

        // Restore to snapshot at start of current turn
        backupService.RestoreSnapshot(currentTurnSnapshot.GameState);
        bool valid = backupService.ValidateSnapshotRestored(currentTurnSnapshot.GameState);
        if (!valid)
        {
            Debug.LogError("[TurnManager] Failed to restore snapshot on CancelEndTurn");
            return;
        }

        // Clear recorded commands for this turn
        currentTurnSnapshot.Commands.Clear();

        // Reset flags
        endConditionPending = false;
        detachActive = false;
        allowedPieceAfterDetach = null;

        // Notify listeners
        eventBus.Publish(new TurnEndCancelledEvent(currentTurn, turnNumber));
        
        Debug.Log($"[TurnManager] Turn cancelled successfully");
    }

    /// <summary>
    /// Query helper for other systems (e.g., UI / GameState) to check if a piece is allowed to act now.
    /// </summary>
    public bool IsPieceAllowedToAct(BasePiece piece)
    {
        if (piece == null) return false;
        
        // If detach is active, only the carrier can act
        if (detachActive)
        {
            return piece == allowedPieceAfterDetach;
        }
        
        // If end condition is pending, no piece can act
        if (endConditionPending)
        {
            return false;
        }
        
        // Normal case: piece must be current team
        return piece.Team == currentTurn;
    }

    /// <summary>
    /// Check if turn end condition has been reached (for UI to show confirm/cancel)
    /// </summary>
    public bool IsEndConditionPending => endConditionPending;

    /// <summary>
    /// Check if detach is active and waiting for carrier action
    /// </summary>
    public bool IsDetachActive => detachActive;

    /// <summary>
    /// Kết thúc lượt hiện tại và chuyển sang lượt tiếp theo
    /// Tự động save snapshot cho turn mới
    /// </summary>
    public void EndTurn()
    {
        var previousTurn = currentTurn;

        // Lưu current snapshot vào history
        if (currentTurnSnapshot != null)
        {
            turnHistory.Push(currentTurnSnapshot);
            replayHistory.Add(currentTurnSnapshot);
            
            // Giới hạn history size
            if (turnHistory.Count > maxTurnHistory)
            {
                var temp = new Stack<TurnSnapshot>();
                int count = 0;
                foreach (var snapshot in turnHistory)
                {
                    if (count < maxTurnHistory)
                        temp.Push(snapshot);
                    count++;
                }
                turnHistory.Clear();
                foreach (var snapshot in temp)
                    turnHistory.Push(snapshot);
            }
        }

        // Chuyển lượt
        currentTurn = currentTurn == Team.Red ? Team.Blue : Team.Red;
        turnNumber++;

        // Reset turn state flags for new turn
        endConditionPending = false;
        detachActive = false;
        allowedPieceAfterDetach = null;

        Debug.Log($"Turn {turnNumber}: {currentTurn}'s turn");

        eventBus.Publish(new TurnChangedEvent(
            newTurn: currentTurn,
            previousTurn: previousTurn,
            turnNumber: turnNumber
        ));
        
        eventBus.Publish(new TurnStartedEvent(
            team: currentTurn,
            turnNumber: turnNumber
        ));

        // Tạo snapshot cho turn mới
        SaveCurrentTurnState();
    }

    /// <summary>
    /// Undo turn hiện tại - restore về đầu turn
    /// Restore toàn bộ: positions, visuals, board dictionary, carrying relationships
    /// </summary>
    public bool UndoTurn()
    {
        if (!CanUndo)
        {
            Debug.LogWarning("[TurnManager] Cannot undo - no turns in history");
            return false;
        }

        try
        {
            // Pop snapshot trước đó
            var previousSnapshot = turnHistory.Pop();
            
            Debug.Log($"[TurnManager] ========== UNDO TURN ==========");
            Debug.Log($"[TurnManager] Undoing turn {turnNumber} -> Restoring turn {previousSnapshot.TurnNumber}");
            Debug.Log($"  - Restoring {previousSnapshot.GameState.PieceData.Count} pieces");
            Debug.Log($"  - Executed {previousSnapshot.Commands.Count} commands will be reverted");

            // Restore game state
            backupService.RestoreSnapshot(previousSnapshot.GameState);

            // Validate restore
            bool isValid = backupService.ValidateSnapshotRestored(previousSnapshot.GameState);
            if (!isValid)
            {
                Debug.LogError("[TurnManager] Snapshot validation FAILED after restore!");
                // Push back snapshot for retry
                turnHistory.Push(previousSnapshot);
                return false;
            }

            // Restore turn info
            currentTurn = previousSnapshot.Team;
            turnNumber = previousSnapshot.TurnNumber;

            // Reset turn state flags
            endConditionPending = false;
            detachActive = false;
            allowedPieceAfterDetach = null;

            Debug.Log($"[TurnManager] Turn undo successful - Now at turn {turnNumber}, {currentTurn}'s turn");
            Debug.Log($"[TurnManager] Board has {board.Pieces.Count} pieces");

            // Publish events
            eventBus.Publish(new TurnUndoneEvent(
                restoredTurn: turnNumber,
                restoredTeam: currentTurn
            ));

            eventBus.Publish(new TurnStartedEvent(
                team: currentTurn,
                turnNumber: turnNumber
            ));

            // Recreate current turn snapshot
            SaveCurrentTurnState();

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[TurnManager] Undo turn failed: {e.Message}\n{e.StackTrace}");
            return false;
        }
    }

    /// <summary>
    /// Save state hiện tại làm snapshot cho turn
    /// </summary>
    private void SaveCurrentTurnState()
    {
        var gameState = backupService.CreateFullBoardSnapshot();
        
        if (gameState == null)
        {
            Debug.LogError("[TurnManager] Failed to create turn snapshot!");
            return;
        }

        currentTurnSnapshot = new TurnSnapshot(turnNumber, currentTurn, gameState);
        
        Debug.Log($"[TurnManager] Turn snapshot saved: Turn {turnNumber}, {currentTurn}, {gameState.PieceData.Count} pieces");
    }

    /// <summary>
    /// Reset về lượt đầu tiên (dùng khi restart game)
    /// </summary>
    public void ResetTurn()
    {
        currentTurn = Team.Red;
        turnNumber = 1;
        
        // Clear history
        turnHistory.Clear();
        replayHistory.Clear();
        currentTurnSnapshot = null;

        // Reset turn state flags
        endConditionPending = false;
        detachActive = false;
        allowedPieceAfterDetach = null;

        Debug.Log($"Turn reset: {currentTurn}'s turn");
        
        eventBus.Publish(new TurnStartedEvent(
            team: currentTurn,
            turnNumber: turnNumber
        ));

        // Tạo snapshot mới
        SaveCurrentTurnState();
    }

    /// <summary>
    /// Đặt lượt cụ thể (dùng khi load game hoặc sync online)
    /// </summary>
    public void SetTurn(Team team, int turn)
    {
        var previousTurn = currentTurn;
        currentTurn = team;
        turnNumber = turn;

        Debug.Log($"Turn set to {turnNumber}: {currentTurn}'s turn");
        
        eventBus.Publish(new TurnChangedEvent(
            newTurn: currentTurn,
            previousTurn: previousTurn,
            turnNumber: turnNumber
        ));
        
        // Recreate snapshot
        SaveCurrentTurnState();
    }

    #endregion

    #region Replay & Online Sync Support

    /// <summary>
    /// Lấy toàn bộ replay history từ đầu game
    /// </summary>
    public List<TurnSnapshot> GetReplayHistory()
    {
        return new List<TurnSnapshot>(replayHistory);
    }

    /// <summary>
    /// Serialize turn data để gửi qua network (online sync)
    /// </summary>
    [Serializable]
    public class TurnData
    {
        public int TurnNumber;
        public Team Team;
        public List<CommandData> Commands;
        public DateTime Timestamp;

        [Serializable]
        public class CommandData
        {
            public string CommandType;
            public BoardCoord From;
            public BoardCoord To;
            public string Description;
            public DateTime Timestamp;
        }
    }

    /// <summary>
    /// Export current turn data for online sync
    /// </summary>
    public TurnData ExportCurrentTurnData()
    {
        if (currentTurnSnapshot == null)
            return null;

        var turnData = new TurnData
        {
            TurnNumber = currentTurnSnapshot.TurnNumber,
            Team = currentTurnSnapshot.Team,
            Timestamp = currentTurnSnapshot.Timestamp,
            Commands = new List<TurnData.CommandData>()
        };

        foreach (var cmd in currentTurnSnapshot.Commands)
        {
            turnData.Commands.Add(new TurnData.CommandData
            {
                CommandType = cmd.GetType().Name,
                Description = cmd.Description,
                Timestamp = cmd.Timestamp
                // Note: From/To coordinates cần được expose từ ICommand interface nếu cần
            });
        }

        return turnData;
    }

    /// <summary>
    /// Get turn history count for UI display
    /// </summary>
    public int GetExecutedCommandsCount()
    {
        return currentTurnSnapshot?.Commands.Count ?? 0;
    }

    /// <summary>
    /// Clear all history (dùng khi start new game)
    /// </summary>
    public void ClearHistory()
    {
        turnHistory.Clear();
        replayHistory.Clear();
        Debug.Log("[TurnManager] History cleared");
    }

    #endregion

    #region Optional: Timer Support (Future Feature)
    
    // Để lại cấu trúc cho tương lai nếu muốn thêm timer
    
    /*
    float currentTurnTime = 0f;
    float maxTurnTime = 60f; // 60 seconds per turn
    bool isTimerEnabled = false;
    
    public void EnableTimer(float maxSeconds)
    {
        isTimerEnabled = true;
        maxTurnTime = maxSeconds;
        currentTurnTime = maxSeconds;
    }
    
    public void DisableTimer()
    {
        isTimerEnabled = false;
    }
    
    void Update()
    {
        if (!isTimerEnabled) return;
        
        currentTurnTime -= Time.deltaTime;
        
        // Publish timer update event
        eventBus.Publish(new TurnTimeUpdatedEvent(
            team: currentTurn,
            remainingSeconds: currentTurnTime
        ));
        
        // Auto end turn when time runs out
        if (currentTurnTime <= 0f)
        {
            Debug.LogWarning($"{currentTurn} ran out of time!");
            EndTurn();
        }
    }
    */
    
    #endregion
}