using System.Collections.Generic;
using UnityEngine;
using VContainer;

/// <summary>
/// CommandManager - Quản lý tạo và thực thi commands (Factory + Invoker)
/// </summary>
public class CommandManager
{
    #region Dependencies
    [Inject] readonly Board board;
    [Inject] readonly CarryingSystem carryingSystem;
    [Inject] readonly StateBackupService backupService;
    [Inject] readonly PathChecker pathChecker;
    [Inject] readonly MovementExecutor movementExecutor;
    #endregion

    #region History Stacks
    readonly Stack<ICommand> undoStack = new Stack<ICommand>();
    readonly Stack<ICommand> redoStack = new Stack<ICommand>();
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
    /// Execute command và thêm vào history
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
            // Add to undo stack
            undoStack.Push(command);

            // Limit history size
            if (undoStack.Count > maxHistorySize)
            {
                // Remove oldest command
                var temp = new Stack<ICommand>();
                while (undoStack.Count > maxHistorySize)
                {
                    var oldest = undoStack.Pop();
                    if (undoStack.Count > 0)
                        temp.Push(oldest);
                }
                while (temp.Count > 0)
                    undoStack.Push(temp.Pop());
            }

            // Clear redo stack (new action invalidates redo history)
            redoStack.Clear();

            Debug.Log($"Command executed: {command.Description}");
        }
        else
        {
            Debug.LogError($"Command execution failed: {command.Description}");
        }

        return success;
    }

    /// <summary>
    /// Undo last command
    /// </summary>
    public bool Undo()
    {
        if (!CanUndo())
        {
            Debug.LogWarning("Cannot undo - no commands in history");
            return false;
        }

        var command = undoStack.Pop();
        bool success = command.Undo();

        if (success)
        {
            redoStack.Push(command);
            Debug.Log($"Command undone: {command.Description}");
        }
        else
        {
            // Push back to undo stack if undo failed
            undoStack.Push(command);
            Debug.LogError($"Undo failed: {command.Description}");
        }

        return success;
    }

    /// <summary>
    /// Redo last undone command
    /// </summary>
    public bool Redo()
    {
        if (!CanRedo())
        {
            Debug.LogWarning("Cannot redo - no commands in redo history");
            return false;
        }

        var command = redoStack.Pop();
        bool success = command.Execute();

        if (success)
        {
            undoStack.Push(command);
            Debug.Log($"Command redone: {command.Description}");
        }
        else
        {
            // Push back to redo stack if redo failed
            redoStack.Push(command);
            Debug.LogError($"Redo failed: {command.Description}");
        }

        return success;
    }

    /// <summary>
    /// Check if can undo
    /// </summary>
    public bool CanUndo()
    {
        return undoStack.Count > 0;
    }

    /// <summary>
    /// Check if can redo
    /// </summary>
    public bool CanRedo()
    {
        return redoStack.Count > 0;
    }

    #endregion

    #region History Management

    /// <summary>
    /// Clear all command history
    /// </summary>
    public void ClearHistory()
    {
        undoStack.Clear();
        redoStack.Clear();
        Debug.Log("Command history cleared");
    }

    /// <summary>
    /// Get last executed command (for debugging)
    /// </summary>
    public ICommand GetLastCommand()
    {
        return undoStack.Count > 0 ? undoStack.Peek() : null;
    }

    /// <summary>
    /// Get all command history (for debugging/replay)
    /// </summary>
    public List<ICommand> GetCommandHistory()
    {
        return new List<ICommand>(undoStack);
    }

    /// <summary>
    /// Get history count
    /// </summary>
    public int GetHistoryCount()
    {
        return undoStack.Count;
    }

    #endregion
}