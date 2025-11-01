using System;
using UnityEngine;

namespace CommanderChess.Domain
{
    public readonly struct BoardCoord : IEquatable<BoardCoord>
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

        public override string ToString() => $"({x},{y})";

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

        // IEquatable implementation + overrides
        public bool Equals(BoardCoord other) => this.x == other.x && this.y == other.y;
        public override bool Equals(object obj) => obj is BoardCoord other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(x, y);
    }
}
