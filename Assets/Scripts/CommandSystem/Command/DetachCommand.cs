using System;
using System.Linq;
using UnityEngine;

public class DetachCommand : BaseCommand
{
   // carrier from which we detach, passenger being detached, destination
    Piece carrier;
    Piece passenger;
    bool wasShotDown = false;

    public DetachCommand(
        BoardCoord from,
        BoardCoord to,
        Board board,
        CarryingSystem carryingSystem,
        StateBackupService backupService,
        PathChecker pathChecker,
        MovementExecutor movementExecutor)
        : base(from, to, board, carryingSystem, backupService, pathChecker, movementExecutor)
    {
        // 'from' is expected to be the carrier's board coord
        carrier = board.Pieces.ContainsKey(from) ? board.Pieces[from] : null;

        // pick a passenger to detach (first direct carried). If there are multiple callers should create specific command.
        passenger = carrier != null ? carryingSystem.GetDirectCarrying(carrier).FirstOrDefault() : null;

        // For description and template usage we treat passenger as SelectedPiece
        SelectedPiece = passenger;
        wasShotDown = false;
    }

    public override string Description =>
        passenger == null || carrier == null
            ? "Detach: invalid"
            : $"Detach: {passenger.Type} from {carrier.Type} -> {To.ToLabel()}";

    public override bool CanExecute()
    {
        if (carrier == null) return false;
        if (passenger == null) return false;
        if (!board.IsInBoard(To)) return false;
        // destination must be empty (no plain capture here)
        if (board.Pieces.ContainsKey(To)) return false;
        return true;
    }

    protected override bool DoExecute()
    {
        try
        {
            // remember start position (carrier position)
            var start = carrier.Position;

            // Perform detach in carrying system
            if (!carryingSystem.Detach(passenger))
            {
                Debug.LogError($"DetachCommand: Failed to detach {passenger.Type} from {carrier.Type}");
                return false;
            }

            // Check path from carrier position -> To
            var pathResult = pathChecker.CheckPath(passenger, start, To).Result;

            if (pathResult == PathResult.GoThrough || pathResult == PathResult.Inside)
            {
                // shot down during transit / landing
                // Ensure piece is not on board and mark as destroyed
                movementExecutor.RemoveFromBoard(passenger);
                wasShotDown = true;
                return true;
            }

            // Normal: place passenger on board at destination
            if (!movementExecutor.PlaceOnBoard(passenger, To))
            {
                Debug.LogError($"DetachCommand: Failed to place {passenger.Type} at {To.ToLabel()}");
                return false;
            }

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"DetachCommand DoExecute failed: {e.Message}\n{e.StackTrace}");
            return false;
        }
    }

    protected override bool DoUndo()
    {
        try
        {
            // If passenger was shot down -> reattach to carrier (restore original relationship)
            if (wasShotDown)
            {
                if (!carryingSystem.TryAddCarry(carrier, passenger))
                {
                    Debug.LogError($"DetachCommand: Failed to reattach (undo) {passenger?.Type} to {carrier?.Type}");
                    return false;
                }
                return true;
            }

            // Normal undo: remove passenger from board and reattach to carrier
            // If passenger currently on board at To, remove it
            movementExecutor.RemoveFromBoard(passenger);

            if (!carryingSystem.TryAddCarry(carrier, passenger))
            {
                Debug.LogError($"DetachCommand: Failed to reattach (undo) {passenger?.Type} to {carrier?.Type}");
                return false;
            }

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"DetachCommand DoUndo failed: {e.Message}\n{e.StackTrace}");
            return false;
        }
    }
}