using System.Collections.Generic;
using UnityEngine;
using VContainer;
using CommanderChess.Domain;
using CommanderChess.Services;

namespace CommanderChess.CommandSystem
{
    /// <summary>
    /// CommandManager - Quản lý tạo và thực thi commands (Factory + Invoker)
    /// REFACTORED: Commands không còn quản lý backup, TurnManager quản lý turn-level snapshots
    /// </summary>
    public class CommandManager
    {
        #region Dependencies
        [Inject] readonly Board board;
        [Inject] readonly CarryingSystem carryingSystem;
        [Inject] readonly StateBackupService backupService;
        [Inject] readonly PathChecker pathChecker;
        [Inject] readonly MovementExecutor movementExecutor;
        [Inject] readonly TurnManager turnManager; // NEW: Inject TurnManager
        [Inject] readonly EventBus eventBus;
        #endregion

        #region History Stacks
        // Note: Undo/Redo giờ được quản lý bởi TurnManager
        // Chỉ giữ lại command history cho debugging
        readonly List<ICommand> executedCommands = new List<ICommand>();
        const int maxHistorySize = 50;
        #endregion

        #region Factory Methods

        /// <summary>
        /// Tạo MoveCommand
        /// </summary>
        public MoveCommand CreateMoveCommand(BoardCoord from, BoardCoord to)
        {
            return new MoveCommand(
                from, to,
                board, carryingSystem, backupService, pathChecker, movementExecutor
            );
        }

        /// <summary>
        /// Tạo CaptureCommand
        /// </summary>
        public CaptureCommand CreateCaptureCommand(BoardCoord from, BoardCoord to)
        {
            return new CaptureCommand(
                from, to,
                board, carryingSystem, backupService, pathChecker, movementExecutor
            );
        }

        /// <summary>
        /// Tạo BoardingCommand
        /// </summary>
        public BoardingCommand CreateBoardingCommand(BoardCoord from, BoardCoord to)
        {
            return new BoardingCommand(
                from, to,
                board, carryingSystem, backupService, pathChecker, movementExecutor
            );
        }

        /// <summary>
        /// Tạo DetachCommand
        /// </summary>
        public DetachCommand CreateDetachCommand(BoardCoord from, BoardCoord to)
        {
            return new DetachCommand(
                from, to,
                board, carryingSystem, backupService, pathChecker, movementExecutor
            );
        }

        #endregion

        #region Execution Methods

        /// <summary>
        /// Execute command và record vào TurnManager
        /// </summary>
        public bool Execute(ICommand command)
        {
            if (command == null)
            {
                Debug.LogError("Cannot execute null command");
                return false;
            }

            // Execute
            bool success = command.Execute();

            if (success)
            {
                // Record command vào turn history
                turnManager.RecordCommand(command);
                
                // Add to executed commands list (for debugging)
                executedCommands.Add(command);
                
                // Limit history size
                if (executedCommands.Count > maxHistorySize)
                {
                    executedCommands.RemoveAt(0);
                }

//                 Debug.Log($"Command executed: {command.Description}");
            }
            else
            {
                Debug.LogError($"Command execution failed: {command.Description}");
            }

            return success;
        }

        /// <summary>
        /// Undo turn - delegated to TurnManager
        /// </summary>
        public bool UndoTurn()
        {
            return turnManager.UndoTurn();
        }

        /// <summary>
        /// Check if can undo turn
        /// </summary>
        public bool CanUndo()
        {
            return turnManager.CanUndo;
        }

        #endregion

        #region History Management

        /// <summary>
        /// Clear all command history
        /// </summary>
        public void ClearHistory()
        {
            executedCommands.Clear();
            turnManager.ClearHistory();
//             Debug.Log("Command history cleared");
        }

        /// <summary>
        /// Get last executed command (for debugging)
        /// </summary>
        public ICommand GetLastCommand()
        {
            return executedCommands.Count > 0 ? executedCommands[executedCommands.Count - 1] : null;
        }

        /// <summary>
        /// Get all command history (for debugging/replay)
        /// </summary>
        public List<ICommand> GetCommandHistory()
        {
            return new List<ICommand>(executedCommands);
        }

        /// <summary>
        /// Get history count
        /// </summary>
        public int GetHistoryCount()
        {
            return executedCommands.Count;
        }

        #endregion
    }
}
