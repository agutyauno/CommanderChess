using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using VContainer;
using CommanderChess.Services;

namespace CommanderChess.Core
{
    public abstract class BasePiece : MonoBehaviour
    {
        #region Dependencies (VContainer Injection)
        [Inject] protected Board board;
        [Inject] protected CarryingSystem carryingSystem;

        #endregion

        #region Serialize field
        [Header("Piece config")]
        [SerializeField] PieceData pieceData;
        [SerializeField] Team team;

        #endregion

        #region Public properties
        public abstract PieceType Type { get; }
        public Team Team => team;
        public PieceType[] AllowedCarryTypes { get; private set; }
        public int MaxCarryCapacity { get; private set; }
        public bool CanCarryOthers { get => canCarryOthers; }
        public bool IsHero { get; private set; } = false;

        #endregion

        #region Protected properties
        protected BoardCoord CurrentPosition => GetCurrentPos();

        #endregion

        #region Fields
        // Piece capabilities from PieceData
        Terrain[] allowedTerrains;

        bool canMoveStraight;
        int straightMoveRange;
        bool canAttackStraight;
        int straightAttackRange;

        bool canMoveDiagonal;
        int diagonalMoveRange;
        bool canAttackDiagonal;
        int diagonalAttackRange;

        bool canBeBlockedByAllies;
        bool canBeBlockedByEnemies;
        bool canAttackOverPieces;

        // Cached valid moves
        private List<BoardCoord> cachedValidMoves;
        private List<BoardCoord> cachedValidAttacks;
        private List<BoardCoord> cachedRingOfFireZones;
        private bool moveCacheValid;
        private bool attackCacheValid;
        private bool ringOfFireCacheValid;

        //private field
        bool canCarryOthers;

        #endregion

        public void Initialize()
        {
            ApplyData();
            InvalidateCache();
            carryingSystem.RegisterPiece(this);
        }

        private void ApplyData()
        {
            allowedTerrains = pieceData.AllowedTerrains;
            canMoveStraight = pieceData.CanMoveStraight;
            straightMoveRange = pieceData.StraightMoveRange;
            canAttackStraight = pieceData.CanAttackStraight;
            straightAttackRange = pieceData.StraightAttackRange;

            canMoveDiagonal = pieceData.CanMoveDiagonal;
            diagonalMoveRange = pieceData.DiagonalMoveRange;
            canAttackDiagonal = pieceData.CanAttackDiagonal;
            diagonalAttackRange = pieceData.DiagonalAttackRange;

            canBeBlockedByAllies = pieceData.CanBeBlockedByAllies;
            canBeBlockedByEnemies = pieceData.CanBeBlockedByEnemies;
            canAttackOverPieces = pieceData.CanAttackOverPieces;

            AllowedCarryTypes = pieceData.AllowedCarryTypes;
            MaxCarryCapacity = pieceData.MaxCarryCapacity;
        }

        void OnDestroy()
        {
            carryingSystem.UnregisterPiece(this);
        }

        private BoardCoord GetCurrentPos()
        {
            return board.TryGetPiecePosition(this, out BoardCoord pos) ? pos : BoardCoord.Invalid;
        }

        #region Valid Moves Calculation

        /// <summary>
        /// Gets all valid move positions for this piece (cached)
        /// </summary>
        public IReadOnlyList<BoardCoord> GetValidMoves()
        {
            if (!moveCacheValid)
            {
                cachedValidMoves = CalculateValidMoves();
                moveCacheValid = true;
            }
            return cachedValidMoves;
        }

        /// <summary>
        /// Gets all valid attack positions for this piece (cached)
        /// </summary>
        public IReadOnlyList<BoardCoord> GetValidAttacks()
        {
            if (!attackCacheValid)
            {
                cachedValidAttacks = CalculateValidAttacks();
                attackCacheValid = true;
            }
            return cachedValidAttacks;
        }

        /// <summary>
        /// Invalidates the cache, forcing recalculation on next access
        /// Should be called after piece moves or board state changes
        /// </summary>
        public void InvalidateCache()
        {
            moveCacheValid = false;
            attackCacheValid = false;
            ringOfFireCacheValid = false;
        }

        /// <summary>
        /// Calculates valid move positions based on piece data
        /// Can be overridden in derived classes for special movement rules
        /// </summary>
        protected virtual List<BoardCoord> CalculateValidMoves()
        {
            if (board == null)
            {
                return new List<BoardCoord>();
            }

            List<BoardCoord> validMoves = new();
            BoardCoord currentPos = GetCurrentPos();

            // Calculate straight moves (vertical and horizontal)
            if (canMoveStraight)
            {
                foreach (var direction in BoardCoord.StraightDirections)
                {
                    AddMovesInDirection(validMoves, currentPos, direction, straightMoveRange);
                }
            }

            // Calculate diagonal moves
            if (canMoveDiagonal)
            {
                foreach (var direction in BoardCoord.DiagonalDirections)
                {
                    AddMovesInDirection(validMoves, currentPos, direction, diagonalMoveRange);
                }
            }

            return validMoves;
        }

        /// <summary>
        /// Calculates valid attack positions based on piece data
        /// Can be overridden in derived classes for special attack rules
        /// </summary>
        protected virtual List<BoardCoord> CalculateValidAttacks()
        {
            if (board == null)
            {
                return new List<BoardCoord>();
            }

            List<BoardCoord> validAttacks = new();
            BoardCoord currentPos = GetCurrentPos();

            // Calculate straight attacks (vertical and horizontal)
            if (canAttackStraight)
            {
                foreach (var direction in BoardCoord.StraightDirections)
                {
                    AddAttacksInDirection(validAttacks, currentPos, direction, straightAttackRange);
                }
            }

            // Calculate diagonal attacks
            if (canAttackDiagonal)
            {
                foreach (var direction in BoardCoord.DiagonalDirections)
                {
                    AddAttacksInDirection(validAttacks, currentPos, direction, diagonalAttackRange);
                }
            }

            return validAttacks;
        }

        /// <summary>
        /// Adds moves in a specific direction up to a certain range
        /// </summary>
        protected void AddMovesInDirection(List<BoardCoord> moves, BoardCoord currentPos, BoardCoord direction, int range)
        {
            for (int distance = 1; distance <= range; distance++)
            {
                BoardCoord targetCoord = currentPos + (direction * distance);
                if (!board.IsInBoard(targetCoord))
                    break; // Out of bounds

                Terrain terrain = board.GetTerrain(targetCoord);
                if (!IsTerrainAllowed(terrain))
                    break; // Terrain not allowed

                if (board.TryGetPiece(targetCoord, out BasePiece occupant))
                {
                    bool isAlly = occupant.Team == Team;
                    bool thisPieceIsCarrier = AllowedCarryTypes.Contains(occupant.Type);
                    bool carryable = occupant.AllowedCarryTypes.Contains(Type) || thisPieceIsCarrier;

                    if (isAlly && ((thisPieceIsCarrier && canCarryOthers) || (carryable && occupant.CanCarryOthers)))
                        moves.Add(targetCoord);
                    if (isAlly && canBeBlockedByAllies || !isAlly && canBeBlockedByEnemies)
                        break;
                    continue;
                }
                moves.Add(targetCoord);
            }
        }

        /// <summary>
        /// Adds attacks in a specific direction up to a certain range
        /// </summary>
        protected void AddAttacksInDirection(List<BoardCoord> attacks, BoardCoord currentPos, BoardCoord direction, int range)
        {
            for (int distance = 1; distance <= range; distance++)
            {
                BoardCoord targetCoord = currentPos + (direction * distance);
                if (!board.IsInBoard(targetCoord))
                {
                    break; // Out of bounds
                }

                if (board.TryGetPiece(targetCoord, out BasePiece occupant))
                {
                    bool isEnemy = occupant.Team != Team;

                    if (isEnemy && !attacks.Contains(targetCoord))
                        attacks.Add(targetCoord);
                    if (canAttackOverPieces) break;
                    continue;
                }
            }
        }

        /// <summary>
        /// Checks if this piece can move/attack on the given terrain
        /// </summary>
        protected bool IsTerrainAllowed(Terrain terrain)
        {
            if (allowedTerrains == null || allowedTerrains.Length == 0)
            {
                return true; // If no restrictions, allow all terrains
            }

            foreach (var allowedTerrain in allowedTerrains)
            {
                if (terrain == allowedTerrain)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if a move to the target coordinate is valid
        /// </summary>
        public bool IsValidMove(BoardCoord target)
        {
            var validMoves = GetValidMoves();
            return validMoves.Contains(target);
        }

        /// <summary>
        /// Checks if an attack to the target coordinate is valid
        /// </summary>
        public bool IsValidAttack(BoardCoord target)
        {
            var validAttacks = GetValidAttacks();
            return validAttacks.Contains(target);
        }

        #endregion

        #region Ring of fire caculation
        public IReadOnlyList<BoardCoord> GetRingOfFireZones()
        {
            if (!ringOfFireCacheValid)
            {
                cachedRingOfFireZones = CalculateRingOfFireZones();
                ringOfFireCacheValid = true;
            }
            return cachedRingOfFireZones;
        }

        private List<BoardCoord> CalculateRingOfFireZones()
        {
            List<BoardCoord> zone = new();
            if (!pieceData.RingOfFire || pieceData.RingOfFireRange <= 0)
                return zone;

            int range = pieceData.RingOfFireRange;
            BoardCoord position = GetCurrentPos();

            for (int dx = -range; dx <= range; dx++)
            {
                for (int dy = -range; dy <= range; dy++)
                {
                    int distance = Mathf.Abs(dx) + Mathf.Abs(dy);

                    if (distance <= range)
                    {
                        var targetPos = new BoardCoord(position.X + dx, position.Y + dy);

                        if (board.IsInBoard(targetPos))
                            zone.Add(targetPos);
                    }
                }
            }
            return zone;
        }
        #endregion

        public virtual bool ShouldMoveToTarget(BoardCoord target)
        {
            if (pieceData.DoMoveToTarget && !IsTerrainAllowed(board.GetTerrain(target)))
            {
                return true;
            }
            return true;
        }

        public void BecomeHero()
        {
            if (IsHero) return;
            IsHero = true;
            canMoveDiagonal = true;
            canAttackDiagonal = true;
            straightMoveRange += 1;
            diagonalMoveRange += 1;
            straightAttackRange += 1;
            diagonalAttackRange += 1;

            InvalidateCache();
        }
    }
}