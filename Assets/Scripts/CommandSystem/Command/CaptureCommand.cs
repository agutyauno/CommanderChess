using System;
using UnityEngine;

public class CaptureCommand : BaseCommand
{
     Piece pieceToCapture;
    bool attackerShotDown = false;
    bool defenderShotDown = false;

    public CaptureCommand(
        BoardCoord from,
        BoardCoord to,
        Board board,
        CarryingSystem carryingSystem,
        StateBackupService backupService,
        PathChecker pathChecker,
        MovementExecutor movementExecutor) :
        base(from, to, board, carryingSystem, backupService, pathChecker, movementExecutor)
    {
        SelectedPiece = board.Pieces.ContainsKey(from) ? board.Pieces[from] : null;
        pieceToCapture = board.Pieces.ContainsKey(to) ? board.Pieces[to] : null;
    }

    public override string Description => $"{SelectedPiece?.Team} {SelectedPiece?.Type} captures {pieceToCapture?.Team} {pieceToCapture?.Type} at {To.ToLabel()}";

    public override bool CanExecute()
    {
        if (SelectedPiece == null || pieceToCapture == null) return false;
        if (!SelectedPiece.PossibleAttacks.Contains(To)) return false;
        if (pieceToCapture.Team == SelectedPiece.Team) return false;
        return true;
    }

    protected override bool DoExecute()
    {
        try
        {
            var pathResult = pathChecker.CheckPath(SelectedPiece, From, To).Result;

            switch (pathResult)
            {
                case PathResult.GoThrough:
                    // path goes through danger zone -> attacker is shot down before reaching target
                    movementExecutor.RemoveFromBoard(SelectedPiece);
                    attackerShotDown = true;
                    return true;

                case PathResult.Inside:
                    // destination is inside danger zone -> 1-for-1 exchange (both destroyed)
                    movementExecutor.RemoveFromBoard(SelectedPiece);
                    movementExecutor.RemoveFromBoard(pieceToCapture);
                    attackerShotDown = true;
                    defenderShotDown = true;
                    return true;

                case PathResult.None:
                    // safe: normal capture - remove defender and move attacker into its square
                    movementExecutor.RemoveFromBoard(pieceToCapture);
                    defenderShotDown = true;
                    if (SelectedPiece.DoMoveToTarget)
                    {
                        var result = movementExecutor.MovePiece(SelectedPiece, From, To);
                        if (!result.IsSuccess)
                        {
                            Debug.LogError($"CaptureCommand: MovePiece failed: {result.ErrorMessage}");
                            return false;
                        }
                    }
                    return true;

                default:
                    Debug.LogError($"CaptureCommand: Unknown PathResult {pathResult}");
                    return false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"CaptureCommand DoExecute failed: {e.Message}\n{e.StackTrace}");
            return false;
        }
    }

    protected override bool DoUndo()
    {
        try
        {
            // If attacker was shot down (GoThrough or Inside) restore it
            if (attackerShotDown)
            {
                if (!movementExecutor.PlaceOnBoard(SelectedPiece, From))
                {
                    Debug.LogError("CaptureCommand: Failed to restore attacker to board");
                    return false;
                }
            }

            // If defender was destroyed (Inside or normal capture) restore it to original pos
            if (defenderShotDown)
            {
                if (!movementExecutor.PlaceOnBoard(pieceToCapture, To))
                {
                    Debug.LogError("CaptureCommand: Failed to restore defender to board");
                    return false;
                }
            }
            else if (!attackerShotDown)
            {
                // Normal capture case: defender removed and attacker moved -> revert attacker move and restore defender
                if (!movementExecutor.PlaceOnBoard(pieceToCapture, To))
                {
                    Debug.LogError("CaptureCommand: Failed to restore defender to board (normal capture)");
                    return false;
                }

                var res = movementExecutor.RevertMovePiece(SelectedPiece, To, From);
                if (!res.IsSuccess)
                {
                    Debug.LogError($"CaptureCommand: Failed to revert attacker move: {res.ErrorMessage}");
                    return false;
                }
            }

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"CaptureCommand DoUndo failed: {e.Message}\n{e.StackTrace}");
            return false;
        }
    }
}
