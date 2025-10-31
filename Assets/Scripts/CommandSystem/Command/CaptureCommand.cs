using System;
using UnityEngine;

public class CaptureCommand : BaseCommand
{
    BasePiece defender;
    bool attackerShotDown = false;
    bool defenderDestroyed = false;

    public CaptureCommand(
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
        defender = board.Pieces.ContainsKey(to) ? board.Pieces[to] : null;
    }

    public override string Description =>
        $"{SelectedPiece?.Team} {SelectedPiece?.Type} captures {defender?.Team} {defender?.Type} at {To.ToLabel()}";

    public override bool CanExecute()
    {
        if (SelectedPiece == null || defender == null) return false;
        if (!SelectedPiece.PossibleAttacks.Contains(To)) return false;
        if (defender.Team == SelectedPiece.Team) return false;
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
                    // Attacker shot down before reaching target
                    movementExecutor.ShotDownPiece(SelectedPiece);
                    attackerShotDown = true;
                    Debug.Log($"  {SelectedPiece.Type} shot down before reaching target!");
                    return true;

                case PathResult.Inside:
                    // Both destroyed (1-for-1 trade)
                    movementExecutor.ShotDownPiece(SelectedPiece);
                    movementExecutor.ShotDownPiece(defender);
                    attackerShotDown = true;
                    defenderDestroyed = true;
                    Debug.Log($"  1-for-1 trade! Both pieces destroyed!");
                    return true;

                case PathResult.None:
                    // Normal capture
                    var result = movementExecutor.ExecuteCapture(SelectedPiece, defender, From, To);
                    if (!result.IsSuccess)
                    {
                        Debug.LogError($"CaptureCommand: ExecuteCapture failed: {result.ErrorMessage}");
                        return false;
                    }
                    defenderDestroyed = true;
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
            // Case 1: Attacker shot down (GoThrough or Inside)
            if (attackerShotDown)
            {
                if (!movementExecutor.PlaceOnBoard(SelectedPiece, From))
                {
                    Debug.LogError("CaptureCommand: Failed to restore attacker");
                    return false;
                }
            }

            // Case 2: Defender destroyed (Inside or normal capture)
            if (defenderDestroyed)
            {
                if (!movementExecutor.PlaceOnBoard(defender, To))
                {
                    Debug.LogError("CaptureCommand: Failed to restore defender");
                    return false;
                }
            }

            // Case 3: Normal capture - need to move attacker back
            if (!attackerShotDown && defenderDestroyed)
            {
                var result = movementExecutor.RevertCapture(SelectedPiece, defender, From, To);
                if (!result.IsSuccess)
                {
                    Debug.LogError($"CaptureCommand: RevertCapture failed: {result.ErrorMessage}");
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
