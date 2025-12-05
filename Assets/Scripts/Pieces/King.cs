using System.Collections.Generic;
using CommanderChess.Core;

namespace CommanderChess.Pieces
{
    /// <summary>
    /// King piece
    /// </summary>
    public class King : ChessPiece
    {
        private static readonly int[] RowOffsets = { 1, 1, 1, 0, 0, -1, -1, -1 };
        private static readonly int[] ColOffsets = { -1, 0, 1, -1, 1, -1, 0, 1 };

        public King(PlayerSide side, BoardPosition position) 
            : base(PieceType.King, side, position)
        {
        }

        public override List<BoardPosition> GetPossibleMoves(ChessPiece[,] board)
        {
            var moves = new List<BoardPosition>();

            // Regular king moves (one square in any direction)
            for (int i = 0; i < 8; i++)
            {
                var newPos = new BoardPosition(
                    Position.Row + RowOffsets[i],
                    Position.Column + ColOffsets[i]
                );

                if (newPos.IsValid && IsEmptyOrEnemy(newPos, board))
                {
                    moves.Add(newPos);
                }
            }

            // Castling moves are handled separately by the game logic
            // since they require checking for attacks and other conditions

            return moves;
        }

        /// <summary>
        /// Check if kingside castling is potentially available
        /// </summary>
        public bool CanCastleKingside(ChessPiece[,] board)
        {
            if (HasMoved)
                return false;

            int row = Side == PlayerSide.White ? 0 : 7;
            
            // Check rook
            var rook = board[row, 7] as Rook;
            if (rook == null || rook.HasMoved || rook.Side != Side)
                return false;

            // Check empty squares between king and rook
            for (int col = 5; col <= 6; col++)
            {
                if (board[row, col] != null)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Check if queenside castling is potentially available
        /// </summary>
        public bool CanCastleQueenside(ChessPiece[,] board)
        {
            if (HasMoved)
                return false;

            int row = Side == PlayerSide.White ? 0 : 7;
            
            // Check rook
            var rook = board[row, 0] as Rook;
            if (rook == null || rook.HasMoved || rook.Side != Side)
                return false;

            // Check empty squares between king and rook
            for (int col = 1; col <= 3; col++)
            {
                if (board[row, col] != null)
                    return false;
            }

            return true;
        }
    }
}
