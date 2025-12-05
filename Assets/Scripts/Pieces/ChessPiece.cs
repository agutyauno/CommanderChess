using System;
using System.Collections.Generic;
using CommanderChess.Core;
using UnityEngine;

namespace CommanderChess.Pieces
{
    /// <summary>
    /// Base class for all chess pieces
    /// </summary>
    [Serializable]
    public abstract class ChessPiece
    {
        public PieceType Type { get; protected set; }
        public PlayerSide Side { get; protected set; }
        public BoardPosition Position { get; set; }
        public bool HasMoved { get; set; }
        public bool IsCaptured { get; set; }

        protected ChessPiece(PieceType type, PlayerSide side, BoardPosition position)
        {
            Type = type;
            Side = side;
            Position = position;
            HasMoved = false;
            IsCaptured = false;
        }

        /// <summary>
        /// Get all possible moves for this piece (without checking for check)
        /// </summary>
        public abstract List<BoardPosition> GetPossibleMoves(ChessPiece[,] board);

        /// <summary>
        /// Check if this piece can move to the target position
        /// </summary>
        public virtual bool CanMoveTo(BoardPosition target, ChessPiece[,] board)
        {
            if (!target.IsValid)
                return false;

            return GetPossibleMoves(board).Contains(target);
        }

        /// <summary>
        /// Check if the target position is occupied by enemy
        /// </summary>
        protected bool IsEnemy(BoardPosition pos, ChessPiece[,] board)
        {
            if (!pos.IsValid)
                return false;

            var piece = board[pos.Row, pos.Column];
            return piece != null && piece.Side != Side;
        }

        /// <summary>
        /// Check if the target position is empty
        /// </summary>
        protected bool IsEmpty(BoardPosition pos, ChessPiece[,] board)
        {
            if (!pos.IsValid)
                return false;

            return board[pos.Row, pos.Column] == null;
        }

        /// <summary>
        /// Check if the target position is empty or has enemy
        /// </summary>
        protected bool IsEmptyOrEnemy(BoardPosition pos, ChessPiece[,] board)
        {
            return IsEmpty(pos, board) || IsEnemy(pos, board);
        }

        /// <summary>
        /// Add all valid moves in a direction (for sliding pieces)
        /// </summary>
        protected void AddMovesInDirection(int rowDir, int colDir, ChessPiece[,] board, List<BoardPosition> moves)
        {
            for (int i = 1; i < 8; i++)
            {
                var newPos = new BoardPosition(Position.Row + rowDir * i, Position.Column + colDir * i);
                
                if (!newPos.IsValid)
                    break;

                if (IsEmpty(newPos, board))
                {
                    moves.Add(newPos);
                }
                else if (IsEnemy(newPos, board))
                {
                    moves.Add(newPos);
                    break;
                }
                else
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Get the piece symbol for notation
        /// </summary>
        public char GetSymbol()
        {
            char symbol = Type switch
            {
                PieceType.King => 'K',
                PieceType.Queen => 'Q',
                PieceType.Rook => 'R',
                PieceType.Bishop => 'B',
                PieceType.Knight => 'N',
                PieceType.Pawn => 'P',
                PieceType.Commander => 'C',
                _ => '?'
            };

            return Side == PlayerSide.White ? symbol : char.ToLower(symbol);
        }

        public override string ToString()
        {
            return $"{Side} {Type} at {Position}";
        }

        /// <summary>
        /// Create a piece of the specified type
        /// </summary>
        public static ChessPiece Create(PieceType type, PlayerSide side, BoardPosition position)
        {
            return type switch
            {
                PieceType.Pawn => new Pawn(side, position),
                PieceType.Rook => new Rook(side, position),
                PieceType.Knight => new Knight(side, position),
                PieceType.Bishop => new Bishop(side, position),
                PieceType.Queen => new Queen(side, position),
                PieceType.King => new King(side, position),
                PieceType.Commander => new Commander(side, position),
                _ => throw new ArgumentException($"Invalid piece type: {type}")
            };
        }
    }
}
