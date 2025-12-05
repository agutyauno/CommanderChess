using System.Collections.Generic;
using CommanderChess.Core;
using CommanderChess.Managers;
using CommanderChess.Pieces;
using UnityEngine;

namespace CommanderChess.UI
{
    /// <summary>
    /// Handles board visualization and piece rendering
    /// </summary>
    public class BoardRenderer : MonoBehaviour
    {
        [Header("Board Settings")]
        [SerializeField] private float _squareSize = 1f;
        [SerializeField] private Vector3 _boardOffset = Vector3.zero;

        [Header("Colors")]
        [SerializeField] private Color _lightSquareColor = new Color(0.93f, 0.87f, 0.73f);
        [SerializeField] private Color _darkSquareColor = new Color(0.71f, 0.53f, 0.39f);
        [SerializeField] private Color _highlightColor = new Color(0.5f, 0.8f, 0.5f, 0.5f);
        [SerializeField] private Color _selectedColor = new Color(0.8f, 0.8f, 0.2f, 0.5f);
        [SerializeField] private Color _moveTargetColor = new Color(0.2f, 0.5f, 0.8f, 0.5f);
        [SerializeField] private Color _captureColor = new Color(0.8f, 0.2f, 0.2f, 0.5f);

        [Header("Prefabs")]
        [SerializeField] private GameObject _squarePrefab;
        [SerializeField] private GameObject _piecePrefabWhite;
        [SerializeField] private GameObject _piecePrefabBlack;
        [SerializeField] private GameObject _highlightPrefab;

        private GameObject[,] _squares;
        private Dictionary<ChessPiece, GameObject> _pieceObjects;
        private List<GameObject> _highlights;

        private void Awake()
        {
            _squares = new GameObject[8, 8];
            _pieceObjects = new Dictionary<ChessPiece, GameObject>();
            _highlights = new List<GameObject>();
        }

        private void Start()
        {
            CreateBoard();
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            if (GameManager.Instance == null)
                return;

            GameManager.Instance.OnTurnChanged += OnTurnChanged;
            GameManager.Instance.OnPieceSelected += OnPieceSelected;
            GameManager.Instance.OnPieceDeselected += OnPieceDeselected;
            GameManager.Instance.OnMoveMade += OnMoveMade;
        }

        private void UnsubscribeFromEvents()
        {
            if (GameManager.Instance == null)
                return;

            GameManager.Instance.OnTurnChanged -= OnTurnChanged;
            GameManager.Instance.OnPieceSelected -= OnPieceSelected;
            GameManager.Instance.OnPieceDeselected -= OnPieceDeselected;
            GameManager.Instance.OnMoveMade -= OnMoveMade;
        }

        /// <summary>
        /// Create the visual board
        /// </summary>
        public void CreateBoard()
        {
            // Clear existing board
            ClearBoard();

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    CreateSquare(row, col);
                }
            }
        }

        private void CreateSquare(int row, int col)
        {
            Vector3 position = GetWorldPosition(new BoardPosition(row, col));

            GameObject square;
            if (_squarePrefab != null)
            {
                square = Instantiate(_squarePrefab, position, Quaternion.identity, transform);
            }
            else
            {
                // Create a simple quad if no prefab is provided
                square = GameObject.CreatePrimitive(PrimitiveType.Quad);
                square.transform.position = position;
                square.transform.rotation = Quaternion.Euler(90, 0, 0);
                square.transform.localScale = Vector3.one * _squareSize;
                square.transform.parent = transform;
            }

            // Set color
            bool isLightSquare = (row + col) % 2 == 0;
            var renderer = square.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = isLightSquare ? _lightSquareColor : _darkSquareColor;
            }

            square.name = $"Square_{(char)('a' + col)}{row + 1}";
            _squares[row, col] = square;
        }

        /// <summary>
        /// Clear the visual board
        /// </summary>
        public void ClearBoard()
        {
            // Destroy squares
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if (_squares[row, col] != null)
                    {
                        Destroy(_squares[row, col]);
                        _squares[row, col] = null;
                    }
                }
            }

            // Destroy pieces
            foreach (var pieceObj in _pieceObjects.Values)
            {
                if (pieceObj != null)
                    Destroy(pieceObj);
            }
            _pieceObjects.Clear();

            ClearHighlights();
        }

        /// <summary>
        /// Refresh all pieces on the board
        /// </summary>
        public void RefreshPieces()
        {
            if (GameManager.Instance == null)
                return;

            // Clear existing piece objects
            foreach (var pieceObj in _pieceObjects.Values)
            {
                if (pieceObj != null)
                    Destroy(pieceObj);
            }
            _pieceObjects.Clear();

            // Create new piece objects
            var pieces = GameManager.Instance.GetAllPieces();
            foreach (var piece in pieces)
            {
                if (!piece.IsCaptured)
                {
                    CreatePieceObject(piece);
                }
            }
        }

        private void CreatePieceObject(ChessPiece piece)
        {
            Vector3 position = GetWorldPosition(piece.Position);
            position.y += 0.1f; // Slight elevation above board

            GameObject piecePrefab = piece.Side == PlayerSide.White ? _piecePrefabWhite : _piecePrefabBlack;
            
            GameObject pieceObj;
            if (piecePrefab != null)
            {
                pieceObj = Instantiate(piecePrefab, position, Quaternion.identity, transform);
            }
            else
            {
                // Create a simple placeholder if no prefab
                pieceObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pieceObj.transform.position = position;
                pieceObj.transform.localScale = new Vector3(_squareSize * 0.4f, 0.1f, _squareSize * 0.4f);
                pieceObj.transform.parent = transform;

                var renderer = pieceObj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = piece.Side == PlayerSide.White ? Color.white : Color.black;
                }
            }

            pieceObj.name = $"Piece_{piece.Side}_{piece.Type}_{piece.Position}";
            _pieceObjects[piece] = pieceObj;
        }

        /// <summary>
        /// Move a piece visually
        /// </summary>
        public void MovePiece(ChessPiece piece, BoardPosition from, BoardPosition to)
        {
            if (_pieceObjects.TryGetValue(piece, out GameObject pieceObj))
            {
                Vector3 targetPos = GetWorldPosition(to);
                targetPos.y = pieceObj.transform.position.y;
                pieceObj.transform.position = targetPos;
            }
        }

        /// <summary>
        /// Remove a piece visually (when captured)
        /// </summary>
        public void RemovePiece(ChessPiece piece)
        {
            if (_pieceObjects.TryGetValue(piece, out GameObject pieceObj))
            {
                Destroy(pieceObj);
                _pieceObjects.Remove(piece);
            }
        }

        /// <summary>
        /// Show available move highlights
        /// </summary>
        public void ShowMoveHighlights(List<Move> moves)
        {
            ClearHighlights();

            foreach (var move in moves)
            {
                CreateHighlight(move.To, move.Type == MoveType.Capture ? _captureColor : _moveTargetColor);
            }
        }

        /// <summary>
        /// Show selected piece highlight
        /// </summary>
        public void ShowSelectedHighlight(BoardPosition position)
        {
            CreateHighlight(position, _selectedColor);
        }

        private void CreateHighlight(BoardPosition position, Color color)
        {
            Vector3 worldPos = GetWorldPosition(position);
            worldPos.y += 0.05f;

            GameObject highlight;
            if (_highlightPrefab != null)
            {
                highlight = Instantiate(_highlightPrefab, worldPos, Quaternion.identity, transform);
            }
            else
            {
                highlight = GameObject.CreatePrimitive(PrimitiveType.Quad);
                highlight.transform.position = worldPos;
                highlight.transform.rotation = Quaternion.Euler(90, 0, 0);
                highlight.transform.localScale = Vector3.one * _squareSize * 0.9f;
                highlight.transform.parent = transform;

                // Remove collider so it doesn't interfere with clicks
                var collider = highlight.GetComponent<Collider>();
                if (collider != null)
                    Destroy(collider);
            }

            var renderer = highlight.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = color;
            }

            _highlights.Add(highlight);
        }

        /// <summary>
        /// Clear all highlights
        /// </summary>
        public void ClearHighlights()
        {
            foreach (var highlight in _highlights)
            {
                if (highlight != null)
                    Destroy(highlight);
            }
            _highlights.Clear();
        }

        /// <summary>
        /// Convert board position to world position
        /// </summary>
        public Vector3 GetWorldPosition(BoardPosition position)
        {
            return new Vector3(
                position.Column * _squareSize + _squareSize / 2,
                0,
                position.Row * _squareSize + _squareSize / 2
            ) + _boardOffset;
        }

        /// <summary>
        /// Convert world position to board position
        /// </summary>
        public BoardPosition GetBoardPosition(Vector3 worldPosition)
        {
            Vector3 local = worldPosition - _boardOffset;
            int col = Mathf.FloorToInt(local.x / _squareSize);
            int row = Mathf.FloorToInt(local.z / _squareSize);
            return new BoardPosition(row, col);
        }

        #region Event Handlers

        private void OnTurnChanged(PlayerSide player)
        {
            RefreshPieces();
            ClearHighlights();
        }

        private void OnPieceSelected(ChessPiece piece)
        {
            ClearHighlights();
            ShowSelectedHighlight(piece.Position);

            if (GameManager.Instance != null)
            {
                ShowMoveHighlights(GameManager.Instance.AvailableMoves);
            }
        }

        private void OnPieceDeselected()
        {
            ClearHighlights();
        }

        private void OnMoveMade(Move move)
        {
            var piece = GameManager.Instance?.GetPieceAt(move.To);
            if (piece != null)
            {
                MovePiece(piece, move.From, move.To);
            }

            ClearHighlights();
        }

        #endregion
    }
}
