using UnityEngine;
using CommanderChess.Domain;

namespace CommanderChess.GameState
{
    /// <summary>
    /// SelectingDetachTargetState - Trạng thái khi đang chọn vị trí để detach piece
    /// Piece đã được chọn từ info panel, hiển thị possible moves để detach
    /// </summary>
    public class SelectingDetachTargetState : IGameState
    {
        public GameStateData Data { get; }
        public GameStateManager Manager { get; }

        BasePiece carrier;        // Piece đang mang (ví dụ: Tank)
        BasePiece passengerToDetach;  // Piece sẽ detach (ví dụ: Infantry)

        public SelectingDetachTargetState(GameStateData data, GameStateManager manager)
        {
            Data = data;
            Manager = manager;
        }

        public void Enter()
        {
//             Debug.Log($"Entered SelectingDetachTargetState - detaching {Data.SelectedPiece.Type}");

            if (Data.SelectedPiece == null)
            {
                Debug.LogError("SelectingDetachTargetState entered with null SelectedPiece!");
                Manager.ChangeState(GameState.Idle);
                return;
            }

            // Piece được chọn là passenger cần detach
            passengerToDetach = Data.SelectedPiece;
            
            // Find carrier
            carrier = Manager.CarryingSystem.GetCarrier(passengerToDetach);
            if (carrier == null)
            {
                Debug.LogError($"Cannot find carrier for {passengerToDetach.Type}");
                Manager.ChangeState(GameState.Idle);
                return;
            }

//             Debug.Log($"Detaching {passengerToDetach.Type} from {carrier.Type}");

            // Calculate possible detach positions using passenger's possible moves
            Data.HighlightedMoves.Clear();

            // Recalculate cache to get fresh possible moves
            passengerToDetach.RecalculateCache();

            // Use the passenger's possible moves as valid detach positions
            if (passengerToDetach.PossibleMoves != null && passengerToDetach.PossibleMoves.Count > 0)
            {
                Data.HighlightedMoves.AddRange(passengerToDetach.PossibleMoves);
//                 Debug.Log($"Found {Data.HighlightedMoves.Count} possible moves for {passengerToDetach.Type}");
            }

            if (Data.HighlightedMoves.Count == 0)
            {
                Debug.LogWarning("No valid detach positions available!");
                Manager.ChangeState(GameState.Idle);
                return;
            }

            // Highlight valid detach positions
            Manager.HighlightMoves(Data.HighlightedMoves);
            Manager.HighlightSelected(Data.SelectedPosition);

//             Debug.Log($"Highlighted {Data.HighlightedMoves.Count} valid detach positions for {passengerToDetach.Type}");
        }

        public void Exit()
        {
//             Debug.Log("Exited SelectingDetachTargetState");
            Manager.ClearHighlights();
        }

        public void HandleBoardClick(BoardCoord coord)
        {
            // Check if clicked position is valid for detach
            if (!Data.HighlightedMoves.Contains(coord))
            {
                Debug.LogWarning($"Invalid detach position: {coord.ToLabel()}");
                return;
            }

            // Execute detach
            TryExecuteDetach(coord);
        }

        public void HandlePieceClick(BasePiece piece)
        {
            // Clicking on carrier piece should cancel detach mode
            if (piece == carrier)
            {
//                 Debug.Log("Clicked carrier - canceling detach");
                SelectCarrier();
                return;
            }

            // Clicking on the same passenger should deselect and go back to carrier
            if (piece == passengerToDetach)
            {
//                 Debug.Log("Clicked same passenger - deselecting");
                SelectCarrier();
                return;
            }

            // Clicking on other pieces is ignored
            Debug.LogWarning($"Cannot select {piece.Type} while in detach mode");
        }

        public void HandleCancel()
        {
//             Debug.Log("Cancel in SelectingDetachTargetState - back to carrier");
            SelectCarrier();
        }

        public void Update()
        {
            // No update logic needed
        }

        #region Private Helper Methods

        private void TryExecuteDetach(BoardCoord to)
        {
            var from = carrier.Position;

//             Debug.Log($"Attempting detach: {passengerToDetach.Type} from {carrier.Type} at {from.ToLabel()} to {to.ToLabel()}");

            // Validate
            var validation = Manager.ActionValidator.ValidateDetach(carrier, passengerToDetach, to);
            if (!validation.IsValid)
            {
                Debug.LogWarning($"Detach validation failed: {validation.Reason}");
                return;
            }

            // Create and execute command
            var command = Manager.CommandManager.CreateDetachCommand(from, to);
            if (Manager.CommandManager.Execute(command))
            {
//                 Debug.Log($"Detach executed successfully");
                
                // After detach, auto-select carrier for next action
                Manager.ChangeState(GameState.ExecutingAction);
            }
            else
            {
                Debug.LogError($"Detach execution failed");
                Manager.ChangeState(GameState.Idle);
            }
        }

        private void SelectCarrier()
        {
            // Select carrier and go back to PieceSelected state
            Data.SelectedPiece = carrier;
            Data.SelectedPosition = carrier.Position;
            Manager.ChangeState(GameState.PieceSelected);
        }

        #endregion
    }
}
