using System.Collections.Generic;
using CommanderChess.Core;

namespace CommanderChess.Pieces
{
    /// <summary>
    /// Bishop piece
    /// </summary>
    public class Bishop : ChessPiece
    {
        public Bishop(PlayerSide side, BoardPosition position) 
            : base(PieceType.Bishop, side, position)
        {
        }

        public override List<BoardPosition> GetPossibleMoves(ChessPiece[,] board)
        {
            var moves = new List<BoardPosition>();

            // Diagonal directions
            AddMovesInDirection(1, 1, board, moves);   // Up-Right
            AddMovesInDirection(1, -1, board, moves);  // Up-Left
            AddMovesInDirection(-1, 1, board, moves);  // Down-Right
            AddMovesInDirection(-1, -1, board, moves); // Down-Left

            return moves;
        }
    }
}
