namespace CommanderChess.Domain
{
    public class Engineer : BasePiece
    {
        public override PieceType Type => PieceType.Engineer;

        protected override void CalculateMoves()
        {
            // Update canCarryOthers based on current terrain
            UpdateCarryAbility();

            // Nếu đang mang quân khác → chỉ có thể vượt sông
            if (IsCarryingOthers())
            {
                CalculateCrossingRiverMoves();
                return;
            }

            // Nếu không mang → di chuyển bình thường
            base.CalculateMoves();
        }

        /// <summary>
        /// Update canCarryOthers based on current terrain
        /// </summary>
        private void UpdateCarryAbility()
        {
            if (board.TryGetTerrain(Position, out var terrain) && terrain == Terrains.Riverside)
            {
                canCarryOthers = true;
            }
            else
            {
                canCarryOthers = false;
            }
        }

        /// <summary>
        /// Check if Engineer is carrying other pieces
        /// </summary>
        private bool IsCarryingOthers()
        {
            var groupSide = carryingSystem.CountGroupSize(this);
            return groupSide > 1;
        }

        /// <summary>
        /// Calculate moves for crossing river (khi đang mang quân)
        /// Chỉ có thể di chuyển sang bờ bên kia
        /// </summary>
        private void CalculateCrossingRiverMoves()
        {
            // Check current position must be Riverside
            if (!board.TryGetTerrain(Position, out var currentTerrain) || 
                currentTerrain != Terrains.Riverside)
            {
                // Không phải riverside → không di chuyển được
                return;
            }

            // Determine which side of river we're on
            int currentRow = Position.y;
            bool isOnSouthSide = currentRow == 6; // Row 6 is south side
            bool isOnNorthSide = currentRow == 7; // Row 7 is north side

            if (!isOnSouthSide && !isOnNorthSide)
            {
                // Not on river edge → shouldn't happen but safety check
                return;
            }

            // Calculate target row (bờ bên kia)
            int targetRow = isOnSouthSide ? 7 : 6;

            // Check các hướng có thể vượt sông
            foreach (var dir in straightDirs)
            {
                CheckCrossingDirection(dir, targetRow);
            }
        }

        /// <summary>
        /// Check một hướng có thể vượt sông không
        /// </summary>
        private void CheckCrossingDirection(BoardCoord dir, int targetRow)
        {
            var targetPos = Position + dir;

            // Out of bounds
            if (!board.IsInBoard(targetPos))
                return;

            // Must be on target row (bờ bên kia)
            if (targetPos.y != targetRow)
                return;

            // Must be Riverside or Shallow
            if (!board.TryGetTerrain(targetPos, out var terrain))
                return;

            if (terrain != Terrains.Riverside && terrain != Terrains.Shallow)
                return;

            // Check occupant
            if (board.Pieces.TryGetValue(targetPos, out BasePiece occupant))
            {
                bool isAlly = occupant.Team == Team;
                bool thisPieceIsCarrier = AllowedCarryTypes.Contains(occupant.Type);
                bool carryable = occupant.AllowedCarryTypes.Contains(Type) || thisPieceIsCarrier;
                bool canBoard = isAlly && 
                               ((thisPieceIsCarrier && canCarryOthers) || 
                                (carryable && occupant.CanCarryOthers));

                if (canBoard)
                {
                    cachedMoves.Add(targetPos);
                }
                // Có occupant nhưng không thể board → không add
                return;
            }

            // Empty position → can move
            cachedMoves.Add(targetPos);
        }

        public override bool ShouldMoveToTarget(BoardCoord targetPos)
        {
            // Nếu đang mang quân và target không phải Riverside/Shallow
            if (IsCarryingOthers())
            {
                if (board.TryGetTerrain(targetPos, out var terrain))
                {
                    // Chỉ cho phép di chuyển tới Riverside hoặc Shallow
                    if (terrain == Terrains.Riverside || terrain == Terrains.Shallow)
                    {
                        return DoMoveToTarget;
                    }
                    
                    // Terrain khác → không di chuyển
                    return false;
                }
            }

            return base.ShouldMoveToTarget(targetPos);
        }
    }
}
