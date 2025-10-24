using System.Collections.Generic;
using UnityEngine;
using VContainer;

public enum PathResult
{
    None, GoThrough, Inside
}

public class PathChecker
{
    [Inject] readonly Board board;
    [Inject] ZoneProvider zoneProvider;

    public struct PathCheckResult
    {
        public PathResult Result;
        public List<BoardCoord> FullPath;
    }

    public List<BoardCoord> GeneratePath(BoardCoord from, BoardCoord to)
     {
        var path = new List<BoardCoord>();

        int dx = to.x - from.x;
        int dy = to.y - from.y;

        // Xác định hướng di chuyển
        int stepX = dx == 0 ? 0 : (dx > 0 ? 1 : -1);
        int stepY = dy == 0 ? 0 : (dy > 0 ? 1 : -1);

        // Kiểm tra xem có phải đường thẳng hợp lệ không
        if (!IsValidStraightLine(dx, dy))
        {
            return path; // Trả về path rỗng
        }

        // Tính số bước
        int steps = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy));

        // Generate path từng bước
        var current = from;
        for (int i = 0; i < steps; i++)
        {
            current = new BoardCoord(current.x + stepX, current.y + stepY);
            
            if (board.IsInBoard(current))
            {
                path.Add(current);
            }
            else
            {
                break; // Vượt ra ngoài board
            }
        }

        return path;
    }

    /// <summary>
    /// Kiểm tra xem có phải đường thẳng hợp lệ không
    /// Chỉ cho phép: ngang (dx≠0, dy=0), dọc (dx=0, dy≠0), chéo 45 (|dx|=|dy|)
    /// </summary>
    private bool IsValidStraightLine(int dx, int dy)
    {
        // Ngang: dy = 0, dx ≠ 0
        if (dy == 0 && dx != 0) return true;

        // Dọc: dx = 0, dy ≠ 0
        if (dx == 0 && dy != 0) return true;

        // Chéo 45: |dx| = |dy|
        if (Mathf.Abs(dx) == Mathf.Abs(dy) && dx != 0) return true;

        // Trường hợp đặc biệt: from == to
        if (dx == 0 && dy == 0) return false;

        return false;
    }

    public PathCheckResult CheckPath(Piece piece, BoardCoord from, BoardCoord to)
    {
        return CheckPath(piece, GeneratePath(from, to));
    }

    public PathCheckResult CheckPath(Piece piece, List<BoardCoord> path)
    {
        var result = new PathCheckResult
        {
            FullPath = new List<BoardCoord>(path),
            Result = PathResult.None
        };

        if (piece == null || path == null || path.Count == 0)
        {
            return result;
        }

        // Lấy tất cả vùng nguy hiểm cho piece này từ DangerZoneProvider
        var dangerZones = zoneProvider.GetDangerZonesForPiece(piece);

        // Kiểm tra từng vị trí trên path
        bool crossingDanger = false;
        
        for (int i = 0; i < path.Count; i++)
        {
            var position = path[i];
            bool isDestination = i == path.Count - 1;

            if (dangerZones.GetZoneByEnemyTeam(piece.Team).Contains(position))
            {
                // Xác định loại nguy hiểm
                if (isDestination)
                {
                    result.Result = PathResult.Inside;
                }
                else
                {
                    crossingDanger = true;
                }
            }
        }

        // Nếu không nằm trong danger zone nhưng có đi qua
        if (result.Result == PathResult.None && crossingDanger)
        {
            result.Result = PathResult.GoThrough;
        }
        return result;
    }
}
