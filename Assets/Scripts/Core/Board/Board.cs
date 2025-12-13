using UnityEngine;
using System.Collections.Generic;

namespace CommanderChess.Core
{
    /// <summary>
    /// Represents the game board with terrain information and piece positions
    /// Board size: 12x11 (VA: 0-11, HA: 0-10)
    /// Origin (0,0) at bottom-left corner
    /// </summary>
    public class Board : MonoBehaviour
    {
        #region Constants
        public const int BOARD_WIDTH = 12;  // VA axis (0-11)
        public const int BOARD_HEIGHT = 11; // HA axis (0-10)
        #endregion

        #region Fields
        [SerializeField] private Terrain[,] terrainGrid;
        private Dictionary<BoardCoord, BasePiece> pieces;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            InitializeBoard();
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the board with default terrain layout
        /// </summary>
        private void InitializeBoard()
        {
            terrainGrid = new Terrain[BOARD_WIDTH, BOARD_HEIGHT];
            pieces = new Dictionary<BoardCoord, BasePiece>();

            // Initialize with default land terrain
            for (int va = 0; va < BOARD_WIDTH; va++)
            {
                for (int ha = 0; ha < BOARD_HEIGHT; ha++)
                {
                    terrainGrid[va, ha] = Terrain.Land;
                }
            }

            SetupDefaultTerrain();
        }

        /// <summary>
        /// Sets up the default terrain layout for Commander Chess
        /// </summary>
        private void SetupDefaultTerrain()
        {
            BoardLayout.ApplyStandardLayout(this);
        }
        #endregion

        #region Terrain Management
        /// <summary>
        /// Gets the terrain type at the specified coordinate
        /// </summary>
        public Terrain GetTerrain(BoardCoord coord)
        {
            if (!IsInBoard(coord))
            {
                return Terrain.None;
            }
            return terrainGrid[coord.X, coord.Y];
        }

        /// <summary>
        /// Sets the terrain type at the specified coordinate
        /// </summary>
        public void SetTerrain(BoardCoord coord, Terrain terrain)
        {
            if (!IsInBoard(coord))
            {
                return;
            }
            terrainGrid[coord.X, coord.Y] = terrain;
        }

        public void SetTerrainRange(BoardCoord start, BoardCoord end, Terrain terrain)
        {
            if (!IsInBoard(start) || !IsInBoard(end))
            {
                return;
            }

            int minVA = Mathf.Min(start.X, end.X);
            int maxVA = Mathf.Max(start.X, end.X);
            int minHA = Mathf.Min(start.Y, end.Y);
            int maxHA = Mathf.Max(start.Y, end.Y);

            for (int va = minVA; va <= maxVA; va++)
            {
                for (int ha = minHA; ha <= maxHA; ha++)
                {
                    terrainGrid[va, ha] = terrain;
                }
            }
        }

        #endregion

        #region Piece Management
        /// <summary>
        /// Gets the piece at the specified coordinate
        /// </summary>
        public bool TryGetPiece(BoardCoord coord, out BasePiece piece)
        {
            piece = null;
            if (!IsInBoard(coord))
            {
                return false;
            }
            return pieces.TryGetValue(coord, out piece);
        }

        /// <summary>
        /// Places a piece at the specified coordinate
        /// </summary>
        public bool PlacePiece(BasePiece piece, BoardCoord coord)
        {
            if (piece == null)
            {
                Debug.LogError("Cannot place null piece");
                return false;
            }

            if (!IsInBoard(coord))
            {
                Debug.LogError($"Invalid coordinate: {coord}");
                return false;
            }

            if (pieces.ContainsKey(coord))
            {
                Debug.LogWarning($"Position {coord} is already occupied");
                return false;
            }

            pieces[coord] = piece;
            return true;
        }

        /// <summary>
        /// Moves a piece from one coordinate to another
        /// Returns the captured piece if any, null otherwise
        /// </summary>
        public BasePiece MovePiece(BoardCoord from, BoardCoord to)
        {
            if (!IsInBoard(from) || !IsInBoard(to))
            {
                Debug.LogError("Invalid coordinates for move");
                return null;
            }

            if (!pieces.TryGetValue(from, out BasePiece piece))
            {
                Debug.LogError($"No piece at {from}");
                return null;
            }

            // Capture piece at destination if exists
            pieces.TryGetValue(to, out BasePiece capturedPiece);

            // Remove from old position
            pieces.Remove(from);

            // Place at new position
            pieces[to] = piece;

            return capturedPiece;
        }

        /// <summary>
        /// Removes a piece from the board
        /// Returns the removed piece if any, null otherwise
        /// </summary>
        public BasePiece RemovePiece(BoardCoord coord)
        {
            if (!IsInBoard(coord))
            {
                return null;
            }

            if (pieces.TryGetValue(coord, out BasePiece piece))
            {
                pieces.Remove(coord);
                return piece;
            }
            return null;
        }

        /// <summary>
        /// Gets the position of a piece on the board
        /// Returns BoardCoord.Invalid if piece is not found
        /// </summary>
        public bool TryGetPiecePosition(BasePiece piece, out BoardCoord position)
        {
            position = BoardCoord.Invalid;
            if (piece == null)
                return false;

            foreach (var kvp in pieces)
            {
                if (kvp.Value == piece)
                {
                    position = kvp.Key;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if a position is occupied by a piece
        /// </summary>
        public bool IsOccupied(BoardCoord coord)
        {
            return IsInBoard(coord) && pieces.ContainsKey(coord);
        }

        /// <summary>
        /// Gets all pieces currently on the board
        /// </summary>
        public List<BasePiece> GetAllPieces()
        {
            return new List<BasePiece>(pieces.Values);
        }

        /// <summary>
        /// Gets all piece positions as a dictionary
        /// </summary>
        public IReadOnlyDictionary<BoardCoord, BasePiece> GetAllPiecePositions()
        {
            return pieces;
        }

        /// <summary>
        /// Gets the number of pieces on the board
        /// </summary>
        public int PieceCount => pieces.Count;
        #endregion

        #region Validation
        /// <summary>
        /// Checks if the coordinate is within board boundaries
        /// </summary>
        public bool IsInBoard(BoardCoord coord)
        {
            return coord.X >= 0 && coord.X < BOARD_WIDTH && coord.Y >= 0 && coord.Y < BOARD_HEIGHT;
        }
        #endregion

        #region Board State
        /// <summary>
        /// Clears all pieces from the board
        /// </summary>
        public void ClearPieces()
        {
            pieces.Clear();
        }

        /// <summary>
        /// Resets the board to initial state
        /// </summary>
        public void ResetBoard()
        {
            ClearPieces();
            InitializeBoard();
        }
        #endregion

        #region Debug
        /// <summary>
        /// Prints the board state for debugging
        /// </summary>
        public void DebugPrintBoard()
        {
            Debug.Log("=== Board State ===");
            Debug.Log($"Size: {BOARD_WIDTH}x{BOARD_HEIGHT}");
            Debug.Log($"Pieces on board: {pieces.Count}");
            
            foreach (var kvp in pieces)
            {
                Debug.Log($"{kvp.Value?.GetType().Name ?? "Unknown"} at {kvp.Key}");
            }
        }
        #endregion
    }
}