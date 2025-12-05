using System.Collections.Generic;
using CommanderChess.Core;

namespace CommanderChess.Pieces
{
    /// <summary>
    /// Pawn piece
    /// </summary>
    public class Pawn : ChessPiece
    {
        public BoardPosition? EnPassantTarget { get; set; }

        public Pawn(PlayerSide side, BoardPosition position) 
            : base(PieceType.Pawn, side, position)
        {
        }

        public override List<BoardPosition> GetPossibleMoves(ChessPiece[,] board)
        {
            var moves = new List<BoardPosition>();
            int direction = Side == PlayerSide.White ? 1 : -1;
            int startRow = Side == PlayerSide.White ? 1 : 6;
            int promotionRow = Side == PlayerSide.White ? 7 : 0;

            // Forward move
            var oneForward = new BoardPosition(Position.Row + direction, Position.Column);
            if (oneForward.IsValid && IsEmpty(oneForward, board))
            {
                moves.Add(oneForward);

                // Double move from starting position
                if (Position.Row == startRow)
                {
                    var twoForward = new BoardPosition(Position.Row + 2 * direction, Position.Column);
                    if (IsEmpty(twoForward, board))
                    {
                        moves.Add(twoForward);
                    }
                }
            }

            // Diagonal captures
            var leftCapture = new BoardPosition(Position.Row + direction, Position.Column - 1);
            var rightCapture = new BoardPosition(Position.Row + direction, Position.Column + 1);

            if (leftCapture.IsValid && IsEnemy(leftCapture, board))
            {
                moves.Add(leftCapture);
            }

            if (rightCapture.IsValid && IsEnemy(rightCapture, board))
            {
                moves.Add(rightCapture);
            }

            // En passant
            if (EnPassantTarget.HasValue)
            {
                var epPos = EnPassantTarget.Value;
                if (leftCapture == epPos || rightCapture == epPos)
                {
                    moves.Add(epPos);
                }
            }

            return moves;
        }

        public bool IsPromotionMove(BoardPosition to)
        {
            int promotionRow = Side == PlayerSide.White ? 7 : 0;
            return to.Row == promotionRow;
        }
    }
}
