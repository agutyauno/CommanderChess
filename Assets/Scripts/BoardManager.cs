using System.Collections.Generic;
using UnityEngine;
public class BoardManager : MonoBehaviour
{
    [SerializeField] Board board;
    [SerializeField] List<Piece> pieces = new();

    void Awake()
    {
        SetUpPieces();
        SetUpBoard();
    }

    //đặt địa hình ban đầu
    public void SetUpBoard()
    {
        bool ok;
        BoardCoord from, to;

        // set land
        ok = BoardCoord.TryParseLabel("A1", out from);
        ok &= BoardCoord.TryParseLabel("L11", out to);
        if (!ok) { Debug.LogError("Failed parsing A1..L11"); return; }
        board.SetZoneRange(from, to, PositionType.Land);

        // set sea (A1..L2 in your earlier spec)
        ok = BoardCoord.TryParseLabel("A1", out from);
        ok &= BoardCoord.TryParseLabel("L2", out to);
        if (!ok) { Debug.LogError("Failed parsing A1..L2"); return; }
        board.SetZoneRange(from, to, PositionType.Sea);

        // seaside ranges
        ok = BoardCoord.TryParseLabel("A3", out from);
        ok &= BoardCoord.TryParseLabel("L3", out to);
        if (!ok) Debug.LogError("Failed parsing A3..L3");
        else board.SetZoneRange(from, to, PositionType.Seaside);

        ok = BoardCoord.TryParseLabel("F3", out from);
        ok &= BoardCoord.TryParseLabel("G5", out to);
        if (!ok) Debug.LogError("Failed parsing F3..G5");
        else board.SetZoneRange(from, to, PositionType.Seaside);

        ok = BoardCoord.TryParseLabel("F7", out from);
        ok &= BoardCoord.TryParseLabel("G7", out to);
        if (!ok) Debug.LogError("Failed parsing F7..G7");
        else board.SetZoneRange(from, to, PositionType.Seaside);

        ok = BoardCoord.TryParseLabel("F9", out from);
        ok &= BoardCoord.TryParseLabel("G11", out to);
        if (!ok) Debug.LogError("Failed parsing F9..G11");
        else board.SetZoneRange(from, to, PositionType.Seaside);

        // shallow ranges
        ok = BoardCoord.TryParseLabel("F6", out from);
        ok &= BoardCoord.TryParseLabel("G6", out to);
        if (!ok) Debug.LogError("Failed parsing F6..G6");
        else board.SetZoneRange(from, to, PositionType.Shallow);

        ok = BoardCoord.TryParseLabel("F8", out from);
        ok &= BoardCoord.TryParseLabel("G8", out to);
        if (!ok) Debug.LogError("Failed parsing F8..G8");
        else board.SetZoneRange(from, to, PositionType.Shallow);

        // log zone map (after setup)
        foreach (var kvp in board.zoneMap)
            Debug.Log($"Position: {kvp.Key.ToLabel()}, Type: {kvp.Value}");
    }

    //xép cờ lên bàn
    void SetUpPieces()
    {
        foreach (var piece in pieces)
        {
            var pos = piece.InitialPosition;
            if (board.IsInBoard(pos))
            {
                piece.transform.position = board.Grid.CellToWorld(board.BoardCoordToCell(pos));
            }
            else
            {
                Debug.LogError($"Piece {piece.Type} initial position {pos} is out of board range.");
            }
        }
    }
}
