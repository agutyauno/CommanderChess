using UnityEngine;
using VContainer;

namespace CommanderChess.Testing
{
    /// <summary>
    /// Test script để verify undo functionality
    /// Attach vào GameObject trong scene để test
    /// </summary>
    public class UndoTester : MonoBehaviour
    {
        [Inject] TurnManager turnManager;
        [Inject] CommanderChess.Domain.Board board;
        [Inject] Services.CarryingSystem carryingSystem;

        void Update()
        {
            // Press U to undo
            if (Input.GetKeyDown(KeyCode.U))
            {
                TestUndo();
            }

            // Press L to log current state
            if (Input.GetKeyDown(KeyCode.L))
            {
                LogCurrentState();
            }

            // Press V to validate
            if (Input.GetKeyDown(KeyCode.V))
            {
                ValidateState();
            }
            
            // Press D to dump board dictionary
            if (Input.GetKeyDown(KeyCode.D))
            {
                DumpBoardDictionary();
            }
        }

        void DumpBoardDictionary()
        {
            Debug.Log("\n========== BOARD DICTIONARY DUMP ==========");
            Debug.Log($"Total entries: {board.Pieces.Count}");
            
            foreach (var kvp in board.Pieces)
            {
                var pos = kvp.Key;
                var piece = kvp.Value;
                Debug.Log($"  [{pos}] = {piece.Type} (piece.Position = {piece.Position})");
                
                if (piece.Position != pos)
                {
                    Debug.LogError($"    ❌ MISMATCH! Dictionary key {pos} != piece.Position {piece.Position}");
                }
            }
            
            Debug.Log("==========================================\n");
        }

        void TestUndo()
        {
            Debug.Log("\n========== TESTING UNDO ==========");
            
            Debug.Log($"Before Undo: Board has {board.Pieces.Count} pieces");
            LogCurrentState();
            
            bool success = turnManager.UndoTurn();
            
            if (success)
            {
                Debug.Log($"After Undo: Board has {board.Pieces.Count} pieces");
                LogCurrentState();
                ValidateState();
            }
            else
            {
                Debug.LogError("Undo failed!");
            }
        }

        void LogCurrentState()
        {
            Utilities.UndoDebugHelper.LogBoardState(board, "[Test] ");
            Utilities.UndoDebugHelper.LogCarryingState(board, carryingSystem, "[Test] ");
        }

        void ValidateState()
        {
            Utilities.UndoDebugHelper.ValidateAfterUndo(board, carryingSystem, $"Turn {turnManager.TurnNumber}");
        }
    }
}
