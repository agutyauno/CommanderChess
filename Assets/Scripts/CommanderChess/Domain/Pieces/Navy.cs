namespace CommanderChess.Domain
{
    public class Navy : BasePiece
    {
        public override PieceType Type => PieceType.Navy;

        protected override void CalculateAttacks()
        {
            // Navy có thể tấn công quân trên bờ (Coast) và trên biển
            // Default: Straight attacks
            if (canAttackStraight && straightAttackRange > 0)
            {
                foreach (var dir in straightDirs)
                {
                    AddNavyAttacksInDirection(dir, straightAttackRange);
                }
            }

            // Default: Diagonal attacks
            if (canAttackDiagonal && diagonalAttackRange > 0)
            {
                foreach (var dir in diagonalDirs)
                {
                    AddNavyAttacksInDirection(dir, diagonalAttackRange);
                }
            }
        }

        /// <summary>
        /// Thêm các nước tấn công của Navy
        /// Navy có thể tấn công:
        /// - Quân trên biển (Sea, Shallow) -> DoMoveToTarget = true (thế chỗ)
        /// - Quân trên bờ (Coast) -> DoMoveToTarget = false (đứng yên)
        /// </summary>
        void AddNavyAttacksInDirection(BoardCoord dir, int maxRange)
        {
            for (int distance = 1; distance <= maxRange; distance++)
            {
                var targetPos = Position + (dir * distance);

                if (!board.IsInBoard(targetPos))
                    break;

                if (board.Pieces.TryGetValue(targetPos, out BasePiece occupant))
                {
                    bool isEnemy = occupant.Team != Team;

                    if (isEnemy && !cachedAttacks.Contains(targetPos))
                    {
                        cachedAttacks.Add(targetPos);

                        // Kiểm tra terrain của target để set DoMoveToTarget
                        board.TryGetTerrain(targetPos, out Terrains targetTerrain);

                        // Nếu target ở trên bờ (Coast) -> không di chuyển tới đó
                        // Nếu target ở trên biển (Sea, Shallow) -> di chuyển tới đó
                        // 
                        // NOTE: DoMoveToTarget được set trong PieceData, nhưng Navy cần logic đặc biệt
                        // Vì vậy ta cần xử lý riêng trong CaptureCommand
                    }

                    if (attackCanBeBlocked)
                        break;
                    continue;
                }
            }
        }

        /// <summary>
        /// Kiểm tra xem Navy có nên di chuyển tới vị trí target không
        /// - Target trên Sea/Shallow -> TRUE (thế chỗ)
        /// - Target trên Coast -> FALSE (đứng yên, bắn từ xa)
        /// </summary>
        public override bool ShouldMoveToTarget(BoardCoord targetPos)
        {
            if (!board.TryGetTerrain(targetPos, out Terrains terrain))
                return DoMoveToTarget; // fallback to default

            switch (terrain)
            {
                case Terrains.Land:
                    return false; // Đứng yên (bắn từ xa)

                default:
                    return DoMoveToTarget;
            }
        }
    }
}