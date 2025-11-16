using UnityEngine;
using CommanderChess.Domain;

namespace CommanderChess.GameState
{
    /// <summary>
    /// ExecutingActionState - Trạng thái khi đang thực thi action
    /// Block input và chờ animation/command hoàn thành
    /// </summary>
    public class ExecutingActionState : IGameState
    {
        public GameStateData Data { get; }
        public GameStateManager Manager { get; }

        bool isExecuting = false;

        public ExecutingActionState(GameStateData data, GameStateManager manager)
        {
            Data = data;
            Manager = manager;
        }

        public void Enter()
        {
            Debug.Log("Entered ExecutingActionState - blocking input");
            isExecuting = true;

            // TODO: Nếu có animation system, chờ animation complete
            // Hiện tại chỉ transition ngay về Idle

            // Simulate instant execution
            OnCommandCompleted();
        }

        public void Exit()
        {
            Debug.Log("Exited ExecutingActionState");
            isExecuting = false;
        }

        public void HandleBoardClick(BoardCoord coord)
        {
            // Input blocked during execution
            Debug.Log("Input blocked - command executing");
        }

        public void HandlePieceClick(BasePiece piece)
        {
            // Input blocked during execution
            Debug.Log("Input blocked - command executing");
        }

        public void HandleCancel()
        {
            // Cannot cancel during execution
            Debug.Log("Cannot cancel - command executing");
        }

        public void Update()
        {
            // Check if execution is complete
            // Trong thực tế, sẽ check animation state hoặc async operation

            if (!isExecuting)
            {
                OnCommandCompleted();
            }
        }

        private void OnCommandCompleted()
        {
            Debug.Log("Command execution completed");

            // Check if last command was detach - if so, auto-select carrier
            var lastCommand = Manager.CommandManager.GetLastCommand();
            if (lastCommand != null && lastCommand.GetType().Name.Contains("DetachCommand"))
            {
                Debug.Log("Detach command completed - checking for carrier auto-select");
                
                // After detach, TurnManager sets allowedPieceAfterDetach
                // We need to find the carrier and auto-select it
                if (Manager.TurnManager.IsDetachActive)
                {
                    // Find carrier piece at the selected position
                    var carrierPos = Data.SelectedPosition;
                    if (Manager.Board.TryGetPiece(carrierPos, out var carrier))
                    {
                        Debug.Log($"Auto-selecting carrier {carrier.Type} after detach");
                        
                        // Set selection
                        Data.SelectedPiece = carrier;
                        Data.SelectedPosition = carrierPos;
                        
                        // Go to PieceSelected state
                        Manager.ChangeState(GameState.PieceSelected);
                        return;
                    }
                }
            }

            // Clear selection
            Data.Clear();

            // TODO: Check win condition
            // if (Manager.CheckWinCondition()) 
            // {
            //     Manager.ChangeState(GameState.GameOver);
            //     return;
            // }

            // DON'T auto end turn here - TurnManager handles that via ConfirmEndTurn()
            // Just return to Idle and wait for player confirmation
            Manager.ChangeState(GameState.Idle);
        }
    }
}