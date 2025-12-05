using CommanderChess.Core;
using CommanderChess.Managers;
using CommanderChess.Pieces;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CommanderChess.UI
{
    /// <summary>
    /// Main UI controller for the game
    /// </summary>
    public class GameUIController : MonoBehaviour
    {
        [Header("UI Panels")]
        [SerializeField] private GameObject _mainMenuPanel;
        [SerializeField] private GameObject _gamePanel;
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private GameObject _pausePanel;

        [Header("Game UI Elements")]
        [SerializeField] private TextMeshProUGUI _turnText;
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private Button _resignButton;
        [SerializeField] private Button _drawButton;
        [SerializeField] private Button _pauseButton;

        [Header("Main Menu Elements")]
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _commanderModeButton;
        [SerializeField] private Button _standardModeButton;
        [SerializeField] private Toggle _commanderModeToggle;

        [Header("Game Over Elements")]
        [SerializeField] private TextMeshProUGUI _gameOverText;
        [SerializeField] private Button _playAgainButton;
        [SerializeField] private Button _mainMenuButton;

        private bool _isPaused;

        private void Start()
        {
            SetupButtonListeners();
            SubscribeToGameEvents();
            ShowMainMenu();
        }

        private void OnDestroy()
        {
            UnsubscribeFromGameEvents();
        }

        private void Update()
        {
            if (GameManager.Instance != null && 
                (GameManager.Instance.CurrentState == GameState.Playing ||
                 GameManager.Instance.CurrentState == GameState.Check))
            {
                UpdateTimerDisplay();
            }

            // Handle pause input
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }

        private void SetupButtonListeners()
        {
            // Main menu buttons
            if (_newGameButton != null)
                _newGameButton.onClick.AddListener(OnNewGameClicked);
            
            if (_commanderModeButton != null)
                _commanderModeButton.onClick.AddListener(() => StartGame(true));
            
            if (_standardModeButton != null)
                _standardModeButton.onClick.AddListener(() => StartGame(false));

            // Game buttons
            if (_resignButton != null)
                _resignButton.onClick.AddListener(OnResignClicked);
            
            if (_drawButton != null)
                _drawButton.onClick.AddListener(OnDrawClicked);
            
            if (_pauseButton != null)
                _pauseButton.onClick.AddListener(TogglePause);

            // Game over buttons
            if (_playAgainButton != null)
                _playAgainButton.onClick.AddListener(OnPlayAgainClicked);
            
            if (_mainMenuButton != null)
                _mainMenuButton.onClick.AddListener(ShowMainMenu);
        }

        private void SubscribeToGameEvents()
        {
            if (GameManager.Instance == null)
                return;

            GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
            GameManager.Instance.OnTurnChanged += OnTurnChanged;
            GameManager.Instance.OnPieceSelected += OnPieceSelected;
            GameManager.Instance.OnPieceDeselected += OnPieceDeselected;
            GameManager.Instance.OnMoveMade += OnMoveMade;
            GameManager.Instance.OnPieceCaptured += OnPieceCaptured;
        }

        private void UnsubscribeFromGameEvents()
        {
            if (GameManager.Instance == null)
                return;

            GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            GameManager.Instance.OnTurnChanged -= OnTurnChanged;
            GameManager.Instance.OnPieceSelected -= OnPieceSelected;
            GameManager.Instance.OnPieceDeselected -= OnPieceDeselected;
            GameManager.Instance.OnMoveMade -= OnMoveMade;
            GameManager.Instance.OnPieceCaptured -= OnPieceCaptured;
        }

        #region Panel Management

        public void ShowMainMenu()
        {
            SetPanelActive(_mainMenuPanel, true);
            SetPanelActive(_gamePanel, false);
            SetPanelActive(_gameOverPanel, false);
            SetPanelActive(_pausePanel, false);
            _isPaused = false;
        }

        private void ShowGamePanel()
        {
            SetPanelActive(_mainMenuPanel, false);
            SetPanelActive(_gamePanel, true);
            SetPanelActive(_gameOverPanel, false);
            SetPanelActive(_pausePanel, false);
        }

        private void ShowGameOver(string message)
        {
            if (_gameOverText != null)
                _gameOverText.text = message;

            SetPanelActive(_gameOverPanel, true);
            SetPanelActive(_pausePanel, false);
        }

        private void TogglePause()
        {
            if (GameManager.Instance == null)
                return;

            if (GameManager.Instance.CurrentState != GameState.Playing &&
                GameManager.Instance.CurrentState != GameState.Check)
                return;

            _isPaused = !_isPaused;
            SetPanelActive(_pausePanel, _isPaused);
            Time.timeScale = _isPaused ? 0f : 1f;
        }

        private void SetPanelActive(GameObject panel, bool active)
        {
            if (panel != null)
                panel.SetActive(active);
        }

        #endregion

        #region Button Handlers

        private void OnNewGameClicked()
        {
            bool useCommander = _commanderModeToggle != null && _commanderModeToggle.isOn;
            StartGame(useCommander);
        }

        private void StartGame(bool commanderMode)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetCommanderMode(commanderMode);
                GameManager.Instance.StartNewGame();
                ShowGamePanel();
            }
        }

        private void OnResignClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.Resign();
            }
        }

        private void OnDrawClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OfferDraw();
            }
        }

        private void OnPlayAgainClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartNewGame();
                SetPanelActive(_gameOverPanel, false);
            }
        }

        #endregion

        #region Game Event Handlers

        private void OnGameStateChanged(GameState newState)
        {
            UpdateStatusText(newState);

            switch (newState)
            {
                case GameState.WhiteWins:
                    ShowGameOver("White Wins!");
                    break;
                case GameState.BlackWins:
                    ShowGameOver("Black Wins!");
                    break;
                case GameState.Stalemate:
                    ShowGameOver("Stalemate!");
                    break;
                case GameState.Draw:
                    ShowGameOver("Draw!");
                    break;
            }
        }

        private void OnTurnChanged(PlayerSide player)
        {
            if (_turnText != null)
            {
                _turnText.text = $"{player}'s Turn";
                _turnText.color = player == PlayerSide.White ? Color.white : Color.black;
            }
        }

        private void OnPieceSelected(ChessPiece piece)
        {
            // Could show piece info or highlight
        }

        private void OnPieceDeselected()
        {
            // Could clear piece info display
        }

        private void OnMoveMade(Move move)
        {
            // Could play sound or animation
        }

        private void OnPieceCaptured(ChessPiece piece)
        {
            // Could update captured pieces display
        }

        #endregion

        #region UI Updates

        private void UpdateStatusText(GameState state)
        {
            if (_statusText == null)
                return;

            _statusText.text = state switch
            {
                GameState.NotStarted => "Press Start to begin",
                GameState.Playing => "",
                GameState.Check => "Check!",
                GameState.Checkmate => "Checkmate!",
                GameState.Stalemate => "Stalemate!",
                GameState.Draw => "Draw!",
                GameState.WhiteWins => "White Wins!",
                GameState.BlackWins => "Black Wins!",
                _ => ""
            };
        }

        private void UpdateTimerDisplay()
        {
            if (_timerText == null || GameManager.Instance == null)
                return;

            float remaining = GameManager.Instance.GetRemainingTurnTime();
            
            if (remaining < float.MaxValue)
            {
                int minutes = (int)(remaining / 60);
                int seconds = (int)(remaining % 60);
                _timerText.text = $"{minutes:00}:{seconds:00}";
            }
            else
            {
                _timerText.text = "";
            }
        }

        #endregion
    }
}
