using System.Collections.Generic;
using CommanderChess.Core;

namespace CommanderChess.Pieces
{
    /// <summary>
    /// Queen piece
    /// </summary>
    public class Queen : ChessPiece
    {
        public Queen(PlayerSide side, BoardPosition position) 
            : base(PieceType.Queen, side, position)
        {
        }

        public override List<BoardPosition> GetPossibleMoves(ChessPiece[,] board)
        {
            var moves = new List<BoardPosition>();

            // Horizontal and vertical (Rook-like)
            AddMovesInDirection(1, 0, board, moves);
            AddMovesInDirection(-1, 0, board, moves);
            AddMovesInDirection(0, 1, board, moves);
            AddMovesInDirection(0, -1, board, moves);

            // Diagonal (Bishop-like)
            AddMovesInDirection(1, 1, board, moves);
            AddMovesInDirection(1, -1, board, moves);
            AddMovesInDirection(-1, 1, board, moves);
            AddMovesInDirection(-1, -1, board, moves);

            return moves;
        }
    }
}
