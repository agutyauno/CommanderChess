using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class PieceController
{
    [Inject] private readonly Board board;
    private readonly Dictionary<BoardCoord, Piece> pieces;

    public PieceController(Board board)
    {
        this.board = board;
        pieces = new Dictionary<BoardCoord, Piece>();
    }

    // Thêm quân cờ vào vị trí trên bàn
    public bool PlacePiece(Piece piece, BoardCoord coord)
    {
        if (!ValidatePlacement(piece, coord)) return false;

        pieces[coord] = piece;
        piece.Position = coord;
        piece.transform.position = board.BoardCoordToWorld(coord);
        piece.RecalculateCache();
        return true;
    }

    // Di chuyển quân cờ
    public bool MovePiece(BoardCoord from, BoardCoord to)
    {
        if (!pieces.TryGetValue(from, out var piece)) return false;
        if (!ValidateMove(piece, from, to)) return false;

        // Di chuyển piece và tất cả quân nó đang mang
        pieces.Remove(from);
        pieces[to] = piece;
        piece.Position = to;
        piece.transform.position = board.BoardCoordToWorld(to);

        // Cập nhật vị trí của các quân đang được mang
        UpdateCarriedPiecesPosition(piece);
        
        piece.RecalculateCache();
        return true;
    }

    // Tấn công
    public bool Capture(BoardCoord attackerPos, BoardCoord targetPos)
    {
        if (!ValidateCapture(attackerPos, targetPos)) return false;

        var attacker = pieces[attackerPos];
        var target = pieces[targetPos];

        // Xóa target và các quân nó đang mang khỏi bàn cờ
        RemovePieceAndCarried(target);

        // Di chuyển attacker nếu cần
        if (attacker.DoMoveToTarget)
        {
            MovePiece(attackerPos, targetPos);
        }

        target.OnCaptured();
        attacker.RecalculateCache();
        return true;
    }

    // Helper methods
    private bool ValidatePlacement(Piece piece, BoardCoord coord)
    {
        if (piece == null || !board.IsInBoard(coord)) return false;
        if (pieces.ContainsKey(coord)) return false;
        return true;
    }

    private bool ValidateMove(Piece piece, BoardCoord from, BoardCoord to)
    {
        if (!board.IsInBoard(to)) return false;
        if (pieces.ContainsKey(to)) return false;
        if (!piece.PossibleMoves.Contains(to)) return false;
        return true;
    }

    private bool ValidateCapture(BoardCoord attackerPos, BoardCoord targetPos)
    {
        if (!pieces.TryGetValue(attackerPos, out var attacker)) return false;
        if (!pieces.TryGetValue(targetPos, out var target)) return false;
        if (!attacker.PossibleAttacks.Contains(targetPos)) return false;
        if (attacker.Team == target.Team) return false;
        return true;
    }

    private void UpdateCarriedPiecesPosition(Piece carrier)
    {
        foreach (var carried in carrier.CarryingPieces)
        {
            if (carried != null)
            {
                carried.Position = carrier.Position;
                carried.transform.position = carrier.transform.position;
                UpdateCarriedPiecesPosition(carried); // Đệ quy cho các quân được mang bởi carried
            }
        }
    }

    private void RemovePieceAndCarried(Piece piece)
    {
        // Xóa piece khỏi bàn cờ
        pieces.Remove(piece.Position);

        // Xóa đệ quy tất cả các quân đang được mang
        foreach (var carried in piece.CarryingPieces)
        {
            if (carried != null)
            {
                RemovePieceAndCarried(carried);
            }
        }
    }

    // Getter methods
    public bool TryGetPiece(BoardCoord coord, out Piece piece)
    {
        return pieces.TryGetValue(coord, out piece);
    }

    public IEnumerable<KeyValuePair<BoardCoord, Piece>> GetAllPieces()
    {
        return pieces;
    }
}