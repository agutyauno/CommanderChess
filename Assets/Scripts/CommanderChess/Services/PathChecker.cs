using System.Collections.Generic;
using VContainer;
using CommanderChess.Domain;
using UnityEngine;

namespace CommanderChess.Services
{
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

            // Sử dụng operator - để tính delta
            var delta = to - from;
            int dx = delta.x;
            int dy = delta.y;

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

            // Generate path từng bước - sử dụng direction vector
            var direction = new BoardCoord(stepX, stepY);
            var current = from;
            
            for (int i = 0; i < steps; i++)
            {
                // Sử dụng operator + để di chuyển
                current = current + direction;
            
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

        public PathCheckResult CheckPath(BasePiece piece, BoardCoord from, BoardCoord to)
        {
            return CheckPath(piece, GeneratePath(from, to));
        }

        public PathCheckResult CheckPath(BasePiece piece, List<BoardCoord> path)
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

            if (!piece.IsHero)
            {
                // Lấy tất cả vùng nguy hiểm cho piece này từ DangerZoneProvider
                var dangerZone = zoneProvider.GetDangerZonesForPiece(piece);
                if (dangerZone == null) return result;
                var dangerPos = dangerZone.GetZoneByEnemyTeam(piece.Team);
    
                // Kiểm tra từng vị trí trên path
                bool crossingDanger = false;
            
                for (int i = 0; i < path.Count; i++)
                {
                    var position = path[i];
                    bool isDestination = i == path.Count - 1;
                    if (dangerPos.Contains(position))
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
            }
            return result;
        }

        /// <summary>
        /// Kiểm tra xem path có đi qua một vị trí cụ thể không
        /// </summary>
        public bool PathPassesThrough(List<BoardCoord> path, BoardCoord position)
        {
            return path.Contains(position);
        }
    }
}