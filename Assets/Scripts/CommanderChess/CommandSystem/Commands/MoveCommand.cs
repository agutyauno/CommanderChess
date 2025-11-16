using UnityEngine;
using System;
using CommanderChess.Domain;
using CommanderChess.Services;

namespace CommanderChess.CommandSystem
{
    public class MoveCommand : BaseCommand
    {

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
        }

        public override string Description => $"{SelectedPiece?.Team} {SelectedPiece?.Type} moves to {To.ToLabel()}";

        public override bool CanExecute()
        {
            if (SelectedPiece == null) return false;
            if (!board.IsInBoard(To)) return false;
            if (board.Pieces.ContainsKey(To)) return false; // Must be empty
            if (!SelectedPiece.PossibleMoves.Contains(To)) return false;
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
//                     Debug.Log($"  {SelectedPiece.Type} was shot down!");
                    return true;
                }

                // Safe movement
                var result = movementExecutor.MovePiece(SelectedPiece, From, To);
                if (!result.IsSuccess)
                {
                    Debug.LogError($"MoveCommand: MovePiece failed: {result.ErrorMessage}");
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
    }
}
