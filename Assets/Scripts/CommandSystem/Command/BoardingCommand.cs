using System;
using UnityEngine;

public class BoardingCommand : BaseCommand
{
    BasePiece target;
    bool wasShotDown = false;
    bool moverBecamePassenger = false;

    public BoardingCommand(
        BoardCoord from,
        BoardCoord to,
        Board board,
        CarryingSystem carryingSystem,
        StateBackupService backupService,
        PathChecker pathChecker,
        MovementExecutor movementExecutor)
        : base(from, to, board, carryingSystem, backupService, pathChecker, movementExecutor)
    {
        SelectedPiece = board.Pieces[from];
        target = board.Pieces[to];
        wasShotDown = false;
        moverBecamePassenger = false;
    }

    public override string Description =>
        $"Boarding: {SelectedPiece.Type} -> {target.Type} at {To.ToLabel()}";

    public override bool CanExecute()
    {
        if (SelectedPiece == null || target == null) return false;
        if (!SelectedPiece.PossibleMoves.Contains(To)) return false;
        if (SelectedPiece.Team != target.Team) return false;
        return true;
    }

    protected override bool DoExecute()
    {
        try
        {
            var pathResult = pathChecker.CheckPath(SelectedPiece, From, To).Result;

            // Check for danger zones
            if (pathResult == PathResult.GoThrough || pathResult == PathResult.Inside)
            {
                movementExecutor.ShotDownPiece(SelectedPiece);
                wasShotDown = true;
                Debug.Log($"  {SelectedPiece.Type} shot down during boarding!");
                return true;
            }

            // Attempt boarding (CarryingSystem decides who carries who)
            if (!carryingSystem.TryAddCarry(SelectedPiece, target))
            {
                Debug.LogError("BoardingCommand: TryAddCarry failed");
                return false;
            }

            // Check who became passenger
            moverBecamePassenger = carryingSystem.GetCarrier(SelectedPiece) == target;

            // Execute boarding movement
            var result = movementExecutor.ExecuteBoarding(SelectedPiece, target, From, To, moverBecamePassenger);
            if (!result.IsSuccess)
            {
                Debug.LogError($"BoardingCommand: ExecuteBoarding failed: {result.ErrorMessage}");
                return false;
            }

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"BoardingCommand DoExecute failed: {e.Message}\n{e.StackTrace}");
            return false;
        }
    }

    protected override bool DoUndo()
    {
        try
        {
            if (wasShotDown)
            {
                // Restore shot down piece
                if (!movementExecutor.PlaceOnBoard(SelectedPiece, From))
                {
                    Debug.LogError("BoardingCommand: Failed to restore shot piece");
                    return false;
                }
                return true;
            }

            // Revert boarding
            var result = movementExecutor.RevertBoarding(
                SelectedPiece, target, From, To, moverBecamePassenger);
            
            if (!result.IsSuccess)
            {
                Debug.LogError($"BoardingCommand: RevertBoarding failed: {result.ErrorMessage}");
                return false;
            }

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"BoardingCommand DoUndo failed: {e.Message}\n{e.StackTrace}");
            return false;
        }
    }
}
