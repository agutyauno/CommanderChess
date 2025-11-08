namespace CommanderChess.Domain
{
    public class Artillery : BasePiece
    {
        public override PieceType Type => PieceType.Artillery;

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
            for (int distance = 1; distance <= maxRange; distance++)
            {
                var targetPos = Position + (dir * distance);

                if (!ValidateMove(targetPos, dir))
                    break;

                if (board.Pieces.TryGetValue(targetPos, out BasePiece occupant))
                {
                    bool isAlly = occupant.Team == Team;
                    bool thisPieceIsCarrier = AllowedCarryTypes.Contains(occupant.Type);
                    bool carryable = occupant.AllowedCarryTypes.Contains(Type) || thisPieceIsCarrier;

                    if (isAlly && ((thisPieceIsCarrier && canCarryOthers) || (carryable && occupant.CanCarryOthers)))
                        cachedMoves.Add(targetPos);
                    if (canBeBlocked) break;
                    continue;
                }
                cachedMoves.Add(targetPos);
            }
        }
        
        bool ValidateMove(BoardCoord targetPos, BoardCoord dir)
        {
            if (!board.IsInBoard(targetPos) || !IsTerrainAllowed(targetPos) || cachedMoves.Contains(targetPos))
                return false;
            board.TryGetTerrain(targetPos, out Terrains terrain);
            if (terrain == Terrains.Riverside)
            {
                // tính trước nước đi
                var nextPos1 = targetPos + dir;
                var ok = board.TryGetTerrain(nextPos1, out Terrains nextTerrain1);

                if (ok && nextTerrain1 == Terrains.Riverside)
                {
                    return true;
                }

                if (ok && nextTerrain1 == Terrains.Shallow)
                {
                    var nextPos2 = nextPos1 + (dir * 2);
                    ok = board.TryGetTerrain(nextPos2, out Terrains nextTerrain2);
                    if (ok && nextTerrain2 == Terrains.Riverside)
                    {
                        return true;
                    }
                }
                return false;
            }
            return true;
        }

    }
}