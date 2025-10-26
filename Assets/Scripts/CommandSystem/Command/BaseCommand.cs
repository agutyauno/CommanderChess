using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class cho tất cả commands trong game
/// Sử dụng Template Method Pattern để định nghĩa flow chung:
/// 1. CanExecute() - Validation
/// 2. Execute() - Backup -> DoExecute() -> UpdateCache
/// 3. Undo() - DoUndo() -> Restore -> UpdateCache
/// </summary>
public abstract class BaseCommand : ICommand
{
    // Interface implementation
    public abstract string Description { get; }
    public DateTime Timestamp { get; private set; }
    public bool WasSuccessful { get; protected set; }

    // Dependencies - sẽ được inject từ constructor của derived classes
    protected readonly Board board;
    protected readonly CarryingSystem carryingSystem;
    protected readonly StateBackupService backupService;
    protected readonly PathChecker pathChecker;
    protected readonly MovementExecutor movementExecutor;

    // Backup snapshot
    protected StateBackupService.Snapshot snapshot;

    //fields
    protected BoardCoord From;
    protected BoardCoord To;
    protected Piece SelectedPiece;

    protected BaseCommand(
        BoardCoord from,
        BoardCoord to,
        Board board,
        CarryingSystem carryingSystem,
        StateBackupService backupService,
        PathChecker pathChecker,
        MovementExecutor movementExecutor)
    {
        From = from;
        To = to;
        this.board = board;
        this.carryingSystem = carryingSystem;
        this.backupService = backupService;
        this.pathChecker = pathChecker;
        this.movementExecutor = movementExecutor;
        Timestamp = DateTime.Now;
        WasSuccessful = false;
    }

    #region Template Method Pattern

    /// <summary>
    /// Template method - Định nghĩa flow thực thi command
    /// </summary>
    public bool Execute()
    {
        // Step 1: Validate
        if (!CanExecute())
        {
            WasSuccessful = false;
            Debug.LogWarning($"Cannot execute: {Description}");
            return false;
        }

        try
        {
            // Step 2: Backup state
            var piecesToBackup = GetPiecesToBackup();
            if (piecesToBackup != null && piecesToBackup.Length > 0)
            {
                snapshot = backupService.CreateSnapshot(piecesToBackup);
            }

            // Step 3: Execute actual logic
            WasSuccessful = DoExecute();

            if (WasSuccessful)
            {
                // Step 4: Update cache cho các pieces bị ảnh hưởng
                UpdateAffectedPiecesCache();

                Debug.Log($"{Description}");
                OnExecuteSuccess();
            }
            else
            {
                Debug.LogWarning($"{Description} failed");
            }

            return WasSuccessful;
        }
        catch (Exception e)
        {
            Debug.LogError($"Execute failed: {e.Message}\n{e.StackTrace}");
            WasSuccessful = false;
            return false;
        }
    }

    /// <summary>
    /// Template method - Định nghĩa flow undo command
    /// </summary>
    public bool Undo()
    {
        if (!WasSuccessful)
        {
            Debug.LogWarning($"Cannot undo: {Description} was not successful");
            return false;
        }

        if (snapshot == null)
        {
            Debug.LogWarning($"Cannot undo: {Description} has no backup");
            return false;
        }

        try
        {
            bool undoSuccess = DoUndo();
            if (undoSuccess)
            {
                backupService.RestoreSnapshot(snapshot);
                UpdateAffectedPiecesCache();

                Debug.Log($"Undo: {Description}");
                OnUndoSuccess();
            }

            return undoSuccess;
        }
        catch (Exception e)
        {
            Debug.LogError($"Undo failed: {e.Message}\n{e.StackTrace}");
            return false;
        }
    }

    #endregion

    #region Abstract Methods - Phải implement ở derived classes

    /// <summary>
    /// Kiểm tra xem command có thể thực thi không
    /// </summary>
    public abstract bool CanExecute();

    /// <summary>
    /// Logic thực thi chính của command
    /// Return true nếu thành công
    /// </summary>
    protected abstract bool DoExecute();

    /// <summary>
    /// Logic undo của command
    /// Return true nếu thành công
    /// Note: Restore state sẽ được xử lý tự động bởi BaseCommand
    /// </summary>
    protected abstract bool DoUndo();

    #endregion

    #region Virtual Methods - Có thể override nếu cần

    /// <summary>
    /// Hook được gọi sau khi Execute thành công
    /// Override nếu cần logic bổ sung
    /// </summary>
    protected virtual void OnExecuteSuccess() { }

    /// <summary>
    /// Hook được gọi sau khi Undo thành công
    /// Override nếu cần logic bổ sung
    /// </summary>
    protected virtual void OnUndoSuccess() { }

    /// <summary>
    /// Update cache cho các pieces bị ảnh hưởng
    /// Override nếu cần custom logic
    /// </summary>
    protected virtual void UpdateAffectedPiecesCache()
    {
        if (snapshot == null) return;

        foreach (var piece in snapshot.PieceData.Keys)
        {
            piece.RecalculateCache();
        }
    }

    #endregion

    #region Helper Methods
    /// <summary>
    /// Trả về danh sách pieces cần backup
    /// </summary>
    protected Piece[] GetPiecesToBackup()
    {
        var movedPiece = board.Pieces[From];
        var targetPiece = board.Pieces[To];
        List<Piece> piecesToBackUp = new();

        if (movedPiece != null)
        {
            piecesToBackUp.AddRange(carryingSystem.GetAllCarriedPieces(movedPiece));
        }

        if (targetPiece != null)
        {
            piecesToBackUp.AddRange(carryingSystem.GetAllCarriedPieces(targetPiece));
        }
        return piecesToBackUp.ToArray();
    }

    /// <summary>
    /// Helper: Update position của piece và các pieces nó đang mang
    /// </summary>
    protected void UpdatePieceAndCarriedPositions(Piece piece, BoardCoord newPosition)
    {
        piece.Position = newPosition;

        // TODO: Gửi event cập nhật vị trí
        // EventBus.Publish(new PieceMovedEvent(piece, newPosition));

        // Update carried pieces
        var carriedPieces = carryingSystem.GetAllCarriedPieces(piece);
        foreach (var carried in carriedPieces)
        {
            carried.Position = newPosition;
            // TODO: Gửi event
        }
    }

    /// <summary>
    /// Helper: Update board dictionary
    /// </summary>
    protected void UpdateBoardPosition(BoardCoord from, BoardCoord to, Piece piece)
    {
        board.Pieces.Remove(from);
        board.Pieces[to] = piece;
    }



    #endregion
}