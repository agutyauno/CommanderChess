using System;
using UnityEngine;
using CommanderChess.Domain;
using CommanderChess.Services;

namespace CommanderChess.CommandSystem
{
    public class CaptureCommand : BaseCommand
    {
        BasePiece defender;
        bool attackerShotDown = false;
        bool defenderDestroyed = false;
        bool shouldMoveToTarget = true; // Mặc định là di chuyển

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
            
            // Xác định xem có nên di chuyển tới target không
            DetermineMoveToTarget();
        }

        public override string Description =>
            $"{SelectedPiece?.Team} {SelectedPiece?.Type} captures {defender?.Team} {defender?.Type} at {To.ToLabel()}";

        /// <summary>
        /// Xác định xem attacker có nên di chuyển tới vị trí defender không
        /// Logic đặc biệt cho Navy và các piece khác
        /// </summary>
        void DetermineMoveToTarget()
        {
            if (SelectedPiece == null || defender == null)
            {
                shouldMoveToTarget = true; // fallback
                return;
            }

            // Case 3: Các piece khác - dùng DoMoveToTarget từ PieceData
            shouldMoveToTarget = SelectedPiece.ShouldMoveToTarget(To);
        }

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
                        var result = movementExecutor.ExecuteCapture(
                            SelectedPiece, 
                            defender, 
                            From, 
                            To, 
                            shouldMoveToTarget  // Truyền flag vào đây
                        );
                        
                        if (!result.IsSuccess)
                        {
                            Debug.LogError($"CaptureCommand: ExecuteCapture failed: {result.ErrorMessage}");
                            return false;
                        }
                        
                        defenderDestroyed = true;
                        
                        if (!shouldMoveToTarget)
                        {
                            Debug.Log($"  {SelectedPiece.Type} captured {defender.Type} without moving (ranged attack)");
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
        // ===================================
        // 🔧 FIX: Xử lý rõ ràng từng case
        // ===================================
        
        Debug.Log($"[CaptureCommand.DoUndo] Starting undo:");
        Debug.Log($"  - Attacker: {SelectedPiece?.Type} (ShotDown: {attackerShotDown})");
        Debug.Log($"  - Defender: {defender?.Type} (Destroyed: {defenderDestroyed})");
        Debug.Log($"  - Snapshot: {(snapshot != null ? $"{snapshot.PieceData.Count} pieces" : "NULL")}");

        if (attackerShotDown && defenderDestroyed)
        {
            // Case 1: Inside (1-for-1 trade)
            // Cả 2 piece đều bị shot down
            
            Debug.Log($"Undo Case 1: Inside (1-for-1) - Restoring both pieces");
            
            // StateBackupService sẽ restore positions và relationships
            // Chỉ cần enable lại visuals
            if (SelectedPiece.gameObject != null)
            {
                SelectedPiece.gameObject.SetActive(true);
            }
            
            if (defender.gameObject != null)
            {
                defender.gameObject.SetActive(true);
            }
            
            // Restore carried pieces visuals
            var attackerCarried = carryingSystem.GetAllCarriedPieces(SelectedPiece);
            foreach (var carried in attackerCarried)
            {
                if (carried?.gameObject != null)
                {
                    carried.gameObject.SetActive(true);
                }
            }
            
            var defenderCarried = carryingSystem.GetAllCarriedPieces(defender);
            foreach (var carried in defenderCarried)
            {
                if (carried?.gameObject != null)
                {
                    carried.gameObject.SetActive(true);
                }
            }
            
            return true;
        }
        else if (attackerShotDown && !defenderDestroyed)
        {
            // Case 2: GoThrough
            // Chỉ attacker bị shot down trước khi đến target
            
            Debug.Log($"Undo Case 2: GoThrough - Restoring attacker only");
            
            // StateBackupService sẽ restore position
            // Chỉ cần enable lại visuals
            if (SelectedPiece.gameObject != null)
            {
                SelectedPiece.gameObject.SetActive(true);
            }
            
            // Restore carried pieces visuals
            var attackerCarried = carryingSystem.GetAllCarriedPieces(SelectedPiece);
            foreach (var carried in attackerCarried)
            {
                if (carried?.gameObject != null)
                {
                    carried.gameObject.SetActive(true);
                }
            }
            
            return true;
        }
        else if (!attackerShotDown && defenderDestroyed)
        {
            // Case 3: Normal capture (PathResult.None)
            // Attacker thành công, defender bị destroyed
            
            Debug.Log($"Undo Case 3: Normal Capture - Restoring defender, reverting attacker movement");
            
            // Enable defender visuals
            if (defender.gameObject != null)
            {
                defender.gameObject.SetActive(true);
            }
            
            // Restore defender's carried pieces visuals
            var defenderCarried = carryingSystem.GetAllCarriedPieces(defender);
            foreach (var carried in defenderCarried)
            {
                if (carried?.gameObject != null)
                {
                    carried.gameObject.SetActive(true);
                }
            }
            
            // Revert attacker movement CHỈ NẾU có di chuyển
            if (shouldMoveToTarget)
            {
                var result = movementExecutor.RevertCapture(
                    SelectedPiece, 
                    defender, 
                    From, 
                    To,
                    shouldMoveToTarget
                );
                
                if (!result.IsSuccess)
                {
                    Debug.LogError($"CaptureCommand: RevertCapture failed: {result.ErrorMessage}");
                    return false;
                }
            }
            else
            {
                // Attacker không di chuyển, chỉ cần restore defender
                Debug.Log($"Attacker didn't move, only restoring defender");
            }
            
            return true;
        }
        else
        {
            // Case không xác định
            Debug.LogError($"CaptureCommand DoUndo: Unknown case - attackerShotDown={attackerShotDown}, defenderDestroyed={defenderDestroyed}");
            return false;
        }
    }
    catch (Exception e)
    {
        Debug.LogError($"CaptureCommand DoUndo failed: {e.Message}\n{e.StackTrace}");
        return false;
    }
}

    }
}