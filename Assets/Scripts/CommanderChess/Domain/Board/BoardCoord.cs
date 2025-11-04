using System;
using UnityEngine;

namespace CommanderChess.Domain
{
    public struct BoardCoord
    {
        public readonly int x;
        public readonly int y;

        public BoardCoord(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        // Conversions to/from Unity types for convenience
        public static implicit operator Vector2Int(BoardCoord c) => new(c.x, c.y);
        public static implicit operator Vector3Int(BoardCoord c) => new(c.x, c.y, 0);
        public static implicit operator BoardCoord(Vector2Int v) => new(v.x, v.y);
        public static implicit operator BoardCoord(Vector3Int v) => new(v.x, v.y);

        // equality operators
        public static bool operator ==(BoardCoord a, BoardCoord b) => a.x == b.x && a.y == b.y;
        public static bool operator !=(BoardCoord a, BoardCoord b) => !(a == b);

        // Operator overloads for vector-like operations
        public static BoardCoord operator +(BoardCoord a, BoardCoord b)
            => new BoardCoord(a.x + b.x, a.y + b.y);

        public static BoardCoord operator -(BoardCoord a, BoardCoord b)
            => new BoardCoord(a.x - b.x, a.y - b.y);

        public static BoardCoord operator *(BoardCoord a, int scalar)
            => new BoardCoord(a.x * scalar, a.y * scalar);

        public static BoardCoord operator /(BoardCoord a, int scalar)
            => new BoardCoord(a.x / scalar, a.y / scalar);

        public override string ToString() => $"({x}, {y})";

        // Convert to label like "A1", "B3", "AA10"
        public string ToLabel()
        {
            if (x <= 0) throw new ArgumentOutOfRangeException(nameof(x), "x must be >= 1 for label");
            int col = x - 1;
            string colLetters = "";
            do
            {
                int rem = col % 26;
                colLetters = (char)('A' + rem) + colLetters;
                col = (col / 26) - 1;
            } while (col >= 0);

            int rowNumber = y;
            return $"{colLetters}{rowNumber}";
        }

        // Try parse a label into 1-based BoardCoord (letters -> x, numbers -> y)
        public static bool TryParseLabel(string label, out BoardCoord coord)
        {
            coord = default;
            if (string.IsNullOrWhiteSpace(label)) return false;

            label = label.Trim();
            int i = 0;
            while (i < label.Length && char.IsLetter(label[i])) i++;
            if (i == 0) return false;

            string letters = label.Substring(0, i).ToUpperInvariant();
            string digits = label.Substring(i);
            if (string.IsNullOrEmpty(digits) || !int.TryParse(digits, out int row) || row <= 0) return false;

            int col = 0;
            for (int j = 0; j < letters.Length; j++)
            {
                char c = letters[j];
                if (c < 'A' || c > 'Z') return false;
                col = col * 26 + (c - 'A' + 1);
            }

            // letters -> x (1-based), digits -> y (1-based)
            coord = new BoardCoord(col, row);
            return true;
        }

        // Utility methods for common operations
        public int ManhattanDistance(BoardCoord other)
            => Mathf.Abs(x - other.x) + Mathf.Abs(y - other.y);

        public float EuclideanDistance(BoardCoord other)
            => Mathf.Sqrt(Mathf.Pow(x - other.x, 2) + Mathf.Pow(y - other.y, 2));

        public bool IsAdjacent(BoardCoord other)
            => ManhattanDistance(other) == 1;

        public bool IsDiagonal(BoardCoord other)
            => Mathf.Abs(x - other.x) == 1 && Mathf.Abs(y - other.y) == 1;

        // Direction vectors for common movements
        public static readonly BoardCoord Up = new BoardCoord(0, 1);
        public static readonly BoardCoord Down = new BoardCoord(0, -1);
        public static readonly BoardCoord Left = new BoardCoord(-1, 0);
        public static readonly BoardCoord Right = new BoardCoord(1, 0);

        public static readonly BoardCoord UpLeft = new BoardCoord(-1, 1);
        public static readonly BoardCoord UpRight = new BoardCoord(1, 1);
        public static readonly BoardCoord DownLeft = new BoardCoord(-1, -1);
        public static readonly BoardCoord DownRight = new BoardCoord(1, -1);

        // Helper method to get all adjacent coordinates
        public BoardCoord[] GetAdjacentCoords()
        {
            return new BoardCoord[]
            {
                this + Up,
                this + Right,
                this + Down,
                this + Left
            };
        }

        // Helper method to get all surrounding coordinates (including diagonals)
        public BoardCoord[] GetSurroundingCoords()
        {
            return new BoardCoord[]
            {
                this + Up,
                this + UpRight,
                this + Right,
                this + DownRight,
                this + Down,
                this + DownLeft,
                this + Left,
                this + UpLeft
            };
        }

        // IEquatable implementation + overrides
        public bool Equals(BoardCoord other) => this.x == other.x && this.y == other.y;
        public override bool Equals(object obj) => obj is BoardCoord other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(x, y);
    }
}
