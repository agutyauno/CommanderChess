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

        void AddMoves((int dx, int dy) dir, int maxRange)
        {
            for (int distance = 1; distance <= maxRange; distance++)
            {
                var targetPos = new BoardCoord(Position.x + dir.dx * distance, Position.y + dir.dy * distance);

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
        
        bool ValidateMove(BoardCoord targetPos, (int dx, int dy) dir)
        {
            if (!board.IsInBoard(targetPos) || !IsTerrainAllowed(targetPos) || cachedMoves.Contains(targetPos))
                return false;
            board.TryGetTerrain(targetPos, out Terrains terrain);
            if (terrain == Terrains.Riverside)
            {
                // tính trước 2 bước
                var nextPos1 = new BoardCoord(targetPos.x + dir.dx, targetPos.y + dir.dy);
                var nextPos2 = new BoardCoord(nextPos1.x + dir.dx, nextPos1.y + dir.dy);

                // nếu 2 bước tiếp theo không phải là river thì không được đi qua riverside
                board.TryGetTerrain(nextPos1, out Terrains terrain1);
                board.TryGetTerrain(nextPos2, out Terrains terrain2);
                if ( terrain1 == Terrains.Riverside)
                {
                    if (terrain2 != Terrains.Riverside)
                        return false;
                }
            }
            return true;
        }

    }
}