using System;
using System.Collections.Generic;
using CommanderChess.Board;
using CommanderChess.Commands;
using CommanderChess.Core;
using CommanderChess.Pieces;
using UnityEngine;

namespace CommanderChess.Managers
{
    /// <summary>
    /// Main game manager that handles game flow and state
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game Settings")]
        [SerializeField] private bool _useCommanderMode = true;
        [SerializeField] private float _turnTimeLimit = 0f; // 0 = no limit

        // Core game components
        private ChessBoard _board;
        private MoveValidator _moveValidator;
        private CommanderAbilityHandler _abilityHandler;

        // Game state
        private GameState _currentState;
        private PlayerSide _currentPlayer;
        private ChessPiece _selectedPiece;
        private List<Move> _availableMoves;
        private int _turnNumber;
        private float _turnTimer;

        // Events
        public event Action<GameState> OnGameStateChanged;
        public event Action<PlayerSide> OnTurnChanged;
        public event Action<ChessPiece> OnPieceSelected;
        public event Action OnPieceDeselected;
        public event Action<Move> OnMoveMade;
        public event Action<ChessPiece> OnPieceCaptured;
        public event Action<Commander, CommanderAbility> OnAbilityUsed;

        // Properties
        public GameState CurrentState => _currentState;
        public PlayerSide CurrentPlayer => _currentPlayer;
        public ChessPiece SelectedPiece => _selectedPiece;
        public List<Move> AvailableMoves => _availableMoves;
        public int TurnNumber => _turnNumber;
        public ChessBoard Board => _board;
        public bool IsCommanderMode => _useCommanderMode;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Initialize()
        {
            _board = new ChessBoard();
            _moveValidator = new MoveValidator(_board);
            _abilityHandler = new CommanderAbilityHandler(_board);
            _availableMoves = new List<Move>();
            _currentState = GameState.NotStarted;
        }

        private void Update()
        {
            if (_currentState == GameState.Playing && _turnTimeLimit > 0)
            {
                _turnTimer -= Time.deltaTime;
                if (_turnTimer <= 0)
                {
                    // Time out - could auto-pass or forfeit
                    OnTimeOut();
                }
            }
        }

        /// <summary>
        /// Start a new game
        /// </summary>
        public void StartNewGame()
        {
            if (_useCommanderMode)
            {
                _board.SetupCommanderGame();
            }
            else
            {
                _board.SetupStandardGame();
            }

            _currentPlayer = PlayerSide.White;
            _turnNumber = 1;
            _selectedPiece = null;
            _availableMoves.Clear();
            _turnTimer = _turnTimeLimit;

            SetGameState(GameState.Playing);
            OnTurnChanged?.Invoke(_currentPlayer);

            Debug.Log($"Game started in {(_useCommanderMode ? "Commander" : "Standard")} mode");
        }

        /// <summary>
        /// Select a piece to see its available moves
        /// </summary>
        public bool SelectPiece(BoardPosition position)
        {
            if (_currentState != GameState.Playing && _currentState != GameState.Check)
                return false;

            var piece = _board.GetPieceAt(position);
            
            if (piece == null || piece.Side != _currentPlayer)
            {
                DeselectPiece();
                return false;
            }

            _selectedPiece = piece;
            _availableMoves = _moveValidator.GetLegalMoves(piece);

            OnPieceSelected?.Invoke(piece);
            Debug.Log($"Selected {piece} with {_availableMoves.Count} available moves");

            return true;
        }

        /// <summary>
        /// Deselect the currently selected piece
        /// </summary>
        public void DeselectPiece()
        {
            _selectedPiece = null;
            _availableMoves.Clear();
            OnPieceDeselected?.Invoke();
        }

        /// <summary>
        /// Attempt to move the selected piece to a position
        /// </summary>
        public bool TryMakeMove(BoardPosition target)
        {
            if (_selectedPiece == null)
                return false;

            // Find the matching move
            Move move = null;
            foreach (var m in _availableMoves)
            {
                if (m.To == target)
                {
                    move = m;
                    break;
                }
            }

            if (move == null)
                return false;

            return ExecuteMove(move);
        }

        /// <summary>
        /// Execute a specific move
        /// </summary>
        public bool ExecuteMove(Move move)
        {
            if (!_moveValidator.IsLegalMove(move))
                return false;

            // Check for capture
            var capturedPiece = _board.GetPieceAt(move.To);
            if (capturedPiece != null)
            {
                OnPieceCaptured?.Invoke(capturedPiece);
            }

            // Execute the move
            _board.ExecuteMove(move);

            OnMoveMade?.Invoke(move);
            Debug.Log($"Move executed: {move}");

            // Check game ending conditions
            CheckGameState();

            // If game is still ongoing, switch turns
            if (_currentState == GameState.Playing || _currentState == GameState.Check)
            {
                EndTurn();
            }

            return true;
        }

        /// <summary>
        /// Use a commander ability
        /// </summary>
        public bool UseAbility(CommanderAbility ability, BoardPosition? target = null)
        {
            if (!_useCommanderMode)
                return false;

            var commander = _board.GetCommander(_currentPlayer);
            if (commander == null || commander.IsCaptured)
                return false;

            bool success = _abilityHandler.ExecuteAbility(commander, ability, target);

            if (success)
            {
                OnAbilityUsed?.Invoke(commander, ability);
                Debug.Log($"{_currentPlayer} Commander used {ability}");
            }

            return success;
        }

        /// <summary>
        /// Get available abilities for current player's commander
        /// </summary>
        public List<CommanderAbility> GetAvailableAbilities()
        {
            if (!_useCommanderMode)
                return new List<CommanderAbility>();

            var commander = _board.GetCommander(_currentPlayer);
            return _abilityHandler.GetAvailableAbilities(commander);
        }

        private void CheckGameState()
        {
            var opponent = _currentPlayer == PlayerSide.White ? PlayerSide.Black : PlayerSide.White;

            if (_moveValidator.IsCheckmate(opponent))
            {
                var winState = _currentPlayer == PlayerSide.White ? GameState.WhiteWins : GameState.BlackWins;
                SetGameState(winState);
                Debug.Log($"Checkmate! {_currentPlayer} wins!");
            }
            else if (_moveValidator.IsStalemate(opponent))
            {
                SetGameState(GameState.Stalemate);
                Debug.Log("Stalemate!");
            }
            else if (_moveValidator.IsKingInCheck(opponent))
            {
                SetGameState(GameState.Check);
                Debug.Log($"{opponent} is in check!");
            }
            else
            {
                SetGameState(GameState.Playing);
            }
        }

        private void EndTurn()
        {
            DeselectPiece();
            
            _currentPlayer = _currentPlayer == PlayerSide.White ? PlayerSide.Black : PlayerSide.White;
            
            if (_currentPlayer == PlayerSide.White)
            {
                _turnNumber++;
            }

            _turnTimer = _turnTimeLimit;

            // Reset ability effects and update cooldowns
            _abilityHandler.OnTurnStart(_currentPlayer);

            OnTurnChanged?.Invoke(_currentPlayer);
            Debug.Log($"Turn {_turnNumber}: {_currentPlayer}'s turn");
        }

        private void SetGameState(GameState newState)
        {
            if (_currentState != newState)
            {
                _currentState = newState;
                OnGameStateChanged?.Invoke(newState);
            }
        }

        private void OnTimeOut()
        {
            Debug.Log($"{_currentPlayer} ran out of time!");
            // Could implement forfeit or auto-pass here
        }

        /// <summary>
        /// Resign the game
        /// </summary>
        public void Resign()
        {
            var winState = _currentPlayer == PlayerSide.White ? GameState.BlackWins : GameState.WhiteWins;
            SetGameState(winState);
            Debug.Log($"{_currentPlayer} resigned!");
        }

        /// <summary>
        /// Offer a draw
        /// </summary>
        public void OfferDraw()
        {
            // In a real game, this would need to be accepted by the opponent
            Debug.Log($"{_currentPlayer} offers a draw");
        }

        /// <summary>
        /// Accept a draw offer
        /// </summary>
        public void AcceptDraw()
        {
            SetGameState(GameState.Draw);
            Debug.Log("Draw accepted!");
        }

        /// <summary>
        /// Get all pieces on the board
        /// </summary>
        public List<ChessPiece> GetAllPieces()
        {
            var pieces = new List<ChessPiece>();
            pieces.AddRange(_board.GetPieces(PlayerSide.White));
            pieces.AddRange(_board.GetPieces(PlayerSide.Black));
            return pieces;
        }

        /// <summary>
        /// Get piece at a specific position
        /// </summary>
        public ChessPiece GetPieceAt(BoardPosition position)
        {
            return _board.GetPieceAt(position);
        }

        /// <summary>
        /// Check if a position has a legal move available
        /// </summary>
        public bool IsLegalMoveTarget(BoardPosition position)
        {
            foreach (var move in _availableMoves)
            {
                if (move.To == position)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Get the remaining time for the current turn
        /// </summary>
        public float GetRemainingTurnTime()
        {
            return _turnTimeLimit > 0 ? _turnTimer : float.MaxValue;
        }

        /// <summary>
        /// Set game mode (standard or commander)
        /// </summary>
        public void SetCommanderMode(bool enabled)
        {
            if (_currentState == GameState.NotStarted)
            {
                _useCommanderMode = enabled;
            }
        }

        /// <summary>
        /// Set turn time limit
        /// </summary>
        public void SetTurnTimeLimit(float seconds)
        {
            _turnTimeLimit = seconds;
        }
    }
}
