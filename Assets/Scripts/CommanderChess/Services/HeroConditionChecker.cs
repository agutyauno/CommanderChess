using System.Linq;
using CommanderChess.Domain;
using CommanderChess.Events;
using VContainer;

namespace CommanderChess.Services
{
    /// <summary>
    /// Checks hero conditions:
    /// 1. Piece can attack enemy commander without going through danger zones (checked each move)
    /// 2. Piece is the last piece besides Commander and HQ (checked when pieces are captured/destroyed)
    /// </summary>
    public class HeroConditionChecker
    {
        private readonly Board board;
        private readonly PathChecker pathChecker;
        private readonly EventBus eventBus;

        [Inject]
        public HeroConditionChecker(Board board, PathChecker pathChecker, EventBus eventBus)
        {
            this.board = board;
            this.pathChecker = pathChecker;
            this.eventBus = eventBus;
        }

        /// <summary>
        /// Check hero conditions for all pieces of both teams
        /// </summary>
        public void CheckAllHeroConditions()
        {
            CheckHeroConditionsForTeam(Team.Red);
            CheckHeroConditionsForTeam(Team.Blue);
        }

        /// <summary>
        /// Check hero conditions for specific piece after move
        /// </summary>
        public void CheckHeroConditionForPiece(BasePiece piece)
        {
            if (piece == null || piece.IsHero)
                return;

            // Condition 1: Can attack commander without danger zones
            if (CanAttackCommanderSafely(piece))
            {
                PromoteToHero(piece, HeroCondition.CanAttackCommander);
                return;
            }

            // Condition 2: Last piece besides Commander and HQ
            if (IsLastPiece(piece))
            {
                PromoteToHero(piece, HeroCondition.LastPiece);
            }
        }

        private void CheckHeroConditionsForTeam(Team team)
        {
            var teamPieces = board.Pieces.Values
                .Where(p => p.Team == team && !p.IsHero)
                .ToList();

            foreach (var piece in teamPieces)
            {
                CheckHeroConditionForPiece(piece);
            }
        }

        /// <summary>
        /// Check if piece can attack enemy commander without passing through danger zones
        /// Commander must be in PossibleAttacks AND CheckPath returns PathResult.None
        /// </summary>
        private bool CanAttackCommanderSafely(BasePiece piece)
        {
            // Only pieces except Commander and HQ can become heroes
            if (piece.Type == BasePiece.PieceType.Commander || 
                piece.Type == BasePiece.PieceType.Headquarters)
            {
                return false;
            }

            // Find enemy commander
            var enemyTeam = piece.Team == Team.Red ? Team.Blue : Team.Red;
            var enemyCommander = board.Pieces.Values
                .FirstOrDefault(p => p.Team == enemyTeam && p.Type == BasePiece.PieceType.Commander);

            if (enemyCommander == null)
                return false;

            // Check if commander is in possible attacks
            if (!piece.PossibleAttacks.Contains(enemyCommander.Position))
                return false;

            // Check if path to commander is safe (no danger zones)
            var pathResult = pathChecker.CheckPath(piece, piece.Position, enemyCommander.Position);
            return pathResult.Result == PathResult.None;
        }

        /// <summary>
        /// Check if piece is the last piece besides Commander and HQ
        /// Example: Team has Commander + HQ + 1 piece -> that piece becomes hero
        /// Example: Team has Commander + 2 HQ + 1 piece -> that piece becomes hero
        /// </summary>
        private bool IsLastPiece(BasePiece piece)
        {
            // Only pieces except Commander and HQ can become heroes
            if (piece.Type == BasePiece.PieceType.Commander || 
                piece.Type == BasePiece.PieceType.Headquarters)
            {
                return false;
            }

            // Count non-Commander, non-HQ pieces
            var nonSpecialPieces = board.Pieces.Values
                .Where(p => p.Team == piece.Team && 
                           p.Type != BasePiece.PieceType.Commander && 
                           p.Type != BasePiece.PieceType.Headquarters)
                .ToList();

            // If only 1 piece left (this piece), it becomes hero
            return nonSpecialPieces.Count == 1 && nonSpecialPieces[0] == piece;
        }

        private void PromoteToHero(BasePiece piece, HeroCondition condition)
        {
            piece.IsHero = true;
            eventBus.Publish(new HeroBecameEvent(piece, condition));
            UnityEngine.Debug.Log($"[HeroConditionChecker] {piece.Team} {piece.Type} at {piece.Position.ToLabel()} became HERO! ({condition})");
        }
    }
}
