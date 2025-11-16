using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using VContainer;
using CommanderChess.Services;
using CommanderChess.Domain;

namespace CommanderChess.Presentation
{
    /// <summary>
    /// BoardHighlighter - Hiển thị visual feedback trên board bằng Tilemaps
    /// </summary>
    public class BoardHighlighter : MonoBehaviour
    {
        [Inject] readonly Board board;
        [Inject] EventBus eventBus;

        #region Tilemaps
        [Header("Tilemaps")]
        [SerializeField] Tilemap moveTilemap;
        [SerializeField] Tilemap attackTilemap;
        [SerializeField] Tilemap selectedTilemap;
        [SerializeField] Tilemap dangerTilemap;
        #endregion

        #region Tiles
        [Header("Tiles")]
        [SerializeField] TileBase moveTile;       // Green tile
        [SerializeField] TileBase attackTile;     // Red tile
        [SerializeField] TileBase selectedTile;   // Yellow tile
        [SerializeField] TileBase dangerTile;     // Orange tile
        #endregion

        #region Current State
        readonly HashSet<BoardCoord> currentMoves = new HashSet<BoardCoord>();
        readonly HashSet<BoardCoord> currentAttacks = new HashSet<BoardCoord>();
        readonly HashSet<BoardCoord> currentDangers = new HashSet<BoardCoord>();
        BoardCoord? currentSelected;
        #endregion

        #region Public API - Highlight Methods

        /// <summary>
        /// Highlight valid move positions
        /// </summary>
        public void HighlightMoves(List<BoardCoord> moves)
        {
            ClearMoves();

            if (moves == null || moves.Count == 0) return;

            foreach (var coord in moves)
            {
                if (board.IsInBoard(coord))
                {
                    var cellPos = board.BoardCoordToCell(coord);
                    moveTilemap.SetTile(cellPos, moveTile);
                    currentMoves.Add(coord);
                }
            }

//             Debug.Log($"Highlighted {currentMoves.Count} move positions");
        }

        /// <summary>
        /// Highlight valid attack positions
        /// </summary>
        public void HighlightAttacks(List<BoardCoord> attacks)
        {
            ClearAttacks();

            if (attacks == null || attacks.Count == 0) return;

            foreach (var coord in attacks)
            {
                if (board.IsInBoard(coord))
                {
                    var cellPos = board.BoardCoordToCell(coord);
                    attackTilemap.SetTile(cellPos, attackTile);
                    currentAttacks.Add(coord);
                }
            }

//             Debug.Log($"Highlighted {currentAttacks.Count} attack positions");
        }

        /// <summary>
        /// Highlight selected piece position
        /// </summary>
        public void HighlightSelected(BoardCoord coord)
        {
            ClearSelected();

            if (board.IsInBoard(coord))
            {
                var cellPos = board.BoardCoordToCell(coord);
                selectedTilemap.SetTile(cellPos, selectedTile);
                currentSelected = coord;

//                 Debug.Log($"Highlighted selected position: {coord.ToLabel()}");
            }
        }

        /// <summary>
        /// Highlight danger zones (Ring of Fire, etc.)
        /// </summary>
        public void HighlightDangerZones(List<BoardCoord> zones)
        {
            ClearDangers();

            if (zones == null || zones.Count == 0) return;

            foreach (var coord in zones)
            {
                if (board.IsInBoard(coord))
                {
                    var cellPos = board.BoardCoordToCell(coord);
                    dangerTilemap.SetTile(cellPos, dangerTile);
                    currentDangers.Add(coord);
                }
            }

//             Debug.Log($"Highlighted {currentDangers.Count} danger zones");
        }

        #endregion

        #region Public API - Clear Methods

        /// <summary>
        /// Clear move highlights
        /// </summary>
        public void ClearMoves()
        {
            foreach (var coord in currentMoves)
            {
                var cellPos = board.BoardCoordToCell(coord);
                moveTilemap.SetTile(cellPos, null);
            }
            currentMoves.Clear();
        }

        /// <summary>
        /// Clear attack highlights
        /// </summary>
        public void ClearAttacks()
        {
            foreach (var coord in currentAttacks)
            {
                var cellPos = board.BoardCoordToCell(coord);
                attackTilemap.SetTile(cellPos, null);
            }
            currentAttacks.Clear();
        }

        /// <summary>
        /// Clear selected highlight
        /// </summary>
        public void ClearSelected()
        {
            if (currentSelected.HasValue)
            {
                var cellPos = board.BoardCoordToCell(currentSelected.Value);
                selectedTilemap.SetTile(cellPos, null);
                currentSelected = null;
            }
        }

        /// <summary>
        /// Clear danger highlights
        /// </summary>
        public void ClearDangers()
        {
            foreach (var coord in currentDangers)
            {
                var cellPos = board.BoardCoordToCell(coord);
                dangerTilemap.SetTile(cellPos, null);
            }
            currentDangers.Clear();
        }

        /// <summary>
        /// Clear all highlights
        /// </summary>
        public void ClearAll()
        {
            ClearMoves();
            ClearAttacks();
            ClearSelected();
            ClearDangers();
//             Debug.Log("Cleared all highlights");
        }

        #endregion

        #region Validation (Editor)

#if UNITY_EDITOR
        void OnValidate()
        {
            ValidateSetup();
        }

        void ValidateSetup()
        {
            if (moveTilemap == null)
                Debug.LogWarning("BoardHighlighter: moveTilemap not assigned!", this);

            if (attackTilemap == null)
                Debug.LogWarning("BoardHighlighter: attackTilemap not assigned!", this);

            if (selectedTilemap == null)
                Debug.LogWarning("BoardHighlighter: selectedTilemap not assigned!", this);

            if (dangerTilemap == null)
                Debug.LogWarning("BoardHighlighter: dangerTilemap not assigned!", this);

            if (moveTile == null)
                Debug.LogWarning("BoardHighlighter: moveTile not assigned!", this);

            if (attackTile == null)
                Debug.LogWarning("BoardHighlighter: attackTile not assigned!", this);

            if (selectedTile == null)
                Debug.LogWarning("BoardHighlighter: selectedTile not assigned!", this);

            if (dangerTile == null)
                Debug.LogWarning("BoardHighlighter: dangerTile not assigned!", this);
        }
#endif

        #endregion

        #region Utility Methods

        /// <summary>
        /// Check if a position is currently highlighted as move
        /// </summary>
        public bool IsHighlightedAsMove(BoardCoord coord)
        {
            return currentMoves.Contains(coord);
        }

        /// <summary>
        /// Check if a position is currently highlighted as attack
        /// </summary>
        public bool IsHighlightedAsAttack(BoardCoord coord)
        {
            return currentAttacks.Contains(coord);
        }

        /// <summary>
        /// Check if a position is currently selected
        /// </summary>
        public bool IsSelected(BoardCoord coord)
        {
            return currentSelected.HasValue && currentSelected.Value == coord;
        }

        /// <summary>
        /// Check if a position is in danger zone
        /// </summary>
        public bool IsInDangerZone(BoardCoord coord)
        {
            return currentDangers.Contains(coord);
        }

        #endregion

        void OnEnable()
        {
            // ✅ Subscribe to movement events
            eventBus.Subscribe<PieceMovedEvent>(OnPieceMoved);
            eventBus.Subscribe<PieceCapturedEvent>(OnPieceCaptured);
            eventBus.Subscribe<PieceBoardedEvent>(OnPieceBoarded);
            eventBus.Subscribe<PieceDetachedEvent>(OnPieceDetached);
        }

        void OnDisable()
        {
            // ✅ Always unsubscribe
            eventBus.Unsubscribe<PieceMovedEvent>(OnPieceMoved);
            eventBus.Unsubscribe<PieceCapturedEvent>(OnPieceCaptured);
            eventBus.Unsubscribe<PieceBoardedEvent>(OnPieceBoarded);
            eventBus.Unsubscribe<PieceDetachedEvent>(OnPieceDetached);
        }

        void OnPieceMoved(PieceMovedEvent evt)
        {
            // Update highlights
            ClearAll();
        }

        void OnPieceCaptured(PieceCapturedEvent evt)
        {
            // Show capture effect
            // PlayCaptureEffect(evt.To);
        }

        void OnPieceBoarded(PieceBoardedEvent evt)
        {
            // Show boarding animation
            // PlayBoardingAnimation(evt.Carrier, evt.Passenger);
        }

        void OnPieceDetached(PieceDetachedEvent evt)
        {
            // Show detach animation
            // PlayDetachAnimation(evt.Passenger, evt.DetachPosition);
        }
    }
}
