using System;
using System.Collections.Generic;
using UnityEngine;
using CommanderChess.Domain;
using CommanderChess.Services;

namespace CommanderChess.CommandSystem
{
    /// <summary>
    /// Base class cho tất cả commands trong game
    /// REFACTORED: Không còn quản lý backup/undo - TurnManager quản lý turn-level snapshots
    /// Sử dụng Template Method Pattern để định nghĩa flow chung:
    /// 1. CanExecute() - Validation
    /// 2. Execute() - DoExecute() -> UpdateCache
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
        protected readonly StateBackupService backupService; // Keep for internal use if needed
        protected readonly PathChecker pathChecker;
        protected readonly MovementExecutor movementExecutor;

        //fields
        protected BoardCoord From;
        protected BoardCoord To;
        protected BasePiece SelectedPiece;

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
        /// REFACTORED: Không còn tạo snapshot - TurnManager quản lý
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
                // Step 2: Execute actual logic
                WasSuccessful = DoExecute();

                if (WasSuccessful)
                {
                    // Step 3: Update cache cho các pieces bị ảnh hưởng
                    UpdateAffectedPiecesCache();

//                     Debug.Log($"{Description}");
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
        /// DoUndo is no longer used - kept for backward compatibility
        /// TurnManager handles all undo operations at turn-level
        /// </summary>
        protected virtual bool DoUndo() 
        { 
            return false; 
        }

        #endregion

        #region Virtual Methods - Có thể override nếu cần

        /// <summary>
        /// Hook được gọi sau khi Execute thành công
        /// Override nếu cần logic bổ sung
        /// </summary>
        protected virtual void OnExecuteSuccess() { }

        /// <summary>
        /// Update cache cho các pieces bị ảnh hưởng
        /// Override nếu cần custom logic
        /// </summary>
        protected virtual void UpdateAffectedPiecesCache()
        {
            // Update cache cho piece đã di chuyển
            board.Pieces.TryGetValue(From, out var fromPiece);
            board.Pieces.TryGetValue(To, out var toPiece);
            
            fromPiece?.RecalculateCache();
            toPiece?.RecalculateCache();
            
            // Update carried pieces
            if (fromPiece != null)
            {
                foreach (var carried in carryingSystem.GetAllCarriedPieces(fromPiece))
                {
                    carried.RecalculateCache();
                }
            }
            
            if (toPiece != null)
            {
                foreach (var carried in carryingSystem.GetAllCarriedPieces(toPiece))
                {
                    carried.RecalculateCache();
                }
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Helper: Update position của piece và các pieces nó đang mang
        /// </summary>
        protected void UpdatePieceAndCarriedPositions(BasePiece piece, BoardCoord newPosition)
        {
            piece.Position = newPosition;

            // Update carried pieces
            var carriedPieces = carryingSystem.GetAllCarriedPieces(piece);
            foreach (var carried in carriedPieces)
            {
                carried.Position = newPosition;
            }
        }

        /// <summary>
        /// Helper: Update board dictionary
        /// </summary>
        protected void UpdateBoardPosition(BoardCoord from, BoardCoord to, BasePiece piece)
        {
            board.Pieces.Remove(from);
            board.Pieces[to] = piece;
        }

        #endregion
    }
}
