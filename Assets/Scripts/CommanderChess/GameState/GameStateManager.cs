using System.Collections.Generic;
using UnityEngine;
using VContainer;
using CommanderChess.Domain;
using CommanderChess.CommandSystem;
using CommanderChess.Services;
using CommanderChess.Presentation;

namespace CommanderChess.GameState
{
    /// <summary>
    /// GameStateManager - Context trong State Pattern
    /// REFACTORED: Sử dụng EventBus thay vì C# events
    /// </summary>
    public class GameStateManager : MonoBehaviour
    {
        #region Dependencies
        [Inject] readonly Board board;
        [Inject] readonly CommandManager commandManager;
        [Inject] readonly TurnManager turnManager;
        [Inject] readonly ActionValidator actionValidator;
        [Inject] readonly CarryingSystem carryingSystem;
        [Inject] readonly BoardHighlighter highlighter;
        [Inject] readonly EventBus eventBus;
        #endregion

        #region State Management
        readonly GameStateData stateData = new GameStateData();
        IGameState currentState;
        readonly Dictionary<GameState, IGameState> states = new Dictionary<GameState, IGameState>();
        #endregion

        #region Properties
        public GameState CurrentState => stateData.CurrentState;
        public BasePiece SelectedPiece => stateData.SelectedPiece;

        // Expose dependencies for states
        public Board Board => board;
        public CommandManager CommandManager => commandManager;
        public TurnManager TurnManager => turnManager;
        public ActionValidator ActionValidator => actionValidator;
        public CarryingSystem CarryingSystem => carryingSystem;
        public EventBus EventBus => eventBus;
        #endregion

        #region Initialization
        void Awake()
        {
            InitializeStates();
        }

        void Start()
        {
            // Start in Idle state
            ChangeState(GameState.Idle);
        }

        void InitializeStates()
        {
            states[GameState.Idle] = new IdleState(stateData, this);
            states[GameState.PieceSelected] = new PieceSelectedState(stateData, this);
            states[GameState.ExecutingAction] = new ExecutingActionState(stateData, this);
            states[GameState.SelectingDetachTarget] = new SelectingDetachTargetState(stateData, this);
            // TODO: Add more states
            // states[GameState.WaitingForOpponent] = new WaitingForOpponentState(stateData, this);
            // states[GameState.GameOver] = new GameOverState(stateData, this);
        }
        #endregion

        #region Public API - Input Handling

        /// <summary>
        /// Được gọi khi người chơi click vào board position
        /// </summary>
        public void OnBoardPositionClicked(BoardCoord coord)
        {
            if (currentState == null)
            {
                Debug.LogError("CurrentState is null!");
                return;
            }

            // Check if there's a piece at this position
            if (board.TryGetPiece(coord, out var piece))
            {
                currentState.HandlePieceClick(piece);
            }
            else
            {
                currentState.HandleBoardClick(coord);
            }
        }

        /// <summary>
        /// Cancel current action
        /// </summary>
        public void CancelCurrentAction()
        {
            if (currentState == null)
            {
                Debug.LogError("CurrentState is null!");
                return;
            }

            currentState.HandleCancel();
        }
        
        /// <summary>
        /// Deselect current piece and return to idle
        /// </summary>
        public void DeselectPiece()
        {
            NotifyPieceDeselected();
            ChangeState(GameState.Idle);
        }

        /// <summary>
        /// Undo current turn - restore to beginning of turn
        /// </summary>
        public void UndoLastMove()
        {
            if (commandManager.CanUndo())
            {
                Debug.Log("Undoing current turn");
                bool success = commandManager.UndoTurn();
                
                if (success)
                {
                    // Return to Idle state after undo
                    ChangeState(GameState.Idle);
                }
            }
            else
            {
                Debug.LogWarning("Cannot undo - no commands in history");
            }
        }

        /// <summary>
        /// Called when user selects a carried piece from info panel to detach
        /// </summary>
        public void SelectPieceForDetach(BasePiece piece)
        {
            if (piece == null)
            {
                Debug.LogError("SelectPieceForDetach: piece is null");
                return;
            }

            // Check if piece is carried
            if (!carryingSystem.IsCarried(piece))
            {
                Debug.LogWarning($"{piece.Type} is not carried - cannot detach");
                return;
            }

            // Set selected piece in state data
            stateData.SelectedPiece = piece;
            stateData.SelectedPosition = piece.Position; // Position of carrier

            // Transition to SelectingDetachTarget state
            ChangeState(GameState.SelectingDetachTarget);
        }

        /// <summary>
        /// Called when user selects a piece (carrier or standalone) from info panel
        /// </summary>
        public void SelectPiece(BasePiece piece)
        {
            if (piece == null)
            {
                Debug.LogError("SelectPiece: piece is null");
                return;
            }

            // Check if piece is allowed to act
            if (!turnManager.IsPieceAllowedToAct(piece))
            {
                Debug.LogWarning($"Piece {piece.Type} is not allowed to act");
                return;
            }

            // Set selected piece
            stateData.SelectedPiece = piece;
            stateData.SelectedPosition = piece.Position;

            // Transition to PieceSelected state
            ChangeState(GameState.PieceSelected);
        }

        #endregion

        #region State Transitions

        /// <summary>
        /// Change to new state
        /// </summary>
        public void ChangeState(GameState newState)
        {
            if (!states.ContainsKey(newState))
            {
                Debug.LogError($"State {newState} not registered!");
                return;
            }

            var previousState = stateData.CurrentState;

            // Exit current state
            currentState?.Exit();

            // Update state
            stateData.CurrentState = newState;
            currentState = states[newState];

            // Enter new state
            currentState.Enter();

            Debug.Log($"State changed: {previousState} -> {newState}");
        }

        #endregion

        #region Visual Feedback Methods (called by States)

        /// <summary>
        /// Highlight valid moves
        /// </summary>
        public void HighlightMoves(List<BoardCoord> moves)
        {
            highlighter?.HighlightMoves(moves);
        }

        /// <summary>
        /// Highlight valid attacks
        /// </summary>
        public void HighlightAttacks(List<BoardCoord> attacks)
        {
            highlighter?.HighlightAttacks(attacks);
        }

        /// <summary>
        /// Highlight selected piece position
        /// </summary>
        public void HighlightSelected(BoardCoord position)
        {
            highlighter?.HighlightSelected(position);
        }

        /// <summary>
        /// Clear all highlights
        /// </summary>
        public void ClearHighlights()
        {
            highlighter?.ClearAll();
        }

        #endregion

        #region Event Publishing Methods (called by States)

        /// <summary>
        /// Notify that a piece was selected
        /// </summary>
        public void NotifyPieceSelected(BasePiece piece)
        {
            eventBus.Publish(new PieceSelectedEvent(piece));
        }

        /// <summary>
        /// Notify that piece was deselected
        /// </summary>
        public void NotifyPieceDeselected()
        {
            eventBus.Publish(new PieceDeselectedEvent());
        }

        #endregion

        #region Unity Lifecycle
        void Update()
        {
            currentState?.Update();
        }
        #endregion
    }
}