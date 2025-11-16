using UnityEngine;
using CommanderChess.Domain;

namespace CommanderChess.GameState
{
    /// <summary>
    /// IdleState - Trạng thái chờ người chơi chọn quân
    /// </summary>
    public class IdleState : IGameState
    {
        public GameStateData Data { get; }
        public GameStateManager Manager { get; }

        public IdleState(GameStateData data, GameStateManager manager)
        {
            Data = data;
            Manager = manager;
        }

        public void Enter()
        {
//             Debug.Log("Entered IdleState - Waiting for piece selection");

            // Clear any previous selection data
            Data.Clear();

            // Clear visual highlights
            Manager.ClearHighlights();
            
            // Notify deselection
            Manager.NotifyPieceDeselected();
        }

    public void Exit()
    {
    }    public void HandleBoardClick(BoardCoord coord)
    {
        // Clicking on empty board in Idle state does nothing
    }        public void HandlePieceClick(BasePiece piece)
        {
            // Kiểm tra xem piece có được phép hành động không (respects detach logic and end-condition)
            if (!Manager.TurnManager.IsPieceAllowedToAct(piece))
            {
                if (Manager.TurnManager.IsDetachActive)
                {
                    Debug.LogWarning($"Cannot select {piece.Type} - only the carrier can act after detach");
                }
                else if (Manager.TurnManager.IsEndConditionPending)
                {
                    Debug.LogWarning($"Cannot select {piece.Type} - turn end pending, please confirm or cancel");
                }
                else
                {
                    Debug.LogWarning($"Cannot select {piece.Team} {piece.Type} - not your turn");
                }
                return;
            }

            // Kiểm tra xem piece có đang được mang không (nếu đang được mang thì không thể chọn)
            if (Manager.CarryingSystem.IsCarried(piece))
            {
                Debug.LogWarning($"{piece.Type} is being carried - cannot select");
                return;
            }

            // Select piece và chuyển sang PieceSelectedState

            Data.SelectedPiece = piece;
            Data.SelectedPosition = piece.Position;

            Manager.ChangeState(GameState.PieceSelected);
        }

    public void HandleCancel()
    {
        // Nothing to cancel in Idle state
    }        public void Update()
        {
            // Idle state không cần update logic
        }
    }
}
