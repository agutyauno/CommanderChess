using CommanderChess.Core;
using CommanderChess.Managers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CommanderChess.UI
{
    /// <summary>
    /// Handles player input for selecting and moving pieces
    /// </summary>
    public class InputHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BoardRenderer _boardRenderer;
        [SerializeField] private Camera _gameCamera;

        [Header("Settings")]
        [SerializeField] private LayerMask _boardLayerMask = -1;

        private bool _isDragging;
        private Vector3 _dragStartPosition;

        private void Awake()
        {
            if (_gameCamera == null)
            {
                _gameCamera = Camera.main;
            }
        }

        private void Update()
        {
            // Don't process input if over UI
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            // Don't process if game is not active
            if (GameManager.Instance == null)
                return;

            var state = GameManager.Instance.CurrentState;
            if (state != GameState.Playing && state != GameState.Check)
                return;

            HandleMouseInput();
            HandleKeyboardInput();
        }

        private void HandleMouseInput()
        {
            // Left click - select or move
            if (Input.GetMouseButtonDown(0))
            {
                OnLeftClick();
            }

            // Right click - deselect
            if (Input.GetMouseButtonDown(1))
            {
                GameManager.Instance.DeselectPiece();
            }
        }

        private void HandleKeyboardInput()
        {
            // Number keys for abilities (when commander mode is active)
            if (GameManager.Instance.IsCommanderMode)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                    TryUseAbility(CommanderAbility.Rally);
                if (Input.GetKeyDown(KeyCode.Alpha2))
                    TryUseAbility(CommanderAbility.Charge);
                if (Input.GetKeyDown(KeyCode.Alpha3))
                    TryUseAbility(CommanderAbility.Shield);
                if (Input.GetKeyDown(KeyCode.Alpha4))
                    TryUseAbility(CommanderAbility.Tactics);
                if (Input.GetKeyDown(KeyCode.Alpha5))
                    TryUseAbility(CommanderAbility.Inspire);
            }
        }

        private void OnLeftClick()
        {
            BoardPosition clickedPos = GetClickedBoardPosition();

            if (!clickedPos.IsValid)
                return;

            // If we have a piece selected and clicked on a valid move target, make the move
            if (GameManager.Instance.SelectedPiece != null)
            {
                if (GameManager.Instance.IsLegalMoveTarget(clickedPos))
                {
                    GameManager.Instance.TryMakeMove(clickedPos);
                    return;
                }
            }

            // Try to select a piece
            GameManager.Instance.SelectPiece(clickedPos);
        }

        private BoardPosition GetClickedBoardPosition()
        {
            if (_gameCamera == null)
                return BoardPosition.Invalid;

            Ray ray = _gameCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 100f, _boardLayerMask))
            {
                if (_boardRenderer != null)
                {
                    return _boardRenderer.GetBoardPosition(hit.point);
                }
                else
                {
                    // Fallback: assume board is at y=0 with 1 unit squares
                    int col = Mathf.FloorToInt(hit.point.x);
                    int row = Mathf.FloorToInt(hit.point.z);
                    return new BoardPosition(row, col);
                }
            }

            // Raycast to ground plane as fallback
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            if (groundPlane.Raycast(ray, out float distance))
            {
                Vector3 point = ray.GetPoint(distance);
                if (_boardRenderer != null)
                {
                    return _boardRenderer.GetBoardPosition(point);
                }
                else
                {
                    int col = Mathf.FloorToInt(point.x);
                    int row = Mathf.FloorToInt(point.z);
                    return new BoardPosition(row, col);
                }
            }

            return BoardPosition.Invalid;
        }

        private void TryUseAbility(CommanderAbility ability)
        {
            if (GameManager.Instance == null)
                return;

            var availableAbilities = GameManager.Instance.GetAvailableAbilities();
            
            if (!availableAbilities.Contains(ability))
            {
                Debug.Log($"Ability {ability} is not available");
                return;
            }

            // For targeted abilities, we need a target position
            BoardPosition? target = null;
            if (ability == CommanderAbility.Charge || 
                ability == CommanderAbility.Tactics || 
                ability == CommanderAbility.Inspire)
            {
                target = GetClickedBoardPosition();
                if (!target.Value.IsValid)
                {
                    Debug.Log($"Ability {ability} needs a valid target");
                    return;
                }
            }

            GameManager.Instance.UseAbility(ability, target);
        }

        /// <summary>
        /// Get the current mouse position on the board
        /// </summary>
        public BoardPosition GetCurrentHoverPosition()
        {
            return GetClickedBoardPosition();
        }
    }
}
