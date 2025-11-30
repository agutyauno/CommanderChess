using UnityEngine;
using CommanderChess.Domain;

namespace CommanderChess.GameState
{
    /// <summary>
    /// AirforceBombingDecisionState - Trạng thái chờ Airforce chọn Stay hoặc Return sau bombing
    /// Block tất cả input khác, chỉ cho phép Stay/Return
    /// </summary>
    public class AirforceBombingDecisionState : IGameState
    {
        public GameStateData Data { get; }
        public GameStateManager Manager { get; }

        public AirforceBombingDecisionState(GameStateData data, GameStateManager manager)
        {
            Data = data;
            Manager = manager;
        }

        public void Enter()
        {
            var carrier = Manager.Data.BombingCarrier;
            var carrierInfo = carrier != null ? $" (carried by {carrier.Type})" : "";
            Debug.Log($"Entered AirforceBombingDecisionState - Airforce at {Data.BombingAirforce.Position.ToLabel()}, original position {Data.BombingOriginalPosition.ToLabel()}{carrierInfo}");

            // Verify data is valid
            if (Data.BombingAirforce == null || Data.BombingAirforce.Type != BasePiece.PieceType.AirForce)
            {
                Debug.LogError("Invalid bombing data - returning to Idle");
                Manager.ChangeState(GameState.Idle);
                return;
            }

            // Highlight current position and original position for visual feedback
            Data.HighlightedMoves.Clear();
            Data.HighlightedMoves.Add(Data.BombingAirforce.Position); // Current (captured) position
            Data.HighlightedMoves.Add(Data.BombingOriginalPosition);   // Original position
            
            Manager.HighlightMoves(Data.HighlightedMoves);

            // Notify UI to show Stay/Return buttons
            Manager.EventBus.Publish(new Services.BombingDecisionRequestedEvent(
                Data.BombingAirforce, 
                Data.BombingOriginalPosition
            ));
        }

        public void Exit()
        {
            Debug.Log("Exited AirforceBombingDecisionState");
            Manager.ClearHighlights();
        }

        public void HandleBoardClick(BoardCoord coord)
        {
            // Input blocked - only allow button choices
            Debug.Log("Board click blocked during bombing decision");
        }

        public void HandlePieceClick(BasePiece piece)
        {
            // Input blocked - only allow button choices
            Debug.Log("Piece click blocked during bombing decision");
        }

        public void HandleCancel()
        {
            // Cancel = Return to original position
            Debug.Log("Cancel pressed - Airforce returns to original position");
            ExecuteReturn();
        }

        public void Update()
        {
            // Nothing to update
        }

        /// <summary>
        /// Called when player chooses to Stay at captured position
        /// </summary>
        public void OnStaySelected()
        {
            Debug.Log("Airforce stays at captured position");
            
            // Clear bombing data
            Data.BombingAirforce = null;
            Data.BombingOriginalPosition = default;
            Data.BombingCarrier = null;

            // Return to Idle
            Manager.ChangeState(GameState.Idle);
        }

        /// <summary>
        /// Called when player chooses to Return to original position
        /// </summary>
        public void OnReturnSelected()
        {
            Debug.Log("Airforce returns to original position");
            ExecuteReturn();
        }

        private void ExecuteReturn()
        {
            var airforce = Data.BombingAirforce;
            var carrier = Data.BombingCarrier;
            
            if (airforce == null)
            {
                Debug.LogError("BombingAirforce is null, cannot execute return");
                Manager.ChangeState(GameState.Idle);
                return;
            }
            
            var from = airforce.Position;
            var to = Data.BombingOriginalPosition;

            // If Airforce was carried, restore carried state
            if (carrier != null)
            {
                Debug.Log($"Airforce was carried by {carrier.Type} - restoring carried state");
                
                // Create boarding command to restore carried state
                var boardingCommand = Manager.CommandManager.CreateBoardingCommand(from, to);
                
                if (boardingCommand == null)
                {
                    Debug.LogError("Failed to create boarding command to restore carried state");
                    Manager.ChangeState(GameState.Idle);
                    return;
                }
                
                if (Manager.CommandManager.Execute(boardingCommand))
                {
                    Debug.Log($"Airforce returned and boarded back to {carrier.Type} at {to.ToLabel()}");
                }
                else
                {
                    Debug.LogError("Failed to execute boarding command to restore carried state");
                }
            }
            else
            {
                // Airforce was not carried - just move back
                var moveCommand = Manager.CommandManager.CreateMoveCommand(from, to);
                
                if (moveCommand == null)
                {
                    Debug.LogError("Failed to create move command");
                    Manager.ChangeState(GameState.Idle);
                    return;
                }
                
                if (Manager.CommandManager.Execute(moveCommand))
                {
                    Debug.Log($"Airforce returned from {from.ToLabel()} to {to.ToLabel()}");
                }
                else
                {
                    Debug.LogError("Failed to execute return movement");
                }
            }

            // Clear bombing data
            Data.BombingAirforce = null;
            Data.BombingOriginalPosition = default;
            Data.BombingCarrier = null;

            // Return to Idle
            Manager.ChangeState(GameState.Idle);
        }
    }
}
