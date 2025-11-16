using System;
using System.Linq;
using UnityEngine;
using CommanderChess.Domain;
using CommanderChess.Services;

namespace CommanderChess.CommandSystem
{
    public class DetachCommand : BaseCommand
    {
        BasePiece carrier;
        BasePiece passenger;

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
            carrier = board.Pieces.ContainsKey(from) ? board.Pieces[from] : null;
            passenger = carrier != null ? carryingSystem.GetDirectCarrying(carrier).FirstOrDefault() : null;
            SelectedPiece = passenger;
        }

        public override string Description =>
            passenger == null || carrier == null
                ? "Detach: invalid"
                : $"Detach: {passenger.Type} from {carrier.Type} -> {To.ToLabel()}";

        public override bool CanExecute()
        {
            if (carrier == null || passenger == null) return false;
            if (!board.IsInBoard(To)) return false;
            if (board.Pieces.ContainsKey(To)) return false; // Must be empty
            return true;
        }

        protected override bool DoExecute()
        {
            try
            {
                var carrierPos = carrier.Position;

                // Detach from carrying system
                if (!carryingSystem.Detach(passenger))
                {
                    Debug.LogError($"DetachCommand: Failed to detach {passenger.Type}");
                    return false;
                }

                // Check path from carrier position to destination
                var pathResult = pathChecker.CheckPath(passenger, carrierPos, To).Result;

                if (pathResult == PathResult.GoThrough || pathResult == PathResult.Inside)
                {
                    // Passenger shot down during detach
                    movementExecutor.ShotDownPiece(passenger);
//                     Debug.Log($"  {passenger.Type} shot down during detach!");
                    return true;
                }

                // Normal detach
                var result = movementExecutor.ExecuteDetach(passenger, carrierPos, To);
                if (!result.IsSuccess)
                {
                    Debug.LogError($"DetachCommand: ExecuteDetach failed: {result.ErrorMessage}");
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
    }
}
