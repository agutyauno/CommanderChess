using System;
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace CommanderChess.Domain
{
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
        readonly Dictionary<BoardCoord, Terrains> terrainMap = new();
        readonly Dictionary<BoardCoord, BasePiece> pieces = new();
        public Grid Grid { get => grid; }
        public Size BoardSize { get => boardSize; }
        #region Properties
        public Dictionary<BoardCoord, BasePiece> Pieces => pieces;

        #endregion
        void Awake()
        {
            grid = GetComponent<Grid>();
        }

        public void Init()
        {
            SetUpTerrains();
        }

        void SetUpTerrains()
        {
            BoardCoord from, to;

            // set land
            from = new BoardCoord(1, 1);
            to = new BoardCoord(11, 12);
            SetTerrainRange(from, to, Terrains.Land);

            // set sea (A1..L2 in your earlier spec)
            from = new BoardCoord(1, 1);
            to = new BoardCoord(2, 12);
            SetTerrainRange(from, to, Terrains.Sea);

            // seaside ranges
            from = new BoardCoord(3, 1);
            to = new BoardCoord(3, 12);
            SetTerrainRange(from, to, Terrains.Coast);

            // riverside ranges
            from = new BoardCoord(3, 6);
            to = new BoardCoord(11, 7);
            SetTerrainRange(from, to, Terrains.Riverside);

            // shallow ranges
            from = new BoardCoord(6, 6);
            to = new BoardCoord(6, 7);
            SetTerrainRange(from, to, Terrains.Shallow);

            from = new BoardCoord(8, 6);
            to = new BoardCoord(8, 7);
            SetTerrainRange(from, to, Terrains.Shallow);
        }
        #region Conversion Methods
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
            return (Vector3Int)position + offsetPos;
        }
        public Vector3Int BoardCoordToCell(string label)
        {
            if (!TryBoardLabelToCoord(label, out BoardCoord coord))
                throw new ArgumentException("Label must be in the format of a letter followed by a number (e.g., A1, B12).");
            return BoardCoordToCell(coord);
        }
        public Vector3 BoardCoordToWorld(BoardCoord position) => grid.CellToWorld(BoardCoordToCell(position)) + new Vector3(0.5f, 0.5f, 0f);
        public Vector3 BoardCoordToWorld(string label) => grid.CellToWorld(BoardCoordToCell(label)) + new Vector3(0.5f, 0.5f, 0f);
        public bool TryWorldToBoardCoord(Vector3 worldPos, out BoardCoord coord)
        {
            Vector3Int cellPos = grid.WorldToCell(worldPos);
            return TryCellToBoardCoord(cellPos, out coord);
        }
        public bool TryCellToBoardCoord(Vector3Int cellPos, out BoardCoord coord)
        {
            Vector3Int offsetPos = Vector3Int.RoundToInt(transform.position) + new Vector3Int(offset.x, offset.y, 0);

            coord = new BoardCoord(
                cellPos.x - offsetPos.x,
                cellPos.y - offsetPos.y
            );

            return IsInBoard(coord);
        }

        #endregion
        public void SetTerrainRange(BoardCoord from, BoardCoord to, Terrains type)
        {
            for (int x = Math.Min(from.x, to.x); x <= Math.Max(from.x, to.x); x++)
            {
                for (int y = Math.Min(from.y, to.y); y <= Math.Max(from.y, to.y); y++)
                {
                    var pos = new BoardCoord(x, y);
                    if (IsInBoard(pos)) terrainMap[pos] = type;
                }
            }
        }
        public bool TryGetTerrain(BoardCoord coord, out Terrains type)
        {
            type = default;
            if (!IsInBoard(coord)) return false;
            return terrainMap.TryGetValue(coord, out type);
        }
        public bool TryGetPiece(BoardCoord coord, out BasePiece piece)
        {
            piece = null;
            if (!IsInBoard(coord)) return false;
            return pieces.TryGetValue(coord, out piece);
        }

        [Header("Debug Visualization")]
        [SerializeField] bool showCoordinates = true;
        [SerializeField] float labelSize = 0.4f;
        [SerializeField] Color labelColor = Color.yellow;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!showCoordinates || grid == null) return;

            // Lưu lại màu cũ
            var oldColor = Handles.color;
            Handles.color = labelColor;

            // Vẽ tọa độ ngang (A-K)
            for (int x = 1; x <= boardSize.width; x++)
            {
                var pos = new BoardCoord(x, 1);
                var worldPos = BoardCoordToWorld(pos);
                worldPos.y -= 0.7f; // Dịch xuống dưới một chút
            
                // Convert số thành chữ cái (1->A, 2->B,...)
                string label = ((char)('A' + x - 1)).ToString();
                Handles.Label(worldPos, label, CreateLabelStyle());
            }

            // Vẽ tọa độ dọc (1-12) 
            for (int y = 1; y <= boardSize.height; y++)
            {
                var pos = new BoardCoord(1, y);
                var worldPos = BoardCoordToWorld(pos);
                worldPos.x -= 0.7f; // Dịch sang trái một chút
            
                Handles.Label(worldPos, y.ToString(), CreateLabelStyle());
            }

            // Khôi phục màu
            Handles.color = oldColor;
        }

        private GUIStyle CreateLabelStyle()
        {
            var style = new GUIStyle();
            style.normal.textColor = labelColor;
            style.fontSize = Mathf.RoundToInt(labelSize * 20);
            style.alignment = TextAnchor.MiddleCenter;
            return style;
        }
#endif
    }
}
