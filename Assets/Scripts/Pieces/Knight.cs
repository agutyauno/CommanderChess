using System.Collections.Generic;
using CommanderChess.Core;

namespace CommanderChess.Pieces
{
    /// <summary>
    /// Knight piece
    /// </summary>
    public class Knight : ChessPiece
    {
        private static readonly int[] RowOffsets = { 2, 2, 1, 1, -1, -1, -2, -2 };
        private static readonly int[] ColOffsets = { 1, -1, 2, -2, 2, -2, 1, -1 };

        public Knight(PlayerSide side, BoardPosition position) 
            : base(PieceType.Knight, side, position)
        {
        }

        public override List<BoardPosition> GetPossibleMoves(ChessPiece[,] board)
        {
            var moves = new List<BoardPosition>();

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

            return moves;
        }
    }
}
