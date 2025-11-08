using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CommanderChess.Domain;

namespace CommanderChess.Services
{
    /// <summary>
    /// class hỗ trợ xác định các vùng ring of fire (ROF)
    /// </summary>
    public class ROFZone : IZone
    {
        readonly Board board;

        public ROFZone(Board board)
        {
            this.board = board;
        }

        readonly Dictionary<Team, HashSet<BoardCoord>> ROFByTeam = new();
        readonly Dictionary<BoardCoord, List<BasePiece>> ROFSourcePieces = new();

        bool isDirty = true;

        public bool IsDirty => isDirty;

        public void MarkDirty()
        {
            isDirty = true;
        }

        /// <summary>
        /// tính toán lại toàn bọ tọa độ các vùng
        /// </summary>
        public void RecalculateAll()
        {
            ROFByTeam.Clear();
            ROFByTeam[Team.Red] = new HashSet<BoardCoord>();
            ROFByTeam[Team.Blue] = new HashSet<BoardCoord>();

            foreach (var kvp in board.Pieces)
            {
                var piece = kvp.Value;
                if (!piece.HadRingOfFire) continue;

                foreach (var zone in piece.RingOfFireZones)
                {
                    ROFByTeam[piece.Team].Add(zone);
                    if (ROFSourcePieces.ContainsKey(zone))
                    {
                        ROFSourcePieces[zone] = new();
                    }
                    if (!ROFSourcePieces.ContainsKey(zone))
                    {
                        ROFSourcePieces[zone] = new List<BasePiece>();
                    }
                    ROFSourcePieces[zone].Add(piece);
                }
            }
            isDirty = false;
        }

        public HashSet<BoardCoord> GetZone()
        {
            if (isDirty) RecalculateAll();
            HashSet<BoardCoord> zones = new();
            zones.UnionWith(ROFByTeam[Team.Red]);
            zones.UnionWith(ROFByTeam[Team.Blue]);
            return zones;
        }

        /// <summary>
        /// lấy danh sách tọa độ vùng Ring Of Fire của đội bất kì
        /// </summary>
        /// <param name="team"></param>
        /// <returns></returns>
        public HashSet<BoardCoord> GetZoneByTeam(Team team)
        {
            if (isDirty) RecalculateAll();
            return ROFByTeam[team];

        }
        public HashSet<BoardCoord> GetZoneByEnemyTeam(Team friendlyTeam)
        {
            var enemyTeam = friendlyTeam == Team.Red ? Team.Blue : Team.Red;
            return GetZoneByTeam(enemyTeam);
        }

        /// <summary>
        /// Lấy piece địch gần nhất tạo ra ring of fire tại vị trí (cho PathValidator)
        /// </summary>
        /// <param name="position"></param>
        /// <param name="team"></param>
        /// <returns></returns>
        public BasePiece GetZoneSourceAtPosition(BoardCoord position, Team team)
        {
            if (isDirty) RecalculateAll();

            if (!ROFSourcePieces.TryGetValue(position, out var sources))
            {
                return null;
            }

            // Lọc chỉ lấy enemy pieces
            var enemyThreats = sources.Where(p => p.Team != team).ToList();

            if (enemyThreats.Count == 0) return null;

            // Trả về piece gần nhất - SỬ DỤNG BoardCoord.ManhattanDistance()
            return enemyThreats
                .OrderBy(p => position.ManhattanDistance(p.Position))
                .FirstOrDefault();
        }

        /// <summary>
        /// Check if a path intersects with any ring of fire
        /// </summary>
        public List<BoardCoord> GetIntersectingZoneOnPath(List<BoardCoord> path, Team team)
        {
            var zones = GetZoneByTeam(team);
            var intersections = new List<BoardCoord>();

            foreach (var cell in path)
            {
                if (zones.Contains(cell))
                {
                    intersections.Add(cell);
                }
            }

            return intersections;
        }

        /// <summary>
        /// Kiểm tra xem một vị trí có nằm trong ROF của team không
        /// </summary>
        public bool IsInROF(BoardCoord position, Team team)
        {
            if (isDirty) RecalculateAll();
            return ROFByTeam[team].Contains(position);
        }

        /// <summary>
        /// Lấy tất cả các source pieces tại một vị trí
        /// </summary>
        public List<BasePiece> GetAllSourcesAtPosition(BoardCoord position)
        {
            if (isDirty) RecalculateAll();
            return ROFSourcePieces.TryGetValue(position, out var sources) 
                ? new List<BasePiece>(sources) 
                : new List<BasePiece>();
        }

        /// <summary>
        /// Kiểm tra xem piece có trong vùng ROF của enemy không
        /// </summary>
        public bool IsPieceInEnemyROF(BasePiece piece)
        {
            if (isDirty) RecalculateAll();
            var enemyTeam = piece.Team == Team.Red ? Team.Blue : Team.Red;
            return ROFByTeam[enemyTeam].Contains(piece.Position);
        }

        /// <summary>
        /// Lấy tất cả các vị trí trong ROF trong khoảng cách nhất định từ position
        /// </summary>
        public List<BoardCoord> GetROFZonesInRange(BoardCoord position, int range, Team team)
        {
            if (isDirty) RecalculateAll();
            var zones = ROFByTeam[team];
            
            return zones
                .Where(zone => position.ManhattanDistance(zone) <= range)
                .ToList();
        }
    }
}