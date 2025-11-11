namespace CommanderChess.Domain
{
    public class Navy : BasePiece
    {
        public override PieceType Type => PieceType.Navy;

        protected override void CalculateMoves()
        {
            // Straight moves
            if (canMoveStraight && straightMoveRange > 0)
            {
                foreach (var dir in straightDirs)
                {
                    AddMoves(dir, straightMoveRange);
                }
            }

            // Diagonal moves
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

                // Out of bounds or terrain không hợp lệ
                if (!IsTerrainAllowed(targetPos))
                    break;

                // Skip nếu đã có trong cache
                if (cachedMoves.Contains(targetPos))
                    continue;

                // Evaluate position
                var (shouldAdd, shouldBreak) = EvaluatePosition(targetPos);
                bool isDiagonal = dir.x != 0 && dir.y != 0;
                if (board.TryGetTerrain(targetPos, out var terrain) && terrain != Terrains.Sea && isDiagonal)
                {
                    var nextPos = targetPos + dir;
                    bool ok = board.TryGetTerrain(nextPos, out var nextTerrain);
                    if (ok && (terrain == Terrains.Coast && nextTerrain == Terrains.Riverside || 
                               terrain == Terrains.Riverside && nextTerrain == Terrains.Coast))
                    {
                        shouldBreak = true;
                    }
                }

                if (shouldAdd)
                    cachedMoves.Add(targetPos);

                if (shouldBreak)
                    break;
            }
        }

        /// <summary>
        /// Đánh giá vị trí có thể di chuyển được không
        /// </summary>
        private (bool shouldAdd, bool shouldBreak) EvaluatePosition(BoardCoord targetPos)
        {
            // Check occupant
            if (board.Pieces.TryGetValue(targetPos, out BasePiece occupant))
            {
                return EvaluateOccupiedPosition(occupant);
            }

            // Default: add và continue
            return (true, false);
        }

        /// <summary>
        /// Đánh giá vị trí có quân cờ
        /// </summary>
        private (bool shouldAdd, bool shouldBreak) EvaluateOccupiedPosition(BasePiece occupant)
        {
            bool isAlly = occupant.Team == Team;
            bool thisPieceIsCarrier = AllowedCarryTypes.Contains(occupant.Type);
            bool carryable = occupant.AllowedCarryTypes.Contains(Type) || thisPieceIsCarrier;
            bool canBoard = isAlly &&
                           ((thisPieceIsCarrier && canCarryOthers) ||
                            (carryable && occupant.CanCarryOthers));

            if (canBoard)
            {
                // Có thể boarding
                return (true, moveCanBeBlocked);
            }

            // Không thể boarding - stop nếu blocked
            return (false, moveCanBeBlocked);
        }

        /// <summary>
        /// Check nếu path từ start đến end có đi qua Land/Coast/Riverside
        /// </summary>
        private bool HasNonSeaInDiagonalPath(BoardCoord start, BoardCoord end)
        {
            // Get direction
            int dx = end.x - start.x;
            int dy = end.y - start.y;

            // Normalize direction
            int stepX = dx == 0 ? 0 : (dx > 0 ? 1 : -1);
            int stepY = dy == 0 ? 0 : (dy > 0 ? 1 : -1);

            // Check each position in path (bao gồm cả end, không bao gồm start)
            var current = start;
            bool isDiagonal = stepX != 0 && stepY != 0;
            if (!isDiagonal)
                return false; // Chỉ check diagonal path
            while (true)
            {
                // Move to next position
                current = new BoardCoord(current.x + stepX, current.y + stepY);

                // Check if current position is NOT Sea
                if (board.TryGetTerrain(current, out var terrain) && terrain != Terrains.Sea)
                {
                    return true; // Found non-Sea terrain in path
                }

                // Reached end - break after checking
                if (current == end)
                    break;
            }

            return false; // All positions are Sea
        }

        public override bool ShouldMoveToTarget(BoardCoord targetPos)
        {
            if (!board.TryGetTerrain(targetPos, out Terrains terrain))
                return DoMoveToTarget; // fallback to default

            if (terrain == Terrains.Land)
                return false;

            if (HasNonSeaInDiagonalPath(Position, targetPos))
                return false;
            
            return DoMoveToTarget;
        }
    }
}