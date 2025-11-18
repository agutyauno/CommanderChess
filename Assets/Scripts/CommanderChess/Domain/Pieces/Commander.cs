using System.Collections.Generic;
using CommanderChess.Services;

namespace CommanderChess.Domain
{
    public class Commander : BasePiece
    {
        public override PieceType Type => PieceType.Commander;

        /// <summary>
        /// Commander cannot move through enemy Commander Zone
        /// But can still board with allies even in enemy zone
        /// </summary>
        protected override void CalculateMoves()
        {
            cachedMoves.Clear();

            // Get enemy commander zone
            var enemyCommanderZone = zoneProvider.GetCommanderZone();
            var enemyZoneCoords = enemyCommanderZone?.GetZoneByEnemyTeam(Team);

            // Default: Straight moves
            if (canMoveStraight && straightMoveRange > 0)
            {
                foreach (var dir in straightDirs)
                {
                    AddCommanderMovesInDirection(dir, straightMoveRange, enemyZoneCoords);
                }
            }

            // Default: Diagonal moves
            if (canMoveDiagonal && diagonalMoveRange > 0)
            {
                foreach (var dir in diagonalDirs)
                {
                    AddCommanderMovesInDirection(dir, diagonalMoveRange, enemyZoneCoords);
                }
            }
        }

        /// <summary>
        /// Special move calculation for Commander - stops before enemy Commander Zone
        /// But allows boarding even in enemy zone
        /// </summary>
        private void AddCommanderMovesInDirection(BoardCoord dir, int maxRange, HashSet<BoardCoord> enemyZone)
        {
            for (int distance = 1; distance <= maxRange; distance++)
            {
                var targetPos = Position + (dir * distance);

                // Check if entering enemy commander zone (not allowed for normal movement)
                bool inEnemyZone = enemyZone != null && enemyZone.Contains(targetPos);

                // Check terrain (but can ignore for boarding)
                bool terrainAllowed = IsTerrainAllowed(targetPos);

                if (cachedMoves.Contains(targetPos))
                    continue;

                if (board.Pieces.TryGetValue(targetPos, out BasePiece occupant))
                {
                    bool isAlly = occupant.Team == Team;
                    bool thisPieceIsCarrier = AllowedCarryTypes.Contains(occupant.Type);
                    bool carryable = occupant.AllowedCarryTypes.Contains(Type) || thisPieceIsCarrier;

                    // Boarding is allowed even in enemy zone or bad terrain
                    if (isAlly && ((thisPieceIsCarrier && canCarryOthers) || (carryable && occupant.CanCarryOthers)))
                    {
                        cachedMoves.Add(targetPos);
                    }

                    // Blocked by piece
                    if (moveCanBeBlocked) break;
                    continue;
                }

                // Normal movement - must have good terrain AND not in enemy zone
                if (terrainAllowed && !inEnemyZone)
                {
                    cachedMoves.Add(targetPos);
                }
            }
        }
    }
}
