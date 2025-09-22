using System;
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Board : MonoBehaviour
{
    public readonly struct Size
    {
        public readonly int width;
        public readonly int height;

        public Size(int width, int height)
        {
            this.width = width;
            this.height = height;
        }
    }

    [SerializeField] Vector2Int offset;
    [SerializeField] Grid grid;
    Size boardSize = new(width: 11, height: 12);
    public readonly Dictionary<BoardCoord, PositionType> zoneMap = new();

    public Grid Grid { get => grid; }
    public Size BoardSize { get => boardSize; }

    void Awake()
    {
        grid = GetComponent<Grid>();
    }

    public bool IsInBoard(BoardCoord position)
    {
        return position.x >= 1 && position.x <= boardSize.width
            && position.y >= 1 && position.y <= boardSize.height;
    }

    public string BoardCoordToLabel(BoardCoord position)
    {
        if (!IsInBoard(position))
            throw new ArgumentOutOfRangeException("Position is out of board range.");
        return position.ToLabel();
    }

    public bool TryBoardLabelToCoord(string label, out BoardCoord coord)
    {
        if (!BoardCoord.TryParseLabel(label, out coord))
            throw new ArgumentException("Label must be in the format of a letter followed by a number (e.g., A1, B12).");

        if (!IsInBoard(coord))
            throw new ArgumentOutOfRangeException("Converted position is out of board range.");
        return true;
    }

    public Vector3Int BoardCoordToCell(BoardCoord position)
    {
        if (!IsInBoard(position))
            throw new ArgumentOutOfRangeException("Position is out of board range.");
        Vector3Int offsetPos = Vector3Int.RoundToInt(transform.position) + (Vector3Int)offset;
        return position + offsetPos;
    }

    public Vector3Int BoardCoordToCell(string label)
    {
        if (!TryBoardLabelToCoord(label, out BoardCoord coord))
            throw new ArgumentException("Label must be in the format of a letter followed by a number (e.g., A1, B12).");
        return BoardCoordToCell(coord);
    }

    public Vector3 BoardCoordToWorld(BoardCoord position) => grid.CellToWorld(BoardCoordToCell(position));

    public Vector3 BoardCoordToWorld(string label) => grid.CellToWorld(BoardCoordToCell(label));

    public void SetZoneRange(BoardCoord from, BoardCoord to, PositionType type)
    {
        for (int y = Math.Min(from.x, to.x); y <= Math.Max(from.x, to.x); y++)
        {
            for (int x = Math.Min(from.y, to.y); x <= Math.Max(from.y, to.y); x++)
            {
                var pos = new BoardCoord(x, y);
                if (IsInBoard(pos)) zoneMap[pos] = type;
            }
        }
    }

    public bool TryGetZone(BoardCoord coord, out PositionType type)
    {
        type = default;
        if (!IsInBoard(coord)) return false;
        return zoneMap.TryGetValue(coord, out type);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!DrawAxisLabels())
        {
            Debug.Log("Failed to draw axis labels. Make sure the Grid component is attached to the same GameObject as this Board script.");
            return;
        }
    }

    private bool DrawAxisLabels()
    {
        if (grid == null) return false;

        // styles
        var axisCharStyle = new GUIStyle();
        axisCharStyle.normal.textColor = Color.white;
        axisCharStyle.alignment = TextAnchor.MiddleRight; // dùng cho chữ ngang (bên trên/giữa)

        var axisNumberStyle = new GUIStyle();
        axisNumberStyle.normal.textColor = Color.white;
        axisNumberStyle.alignment = TextAnchor.UpperCenter; // dùng cho số dọc (trên cùng của nhãn)

        // helper: convert 0-based index to letters (A, B, ..., Z, AA, AB...)
        string IndexToLetters(int index)
        {
            int col = index;
            string s = "";
            do
            {
                int rem = col % 26;
                s = (char)('A' + rem) + s;
                col = (col / 26) - 1;
            } while (col >= 0);
            return s;
        }

        float outOffsetX = Mathf.Abs(grid.cellSize.x) * 0.6f; // distance outward for left labels
        float outOffsetY = Mathf.Abs(grid.cellSize.y) * 0.6f; // distance outward for bottom/top labels

        // --- ĐẢO: Vertical axis (Y) now shows numbers ---
        for (int y = 1; y <= boardSize.height; y++)
        {
            var bc = new BoardCoord(1, y);
            if (!IsInBoard(bc)) continue;

            Vector3 world = BoardCoordToWorld(bc); // intersection world position
            // position label to the left of the leftmost intersections
            Vector3 labelPos = world + Vector3.left * outOffsetX;
            // map y (1-based board) -> number
            string number = y.ToString();
            Handles.Label(labelPos, number, axisNumberStyle);
        }

        // --- ĐẢO: Horizontal axis (X) now shows letters ---
        for (int x = 1; x <= boardSize.width; x++)
        {
            var bc = new BoardCoord(x, 1);
            if (!IsInBoard(bc)) continue;

            Vector3 world = BoardCoordToWorld(bc); // intersection world position
            // position label below the bottommost intersections
            Vector3 labelPos = world + Vector3.down * outOffsetY;
            // map x (1-based board) -> 0-based index for letters
            string letter = IndexToLetters(x - 1);
            Handles.Label(labelPos, letter, axisCharStyle);
        }

        return true;
    }
#endif
}
