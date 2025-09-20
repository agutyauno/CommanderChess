using System;
using UnityEngine;

public readonly struct BoardCoord
{
    public readonly int x;
    public readonly int y;

    public BoardCoord(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    // Conversions to/from Unity types for convenience
    public static implicit operator Vector2Int(BoardCoord c) => new Vector2Int(c.x, c.y);
    public static implicit operator Vector3Int(BoardCoord c) => new Vector3Int(c.x, c.y, 0);
    public static implicit operator BoardCoord(Vector2Int v) => new BoardCoord(v.x, v.y);
    public static implicit operator BoardCoord(Vector3Int v) => new BoardCoord(v.x, v.y);

    public override string ToString() => $"({x},{y})";

    // Convert to label like "A1", "B3", "AA10"
    public string ToLabel()
    {
        if (x < 0) throw new ArgumentOutOfRangeException(nameof(x));
        int col = x;
        string colLetters = "";
        do
        {
            int rem = col % 26;
            colLetters = (char)('A' + rem) + colLetters;
            col = (col / 26) - 1;
        } while (col >= 0);

        int rowNumber = y + 1;
        return $"{colLetters}{rowNumber}";
    }

    // Try parse a label (no board-bounds check here)
    public static bool TryParseLabel(string label, out BoardCoord coord)
    {
        coord = default;
        if (string.IsNullOrWhiteSpace(label)) return false;

        int i = 0;
        while (i < label.Length && char.IsLetter(label[i])) i++;
        if (i == 0) return false;

        string letters = label.Substring(0, i).ToUpperInvariant();
        string digits = label.Substring(i);
        if (!int.TryParse(digits, out int row) || row <= 0) return false;

        int col = 0;
        for (int j = 0; j < letters.Length; j++)
        {
            char c = letters[j];
            if (c < 'A' || c > 'Z') return false;
            col = col * 26 + (c - 'A' + 1);
        }
        col -= 1;
        coord = new BoardCoord(col, row - 1);
        return true;
    }

    public override bool Equals(object obj) => obj is BoardCoord other && other.x == x && other.y == y;
    public override int GetHashCode() => HashCode.Combine(x, y);
}
