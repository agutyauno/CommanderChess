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
        SaveCurrentTurnState();
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
        }
    }

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