using UnityEngine;
using System.Collections.Generic;
using System;
public class MoveCommand : BaseCommand
{
   bool wasShotDown = false;

    public MoveCommand(
        BoardCoord from,
        BoardCoord to,
        Board board,
        CarryingSystem carryingSystem,
        StateBackupService backupService,
        PathChecker pathChecker,
        MovementExecutor movementExecutor)
        : base(from, to, board, carryingSystem, backupService, pathChecker, movementExecutor)
    {
        SelectedPiece = board.Pieces.ContainsKey(from) ? board.Pieces[from] : null;
        wasShotDown = false;
    }

    public override string Description => $"{SelectedPiece.Team} {SelectedPiece.Type} moves to {To.ToLabel()}";

    public override bool CanExecute()
    {
        if (SelectedPiece == null) return false;
        if (!board.IsInBoard(To)) return false;
        // Only allow plain move to empty tile (captures handled by CaptureCommand)
        if (board.Pieces.ContainsKey(To)) return false;
        if (!SelectedPiece.PossibleMoves.Contains(To)) return false;
        return true;
    }

    protected override bool DoExecute()
    {
        try
        {
            var pathResult = pathChecker.CheckPath(SelectedPiece, From, To).Result;

            // If path goes through or ends inside a danger zone -> immediately shot down
            if (pathResult == PathResult.GoThrough || pathResult == PathResult.Inside)
            {
                movementExecutor.RemoveFromBoard(SelectedPiece);
                wasShotDown = true;
                return true;
            }

            // Normal move
            var mv = movementExecutor.MovePiece(SelectedPiece, From, To);
            if (!mv.IsSuccess)
            {
                Debug.LogError($"MoveCommand: MovePiece failed: {mv.ErrorMessage}");
                return false;
            }

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"MoveCommand DoExecute failed: {e.Message}\n{e.StackTrace}");
            return false;
        }
    }

    protected override bool DoUndo()
    {
        try
        {
            if (wasShotDown)
            {
                // restore shot piece back to original position
                if (!movementExecutor.PlaceOnBoard(SelectedPiece, From))
                {
                    Debug.LogError("MoveCommand: Failed to restore shot piece to board");
                    return false;
                }
                return true;
            }

            // revert normal move (current = To, previous = From)
            var res = movementExecutor.RevertMovePiece(SelectedPiece, To, From);
            if (!res.IsSuccess)
            {
                Debug.LogError($"MoveCommand: Failed to revert move: {res.ErrorMessage}");
                return false;
            }

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"MoveCommand DoUndo failed: {e.Message}\n{e.StackTrace}");
            return false;
        }
    }
}
