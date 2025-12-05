using System;
using UnityEngine;

namespace CommanderChess.Core
{
    /// <summary>
    /// Represents a position on the chess board
    /// </summary>
    [Serializable]
    public struct BoardPosition : IEquatable<BoardPosition>
    {
        public int Row;
        public int Column;

        public static readonly BoardPosition Invalid = new BoardPosition(-1, -1);

        public BoardPosition(int row, int column)
        {
            Row = row;
            Column = column;
        }

        public bool IsValid => Row >= 0 && Row < 8 && Column >= 0 && Column < 8;

        public static BoardPosition FromNotation(string notation)
        {
            if (string.IsNullOrEmpty(notation) || notation.Length != 2)
                return Invalid;

            char colChar = char.ToLower(notation[0]);
            char rowChar = notation[1];

            if (colChar < 'a' || colChar > 'h' || rowChar < '1' || rowChar > '8')
                return Invalid;

            int column = colChar - 'a';
            int row = rowChar - '1';

            return new BoardPosition(row, column);
        }

        public string ToNotation()
        {
            if (!IsValid)
                return "??";

            char col = (char)('a' + Column);
            char row = (char)('1' + Row);

            return $"{col}{row}";
        }

        public static BoardPosition operator +(BoardPosition a, BoardPosition b)
        {
            return new BoardPosition(a.Row + b.Row, a.Column + b.Column);
        }

        public static BoardPosition operator -(BoardPosition a, BoardPosition b)
        {
            return new BoardPosition(a.Row - b.Row, a.Column - b.Column);
        }

        public static bool operator ==(BoardPosition a, BoardPosition b)
        {
            return a.Row == b.Row && a.Column == b.Column;
        }

        public static bool operator !=(BoardPosition a, BoardPosition b)
        {
            return !(a == b);
        }

        public bool Equals(BoardPosition other)
        {
            return Row == other.Row && Column == other.Column;
        }

        public override bool Equals(object obj)
        {
            return obj is BoardPosition position && Equals(position);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Row, Column);
        }

        public override string ToString()
        {
            return ToNotation();
        }

        /// <summary>
        /// Manhattan distance to another position
        /// </summary>
        public int DistanceTo(BoardPosition other)
        {
            return Math.Abs(Row - other.Row) + Math.Abs(Column - other.Column);
        }

        /// <summary>
        /// Chebyshev distance (max of row/column difference)
        /// </summary>
        public int ChebyshevDistanceTo(BoardPosition other)
        {
            return Math.Max(Math.Abs(Row - other.Row), Math.Abs(Column - other.Column));
        }
    }
}
