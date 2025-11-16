using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using CommanderChess.Domain;
using CommanderChess.GameState;

namespace CommanderChess.Presentation
{
    /// <summary>
    /// InputHandler - Xử lý input từ người chơi và forward đến GameStateManager
    /// </summary>
    public class InputHandler : MonoBehaviour
    {
        [SerializeField] LayerMask pieceLayer;

        [Inject] Board board;
        [Inject] GameStateManager gameStateManager;

        Vector2 screenPosition;
        Mouse mouse;
        Keyboard keyboard;

        void Awake()
        {
            mouse = Mouse.current;
            keyboard = Keyboard.current;
        }

        void Update()
        {
            if (mouse == null || keyboard == null) return;

            HandleMouseInput();
            HandleKeyboardInput();
        }

        #region Mouse Input

        void HandleMouseInput()
        {
            screenPosition = mouse.position.ReadValue();

            // Left click - Select piece or execute action
            if (mouse.leftButton.wasPressedThisFrame)
            {
                HandleLeftClick();
            }

            // Right click - Cancel action
            if (mouse.rightButton.wasPressedThisFrame)
            {
                HandleRightClick();
            }
        }

        void HandleLeftClick()
        {
            // Convert screen position to board coordinate
            var boardCoord = ScreenToBoard(screenPosition);

            if (boardCoord.HasValue)
            {
//                 Debug.Log($"Left clicked: {boardCoord.Value.ToLabel()}");
                gameStateManager.OnBoardPositionClicked(boardCoord.Value);
            }
            else
            {
//                 Debug.Log("Left clicked outside board");
            }
        }

        void HandleRightClick()
        {
//             Debug.Log("Right click - canceling action");
            gameStateManager.CancelCurrentAction();
        }

        #endregion

        #region Keyboard Input

        void HandleKeyboardInput()
        {
            // ESC - Cancel action
            if (keyboard.escapeKey.wasPressedThisFrame)
            {
//                 Debug.Log("ESC pressed - canceling action");
                gameStateManager.CancelCurrentAction();
            }

            // Ctrl+Z - Undo
            if ((keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed) &&
                keyboard.zKey.wasPressedThisFrame)
            {
//                 Debug.Log("Ctrl+Z pressed - undo");
                gameStateManager.UndoLastMove();
            }
        }

        #endregion

        #region Conversion Helpers

        /// <summary>
        /// Convert screen position to board coordinate
        /// Returns null if outside board
        /// </summary>
        BoardCoord? ScreenToBoard(Vector2 screenPos)
        {
            if (Camera.main == null)
            {
                Debug.LogError("Main camera not found!");
                return null;
            }

            // Convert screen to world position
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);

            // Convert world to board coordinate
            if (board.TryWorldToBoardCoord(worldPos, out BoardCoord coord))
            {
                return coord;
            }

            return null;
        }

        #endregion

        #region Debug (Optional)

        void OnDrawGizmos()
        {
            if (mouse == null || Camera.main == null || board == null) return;

            // Draw a small sphere at mouse world position for debugging
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mouse.position.ReadValue());
            worldPos.z = 0;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(worldPos, 0.2f);

            // If mouse is over valid board position, draw larger sphere
            if (board.TryWorldToBoardCoord(worldPos, out BoardCoord coord))
            {
                Vector3 cellCenter = board.BoardCoordToWorld(coord);
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(cellCenter, 0.3f);
            }
        }

        #endregion
    }
}
