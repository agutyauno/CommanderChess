using System.Collections.Generic;
using CommanderChess.Domain;

namespace CommanderChess.GameState
{
    /// <summary>
    /// GameStateData - Dữ liệu chia sẻ giữa các states
    /// </summary>
    public class GameStateData
    {
        // Current state info
        public GameState CurrentState { get; set; }

        // Selection info
        public BasePiece SelectedPiece { get; set; }
        public BoardCoord SelectedPosition { get; set; }

        // Pending action info
        public PlayerAction? PendingAction { get; set; }

        // Highlighted positions (for visual feedback)
        public List<BoardCoord> HighlightedMoves { get; set; } = new List<BoardCoord>();
        public List<BoardCoord> HighlightedAttacks { get; set; } = new List<BoardCoord>();

        // Bombing decision data (for Airforce)
        public BasePiece BombingAirforce { get; set; }
        public BoardCoord BombingOriginalPosition { get; set; }
        public BasePiece BombingCarrier { get; set; } // Carrier if Airforce was carried during capture

        /// <summary>
        /// Clear all data (dùng khi chuyển về Idle hoặc reset)
        /// </summary>
        public void Clear()
        {
            SelectedPiece = null;
            SelectedPosition = default;
            PendingAction = null;
            HighlightedMoves.Clear();
            HighlightedAttacks.Clear();
            BombingAirforce = null;
            BombingOriginalPosition = default;
            BombingCarrier = null;
        }
    }
}