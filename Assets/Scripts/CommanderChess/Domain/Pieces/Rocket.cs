namespace CommanderChess.Domain
{
    public class Rocket : BasePiece
    {
        public override PieceType Type => PieceType.Rocket;

        protected override void CalculateMoves()
        {
            // Default: Straight moves
            if (canMoveStraight && straightMoveRange > 0)
            {
                foreach (var dir in straightDirs)
                {
                    AddMoves(dir, straightMoveRange);
                }
            }

            // Default: Diagonal moves
            if (canMoveDiagonal && diagonalMoveRange > 0)
            {
                foreach (var dir in diagonalDirs)
                {
                    AddMoves(dir, diagonalMoveRange);
                }
            }
        }

        void AddMoves(BoardCoord dir, int maxRange)
        {
            bool isDiagonal = dir.x != 0 && dir.y != 0;
            for (int distance = 1; distance <= maxRange; distance++)
            {
                var targetPos = Position + (dir * distance);
                if (!board.IsInBoard(targetPos) || !IsTerrainAllowed(targetPos))
                    break;
                if (cachedMoves.Contains(targetPos)) continue;
                if (board.Pieces.TryGetValue(targetPos, out BasePiece occupant))
                {
                    bool isAlly = occupant.Team == Team;
                    bool thisPieceIsCarrier = AllowedCarryTypes.Contains(occupant.Type);
                    bool carryable = occupant.AllowedCarryTypes.Contains(Type) || thisPieceIsCarrier;

                    if (isAlly && ((thisPieceIsCarrier && canCarryOthers) || (carryable && occupant.CanCarryOthers)))
                    {
                        if (moveCanBeBlocked)
                        {
                            cachedMoves.Add(targetPos);
                            break;
                        }
                    }

                    if (moveCanBeBlocked) break;
                    continue;
                }

                var ok = board.TryGetTerrain(targetPos, out var targetTerrain);
                if (ok && targetTerrain == Terrains.Riverside)
                {
                    var nextPos1 = targetPos + dir;
                    ok = board.TryGetTerrain(nextPos1, out var terrain1);
                    if (!isDiagonal)
                    {
                        if (ok && terrain1 != Terrains.Land)
                        {
                            cachedMoves.Add(targetPos);
                            continue;
                        }
                        else if (!ok || terrain1 == Terrains.Sea)
                        {
                            cachedMoves.Add(targetPos);
                            break;
                        }
                    }
                    else if (!ok || terrain1 == Terrains.Shallow)
                    {
                        cachedMoves.Add(targetPos);
                        break;
                    }
                    break;
                }
                else if (isDiagonal && ok && targetTerrain == Terrains.Shallow)
                {
                    break;
                }
                cachedMoves.Add(targetPos);
            }
        }
    }
}
