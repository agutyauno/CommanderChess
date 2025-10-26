using System.Collections.Generic;

public interface IZone
{
    bool IsDirty { get; }
    void MarkDirty();
    void RecalculateAll();
    HashSet<BoardCoord> GetZone();
    HashSet<BoardCoord> GetZoneByTeam(Team team);
    HashSet<BoardCoord> GetZoneByEnemyTeam(Team FriendlyTeam);
    Piece GetZoneSourceAtPosition(BoardCoord position, Team team);
    List<BoardCoord> GetIntersectingZoneOnPath(List<BoardCoord> path, Team team);
}
