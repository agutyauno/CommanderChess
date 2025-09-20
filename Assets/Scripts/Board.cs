using System;
using UnityEngine;

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

    BoardManager boardManager;
    Size boardSize = new(11, 12);
    [SerializeField] Grid grid;

    public Grid Grid { get => grid; }
    public Size BoardSize { get => boardSize; }

    #region IsInBoard Methods
    public bool IsInBoard(BoardCoord position)
    {
        return position.x >= 0 && position.x < boardSize.width && position.y >= 0 && position.y < boardSize.height;
    }
    public bool IsInBoard(Vector3Int position)
    {
        return IsInBoard((BoardCoord)position);
    }
    public bool IsInBoard(Vector2Int position)
    {
        return IsInBoard((BoardCoord)position);
    }
    public bool IsInBoard(int x, int y)
    {
        return IsInBoard(new BoardCoord(x, y));
    }
    #endregion
    #region BoardCoordToLabel Methods
    public string BoardCoordToLabel(BoardCoord position)
    {
        if (!IsInBoard(position))
            throw new ArgumentOutOfRangeException("Position is out of board range.");
        return position.ToLabel();
    }
    public string BoardCoordToLabel(Vector3Int position)
    {
        return BoardCoordToLabel((BoardCoord)position);
    }
    public string BoardCoordToLabel(Vector2Int position)
    {
        return BoardCoordToLabel((BoardCoord)position);
    }
    public string BoardCoordToLabel(int x, int y)
    {
        return BoardCoordToLabel(new BoardCoord(x, y));
    }
    #endregion
    #region BoardLabelToCoord Methods
    public bool TryBoardLabelToCoord(string label, out BoardCoord coord)
    {
        if (!BoardCoord.TryParseLabel(label, out coord))
            throw new ArgumentException("Label must be in the format of a letter followed by a number (e.g., A1, B12).");

        if (!IsInBoard(coord))
            throw new ArgumentOutOfRangeException("Converted position is out of board range.");
        return true;
    }
    public bool TryBoardLabelToCoord(string label, out Vector3Int coord)
    {
        TryBoardLabelToCoord(label, out BoardCoord tempCoord);
        coord = (Vector3Int)tempCoord;
        return true;
    }
    public bool TryBoardLabelToCoord(string label, out Vector2Int coord)
    {
        TryBoardLabelToCoord(label, out Vector3Int tempCoord);
        coord = (Vector2Int)tempCoord;
        return true;
    }
    #endregion

}
public enum PositionType
{
    Sea, Land, Shallow
}