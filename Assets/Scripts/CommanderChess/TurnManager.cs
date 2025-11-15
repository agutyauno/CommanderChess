using UnityEngine;
using VContainer;
using CommanderChess.Domain;
using CommanderChess.Services;

/// <summary>
/// TurnManager - Quản lý lượt chơi trong game
/// REFACTORED: Sử dụng EventBus thay vì C# events
/// </summary>
public class TurnManager : BaseService
{
    Team currentTurn = Team.Red;
    int turnNumber = 1;

    #region Properties
    public Team CurrentTurn => currentTurn;
    public int TurnNumber => turnNumber;
    #endregion

    #region Public API

    /// <summary>
    /// Kiểm tra xem piece có thuộc về người chơi hiện tại không
    /// </summary>
    public bool IsCurrentPlayerPiece(BasePiece piece)
    {
        if (piece == null) return false;
        return piece.Team == currentTurn;
    }

    /// <summary>
    /// Kết thúc lượt hiện tại và chuyển sang lượt tiếp theo
    /// </summary>
    public void EndTurn()
    {
        var previousTurn = currentTurn;

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
    }

    /// <summary>
    /// Reset về lượt đầu tiên (dùng khi restart game)
    /// </summary>
    public void ResetTurn()
    {
        currentTurn = Team.Red;
        turnNumber = 1;

        Debug.Log($"Turn reset: {currentTurn}'s turn");
        
        eventBus.Publish(new TurnStartedEvent(
            team: currentTurn,
            turnNumber: turnNumber
        ));
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