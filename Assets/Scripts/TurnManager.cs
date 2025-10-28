using System;
using UnityEngine;

/// <summary>
/// TurnManager - Quản lý lượt chơi trong game
/// </summary>
public class TurnManager
{
    Team currentTurn = Team.Red;
    int turnNumber = 1;

    #region Properties
    public Team CurrentTurn => currentTurn;
    public int TurnNumber => turnNumber;
    #endregion

    #region Events
    public event Action<Team> OnTurnChanged;
    public event Action<Team> OnTurnStarted;
    #endregion

    #region Public API

    /// <summary>
    /// Kiểm tra xem piece có thuộc về người chơi hiện tại không
    /// </summary>
    public bool IsCurrentPlayerPiece(Piece piece)
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

        // Emit events
        OnTurnChanged?.Invoke(currentTurn);
        OnTurnStarted?.Invoke(currentTurn);
    }

    /// <summary>
    /// Reset về lượt đầu tiên (dùng khi restart game)
    /// </summary>
    public void ResetTurn()
    {
        currentTurn = Team.Red;
        turnNumber = 1;
        
        Debug.Log($"Turn reset: {currentTurn}'s turn");
        OnTurnStarted?.Invoke(currentTurn);
    }

    /// <summary>
    /// Đặt lượt cụ thể (dùng khi load game hoặc sync online)
    /// </summary>
    public void SetTurn(Team team, int turn)
    {
        currentTurn = team;
        turnNumber = turn;
        
        Debug.Log($"Turn set to {turnNumber}: {currentTurn}'s turn");
        OnTurnChanged?.Invoke(currentTurn);
    }

    #endregion
}
