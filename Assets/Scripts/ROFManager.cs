using System.Collections.Generic;
using System.Linq;
using VContainer;
using UnityEngine;

public class ROFManager
{
    [Inject] readonly Board board;
    readonly Dictionary<Team, HashSet<BoardCoord>> ROFByTeam = new();
    readonly Dictionary<BoardCoord, List<Piece>> ROFSourcePieces = new();

    bool isDirty;

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
                ROFSourcePieces[zone].Add(piece);
            }
        }
        isDirty = false;
    }
    
    /// <summary>
    /// lấy danh sách tọa độ vùng Ring Of Fire của đội bất kì
    /// </summary>
    /// <param name="team"></param>
    /// <returns></returns>
    public HashSet<BoardCoord> GetROFByTeam(Team team)
    {
        if (isDirty) RecalculateAll();
        return ROFByTeam[team];
    }

    /// <summary>
    /// lấy danh sách tọa độ vùng Ring Of Fire của đội đối thủ
    /// </summary>
    /// <param name="friendlyTeam"></param>
    /// <returns></returns>
    public HashSet<BoardCoord> GetROFByEnemy(Team friendlyTeam)
    {
        var enemyTeam = friendlyTeam == Team.Red ? Team.Blue : Team.Red;
        return GetROFByTeam(enemyTeam);
    }

    /// <summary>
    /// Lấy piece địch gần nhất tạo ra ring of fire tại vị trí (cho PathValidator)
    /// </summary>
    /// <param name="position"></param>
    /// <param name="friendlyTeam"></param>
    /// <returns></returns>
    public Piece GetEnemyROFAtPosition(BoardCoord position, Team friendlyTeam)
    {
        if (isDirty) RecalculateAll();

        if (!ROFSourcePieces.TryGetValue(position, out var sources))
        {
            return null;
        }

        // Lọc chỉ lấy enemy pieces
        var enemyThreats = sources.Where(p => p.Team != friendlyTeam).ToList();

        if (enemyThreats.Count == 0) return null;

        // Trả về piece gần nhất
        return enemyThreats
            .OrderBy(p => ManhattanDistance(p.Position, position))
            .FirstOrDefault();
    }

    /// <summary>
    /// Check if a path intersects with any ring of fire
    /// </summary>
    public List<BoardCoord> GetIntersectingROFOnPath(
        List<BoardCoord> path,
        Team friendlyTeam)
    {
        var enemyZones = GetROFByEnemy(friendlyTeam);
        var intersections = new List<BoardCoord>();

        foreach (var cell in path)
        {
            if (enemyZones.Contains(cell))
            {
                intersections.Add(cell);
            }
        }

        return intersections;
    }
    
    // helper method
    private int ManhattanDistance(BoardCoord a, BoardCoord b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
}
