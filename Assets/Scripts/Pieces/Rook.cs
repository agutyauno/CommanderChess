using System.Collections.Generic;
using CommanderChess.Core;

namespace CommanderChess.Pieces
{
    /// <summary>
    /// Rook piece
    /// </summary>
    public class Rook : ChessPiece
    {
        public Rook(PlayerSide side, BoardPosition position) 
            : base(PieceType.Rook, side, position)
        {
        }

        public override List<BoardPosition> GetPossibleMoves(ChessPiece[,] board)
        {
            var moves = new List<BoardPosition>();

            // Horizontal and vertical directions
            AddMovesInDirection(1, 0, board, moves);  // Up
            AddMovesInDirection(-1, 0, board, moves); // Down
            AddMovesInDirection(0, 1, board, moves);  // Right
            AddMovesInDirection(0, -1, board, moves); // Left

            return moves;
        }
    }
}
