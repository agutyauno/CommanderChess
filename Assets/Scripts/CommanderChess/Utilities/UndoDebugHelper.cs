using UnityEngine;
using CommanderChess.Domain;
using CommanderChess.Services;
using System.Collections.Generic;

namespace CommanderChess.Utilities
{
    /// <summary>
    /// Debug utility để test và verify undo functionality
    /// </summary>
    public static class UndoDebugHelper
    {
        /// <summary>
        /// Log chi tiết board state hiện tại
        /// </summary>
        public static void LogBoardState(Board board, string prefix = "")
        {
            Debug.Log($"{prefix}========== BOARD STATE ==========");
            Debug.Log($"{prefix}Total pieces on board: {board.Pieces.Count}");
            
            foreach (var kvp in board.Pieces)
            {
                var pos = kvp.Key;
                var piece = kvp.Value;
                Debug.Log($"{prefix}  [{pos}] = {piece.Type} (Team: {piece.Team}, Active: {piece.gameObject?.activeSelf}, Hero: {piece.IsHero})");
            }
            
            Debug.Log($"{prefix}================================");
        }

        /// <summary>
        /// Log chi tiết carrying relationships
        /// </summary>
        public static void LogCarryingState(Board board, CarryingSystem carryingSystem, string prefix = "")
        {
            Debug.Log($"{prefix}========== CARRYING STATE ==========");
            
            foreach (var piece in board.Pieces.Values)
            {
                var carrier = carryingSystem.GetCarrier(piece);
                var carrying = carryingSystem.GetDirectCarrying(piece);
                
                if (carrier != null || carrying.Count > 0)
                {
                    Debug.Log($"{prefix}{piece.Type} at {piece.Position}:");
                    
                    if (carrier != null)
                        Debug.Log($"{prefix}  - Carried by: {carrier.Type}");
                    
                    if (carrying.Count > 0)
                    {
                        var carryingNames = new List<string>();
                        foreach (var p in carrying)
                        {
                            carryingNames.Add(p.Type.ToString());
                        }
                        Debug.Log($"{prefix}  - Carrying: {string.Join(", ", carryingNames)}");
                    }
                }
            }
            
            Debug.Log($"{prefix}====================================");
        }

        /// <summary>
        /// Verify tất cả pieces có position logic = position visual
        /// </summary>
        public static bool ValidateVisualPositions(Board board, string prefix = "")
        {
            bool allValid = true;
            
            foreach (var piece in board.Pieces.Values)
            {
                if (piece.gameObject == null) continue;
                
                var logicPos = piece.Position;
                var expectedWorldPos = board.BoardCoordToWorld(logicPos);
                var actualWorldPos = piece.gameObject.transform.position;
                
                float distance = Vector3.Distance(expectedWorldPos, actualWorldPos);
                
                if (distance > 0.1f) // Tolerance
                {
                    Debug.LogError(
                        $"{prefix}❌ {piece.Type} visual position mismatch!\n" +
                        $"  Logic: {logicPos} -> World: {expectedWorldPos}\n" +
                        $"  Actual: {actualWorldPos}\n" +
                        $"  Distance: {distance}"
                    );
                    allValid = false;
                }
            }
            
            if (allValid)
            {
                Debug.Log($"{prefix}✅ All visual positions match logic positions");
            }
            
            return allValid;
        }

        /// <summary>
        /// Verify board dictionary consistency
        /// </summary>
        public static bool ValidateBoardDictionary(Board board, string prefix = "")
        {
            bool allValid = true;
            
            // Check: mỗi piece trong dictionary phải có position match với key
            foreach (var kvp in board.Pieces)
            {
                var pos = kvp.Key;
                var piece = kvp.Value;
                
                if (piece.Position != pos)
                {
                    Debug.LogError(
                        $"{prefix}❌ Board dictionary inconsistency!\n" +
                        $"  Key: {pos}\n" +
                        $"  Piece: {piece.Type} at {piece.Position}"
                    );
                    allValid = false;
                }
            }
            
            if (allValid)
            {
                Debug.Log($"{prefix}✅ Board dictionary is consistent");
            }
            
            return allValid;
        }

        /// <summary>
        /// Complete validation sau khi undo
        /// </summary>
        public static bool ValidateAfterUndo(Board board, CarryingSystem carryingSystem, string turnInfo = "")
        {
            Debug.Log($"\n========== VALIDATING UNDO: {turnInfo} ==========");
            
            LogBoardState(board, "[Undo] ");
            LogCarryingState(board, carryingSystem, "[Undo] ");
            
            bool visualValid = ValidateVisualPositions(board, "[Undo] ");
            bool dictValid = ValidateBoardDictionary(board, "[Undo] ");
            
            bool allValid = visualValid && dictValid;
            
            if (allValid)
            {
                Debug.Log($"[Undo] ✅✅✅ ALL VALIDATIONS PASSED ✅✅✅\n");
            }
            else
            {
                Debug.LogError($"[Undo] ❌❌❌ VALIDATION FAILED ❌❌❌\n");
            }
            
            return allValid;
        }

        /// <summary>
        /// Compare two board states
        /// </summary>
        public static void CompareBoardStates(
            Dictionary<BoardCoord, BasePiece> state1,
            Dictionary<BoardCoord, BasePiece> state2,
            string label1 = "State 1",
            string label2 = "State 2")
        {
            Debug.Log($"========== COMPARING: {label1} vs {label2} ==========");
            
            // Pieces in state1 but not in state2
            foreach (var kvp in state1)
            {
                if (!state2.ContainsKey(kvp.Key))
                {
                    Debug.Log($"  [{kvp.Key}] {kvp.Value.Type} - Only in {label1}");
                }
                else if (state2[kvp.Key] != kvp.Value)
                {
                    Debug.Log($"  [{kvp.Key}] Different: {kvp.Value.Type} vs {state2[kvp.Key].Type}");
                }
            }
            
            // Pieces in state2 but not in state1
            foreach (var kvp in state2)
            {
                if (!state1.ContainsKey(kvp.Key))
                {
                    Debug.Log($"  [{kvp.Key}] {kvp.Value.Type} - Only in {label2}");
                }
            }
            
            Debug.Log("==================================================");
        }
    }
}
