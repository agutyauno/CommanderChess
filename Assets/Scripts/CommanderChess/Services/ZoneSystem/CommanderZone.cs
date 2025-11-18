
using System.Collections.Generic;
using System.Linq;
using CommanderChess.Domain;
using Unity.VisualScripting;

namespace CommanderChess.Services
{
    public class CommanderZone : IZone
    {
        Board board;
        CarryingSystem carryingSystem;

        public CommanderZone(Board board, CarryingSystem carryingSystem)
        {
            this.board = board;
            this.carryingSystem = carryingSystem;
        }

        bool isDirty = true;

        Dictionary<Team, HashSet<BoardCoord>> zoneByTeam = new();
        public bool IsDirty => isDirty;

        public List<BoardCoord> GetIntersectingZoneOnPath(List<BoardCoord> path, Team team)
        {
            if (isDirty) RecalculateAll();
            var intersectingCoords = new List<BoardCoord>();
            foreach (var coord in path)
            {
                if (zoneByTeam[team].Contains(coord))
                {
                    intersectingCoords.Add(coord);
                }
            }
            return intersectingCoords;
        }

        public HashSet<BoardCoord> GetZone()
        {
            if (isDirty) RecalculateAll();
            var allCoords = new HashSet<BoardCoord>();
            allCoords.UnionWith(zoneByTeam[Team.Red]);
            allCoords.UnionWith(zoneByTeam[Team.Blue]);
            return allCoords;
        }

        public HashSet<BoardCoord> GetZoneByEnemyTeam(Team FriendlyTeam)
        {
            var enemyTeam = FriendlyTeam == Team.Red ? Team.Blue : Team.Red;
            return GetZoneByTeam(enemyTeam);
        }

        public HashSet<BoardCoord> GetZoneByTeam(Team team)
        {
            if (isDirty) RecalculateAll();
            return zoneByTeam[team];
        }

        public BasePiece GetZoneSourceAtPosition(BoardCoord position, Team team)
        {
            // This method is deprecated for CommanderZone
            // Commander zones are based on movement range, not specific source positions
            return null;
        }

        public void MarkDirty()
        {
            isDirty = true;
        }

        public void RecalculateAll()
        {
            // Clear existing zones
            zoneByTeam.Clear();
            zoneByTeam[Team.Red] = new HashSet<BoardCoord>();
            zoneByTeam[Team.Blue] = new HashSet<BoardCoord>();

            // Find all commanders (on board and carried)
            var allCommanders = new List<BasePiece>();

            // 1. Get commanders on board
            foreach (var kvp in board.Pieces)
            {
                var piece = kvp.Value;
                if (piece.Type == BasePiece.PieceType.Commander)
                {
                    allCommanders.Add(piece);
                }
            }

            // 2. Get carried commanders (check all pieces on board for their carried pieces)
            foreach (var kvp in board.Pieces)
            {
                var carrier = kvp.Value;
                var carriedPieces = carryingSystem.GetAllCarriedPieces(carrier);
                
                foreach (var carried in carriedPieces)
                {
                    if (carried.Type == BasePiece.PieceType.Commander)
                    {
                        allCommanders.Add(carried);
                    }
                }
            }

            // 3. Calculate zones based on commander's PossibleMoves
            foreach (var commander in allCommanders)
            {
                // Add all possible moves of this commander to the zone
                foreach (var move in commander.PossibleMoves)
                {
                    zoneByTeam[commander.Team].Add(move);
                }
            }

            isDirty = false;
        }
    }
}
