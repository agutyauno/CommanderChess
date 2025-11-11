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
            bool isDiagonal = dir.x != 0 && dir.y != 0;

            for (int distance = 1; distance <= maxRange; distance++)
            {
                var targetPos = Position + (dir * distance);

                // Early exit: Out of bounds hoặc terrain không hợp lệ
                if (!IsTerrainAllowed(targetPos))
                    break;

                // Skip nếu đã có trong cached moves
                if (cachedMoves.Contains(targetPos))
                    continue;

                // Evaluate position và get flags
                var (shouldAdd, shouldBreak) = EvaluatePosition(targetPos, dir, isDiagonal);

                // Apply actions
                if (shouldAdd)
                    cachedMoves.Add(targetPos);

                if (shouldBreak)
                    break;
            }
        }

        /// <summary>
        /// Đánh giá một vị trí có thể di chuyển được không
        /// </summary>
        /// <returns>(shouldAdd: có add vào moves không, shouldBreak: có dừng loop không)</returns>
        private (bool shouldAdd, bool shouldBreak) EvaluatePosition(BoardCoord targetPos, BoardCoord dir, bool isDiagonal)
        {
            // Check 1: Có quân cờ tại target position
            if (board.Pieces.TryGetValue(targetPos, out BasePiece occupant))
            {
                return EvaluateOccupiedPosition(occupant);
            }

            // Check 2: Diagonal trên Shallow (check sau occupant, trước riverside)
            if (IsDiagonalOnShallow(targetPos, isDiagonal))
            {
                return (false, true);
            }

            // Check 3: Riverside terrain logic
            if (IsRiverside(targetPos))
            {
                return EvaluateRiversidePosition(targetPos, dir, isDiagonal);
            }

            // Default: Add move và tiếp tục
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
            else
            {
                // Không thể boarding
                return (false, moveCanBeBlocked);
            }
        }

        /// <summary>
        /// Đánh giá vị trí Riverside
        /// </summary>
        private (bool shouldAdd, bool shouldBreak) EvaluateRiversidePosition(BoardCoord targetPos, BoardCoord dir, bool isDiagonal)
        {
            var nextPos = targetPos + dir;
            bool hasNextTerrain = board.TryGetTerrain(nextPos, out var nextTerrain);

            if (!hasNextTerrain)
            {
                // Next position out of bounds - logic mặc định
                return (true, false);
            }

            if (isDiagonal)
            {
                return EvaluateRiversideDiagonal(nextPos, nextTerrain);
            }
            else
            {
                return EvaluateRiversideStraight(nextTerrain);
            }
        }

        /// <summary>
        /// Đánh giá Riverside khi di chuyển thẳng
        /// </summary>
        private (bool shouldAdd, bool shouldBreak) EvaluateRiversideStraight(Terrains nextTerrain)
        {
            bool isNextRiverside = nextTerrain == Terrains.Riverside;
            bool isNextLandOrCoast = nextTerrain == Terrains.Land || nextTerrain == Terrains.Coast;

            if (isNextRiverside)
            {
                // Tiếp tục di chuyển trên riverside
                return (true, false);
            }

            if (isNextLandOrCoast)
            {
                // Dừng lại, không thể vào Land/Coast
                return (false, true);
            }

            // Các terrain khác - add và tiếp tục (default behavior từ code cũ)
            return (true, false);
        }

        /// <summary>
        /// Đánh giá Riverside khi di chuyển chéo
        /// </summary>
        private (bool shouldAdd, bool shouldBreak) EvaluateRiversideDiagonal(BoardCoord nextPos, Terrains nextTerrain)
        {
            bool isNextRiverside = nextTerrain == Terrains.Riverside;
            bool isNextLandOrCoast = nextTerrain == Terrains.Land || nextTerrain == Terrains.Coast;
            bool isNextShallow = nextTerrain == Terrains.Shallow;
            bool isNextValid = IsTerrainAllowed(nextPos);
            bool isInRiverside = board.TryGetTerrain(Position, out var currentTerrain) && 
                                 currentTerrain == Terrains.Riverside;

            if (isNextRiverside)
            {
                // Tiếp tục di chuyển trên riverside
                return (true, false);
            }

            if (isNextLandOrCoast)
            {
                // Dừng lại, không thể vào Land/Coast
                return (false, true);
            }

            if (isNextShallow)
            {
                // Có thể vào Shallow nhưng phải dừng
                return (true, true);
            }

            if (!isNextValid && isInRiverside)
            {
                // Terrain không hợp lệ khi đang ở riverside
                return (false, true);
            }

            // Default cho diagonal - add và tiếp tục
            return (true, false);
        }

        /// <summary>
        /// Check có phải đang di chuyển chéo trên Shallow không
        /// </summary>
        private bool IsDiagonalOnShallow(BoardCoord targetPos, bool isDiagonal)
        {
            return isDiagonal && 
                   board.TryGetTerrain(targetPos, out var terrain) && 
                   terrain == Terrains.Shallow;
        }

        /// <summary>
        /// Check có phải terrain Riverside không
        /// </summary>
        private bool IsRiverside(BoardCoord targetPos)
        {
            return board.TryGetTerrain(targetPos, out var terrain) && 
                   terrain == Terrains.Riverside;
        }
    }
}