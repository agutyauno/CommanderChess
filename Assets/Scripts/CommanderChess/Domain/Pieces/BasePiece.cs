using System.Collections.Generic;
using UnityEngine;
using VContainer;
using CommanderChess.Services;
/// <summary>
/// BasePiece - Abstract base class với template method pattern
/// Các class con override để customize logic di chuyển riêng
/// </summary>
namespace CommanderChess.Domain
{
    public abstract class BasePiece : MonoBehaviour
    {
        #region Directions
        protected readonly BoardCoord[] straightDirs = new[]
        {
            BoardCoord.Left, BoardCoord.Right, BoardCoord.Up, BoardCoord.Down
        };

        protected readonly BoardCoord[] diagonalDirs = new[]
        {
            BoardCoord.UpLeft, BoardCoord.UpRight, BoardCoord.DownLeft, BoardCoord.DownRight
        };
        #endregion

        #region Dependencies (VContainer Injection)
        [Inject] protected readonly Board board;
        [Inject] protected readonly CarryingSystem carryingSystem;
        #endregion

        #region Serialized Fields
        [Header("Piece Configuration")]
        [SerializeField] protected Team team;
        [SerializeField] protected PieceData pieceData;
        #endregion

        #region Fields
        protected bool canBeBlocked;
        protected bool canBeHero;
        protected bool canCarryOthers = true; // default
        protected bool canMoveStraight;
        protected bool canMoveDiagonal;
        protected bool canAttackStraight;
        protected bool canAttackDiagonal;
        protected int straightMoveRange;
        protected int diagonalMoveRange;
        protected int straightAttackRange;
        protected int diagonalAttackRange;
        #endregion

        #region Properties
        public abstract PieceType Type { get; }
        public Team Team { get => team; set => team = value; }
        public PieceData PieceData { get => pieceData; set => pieceData = value; }
        public BoardCoord Position { get; set; }
        public BoardCoord InitialPosition { get; protected set; }

        // Cache
        protected List<BoardCoord> cachedMoves = new();
        protected List<BoardCoord> cachedAttacks = new();
        protected List<BoardCoord> cachedRingOfFireZones = new();

        public List<BoardCoord> PossibleMoves => cachedMoves;
        public List<BoardCoord> PossibleAttacks => cachedAttacks;
        public List<BoardCoord> RingOfFireZones => cachedRingOfFireZones;

        // Properties from PieceData
        public HashSet<PieceType> AllowedCarryTypes { get; protected set; } = new HashSet<PieceType>();
        public bool DoMoveToTarget { get; protected set; } = true;
        public bool HadRingOfFire { get; protected set; } = false;
        public bool IsHero { get; set; } = false;
        public bool CanCarryOthers { get => canCarryOthers; }

        #endregion

        #region Piece Types Enum
        public enum PieceType
        {
            Commander, Infantry, Tank, Militia, Engineer,
            Artillery, AntiAircraft, Rocket, AirForce, Navy, Headquarters
        }
        #endregion

        #region Initialization (Template Method)

        public void Init()
        {
            Debug.Log($"[{GetType().Name}.Init] Initializing {team} {Type} at {Position.ToLabel()}");

            if (board == null || carryingSystem == null)
            {
                Debug.LogError($"[{Type}] Dependencies not injected!");
                return;
            }

            if (pieceData == null)
            {
                Debug.LogError($"[{Type}] PieceData is NULL!");
                return;
            }

            ApplyPieceData(pieceData);
            InitialPosition = Position;

            // Call subclass initialization if needed
            OnInit();

            RecalculateCache();

            Debug.Log($"[{GetType().Name}.Init] {team} {Type}: {cachedMoves.Count} moves, {cachedAttacks.Count} attacks");
            carryingSystem.RegisterPiece(this);
        }

        /// <summary>
        /// Override trong subclass nếu cần custom initialization
        /// </summary>
        protected virtual void OnInit() { }

        protected virtual void ApplyPieceData(PieceData data)
        {
            AllowedCarryTypes.Clear();
            if (data.AllowedCarryTypes != null)
            {
                foreach (var carryType in data.AllowedCarryTypes)
                    AllowedCarryTypes.Add(carryType);
            }
            canBeBlocked = data.CanBeBlocked;
            canBeHero = data.CanBeHero;
            DoMoveToTarget = data.DoMoveToTarget;
            HadRingOfFire = data.HadRingOfFire;

            canMoveStraight = data.CanMoveStraight;
            canMoveDiagonal = data.CanMoveDiagonal;
            canAttackStraight = data.CanAttackStraight;
            canAttackDiagonal = data.CanAttackDiagonal;

            straightMoveRange = data.StraightMoveRange;
            diagonalMoveRange = data.DiagonalMoveRange;
            straightAttackRange = data.StraightAttackRange;
            diagonalAttackRange = data.DiagonalAttackRange;
        }

        #endregion

        #region Cache Recalculation (Template Method)

        public void RecalculateCache()
        {
            if (board == null || pieceData == null) return;

            cachedMoves.Clear();
            cachedAttacks.Clear();

            // Template method - subclasses override these
            CalculateMoves();
            CalculateAttacks();

            if (HadRingOfFire)
                CalculateRingOfFire();
        }

        #endregion

        #region Abstract/Virtual Methods - OVERRIDE IN SUBCLASSES

        /// <summary>
        /// Calculate valid moves - OVERRIDE để custom logic
        /// Default implementation: standard straight/diagonal movement
        /// </summary>
        protected virtual void CalculateMoves()
        {
            // Default: Straight moves
            if (canMoveStraight && straightMoveRange > 0)
            {
                foreach (var dir in straightDirs)
                {
                    AddMovesInDirection(dir, straightMoveRange);
                }
            }

            // Default: Diagonal moves
            if (canMoveDiagonal && diagonalMoveRange > 0)
            {
                foreach (var dir in diagonalDirs)
                {
                    AddMovesInDirection(dir, diagonalMoveRange);
                }
            }
        }

        /// <summary>
        /// Calculate valid attacks - OVERRIDE để custom logic
        /// Default implementation: standard straight/diagonal attacks
        /// </summary>
        protected virtual void CalculateAttacks()
        {
            // Default: Straight attacks
            if (canAttackStraight && straightAttackRange > 0)
            {
                foreach (var dir in straightDirs)
                {
                    AddAttacksInDirection(dir, straightAttackRange);
                }
            }

            // Default: Diagonal attacks
            if (canAttackDiagonal && diagonalAttackRange > 0)
            {
                foreach (var dir in diagonalDirs)
                {
                    AddAttacksInDirection(dir, diagonalAttackRange);
                }
            }
        }

        /// <summary>
        /// Calculate Ring of Fire zones - OVERRIDE nếu cần custom
        /// </summary>
        protected virtual void CalculateRingOfFire()
        {
            cachedRingOfFireZones.Clear();

            if (!HadRingOfFire || pieceData.RingOfFireRange <= 0)
                return;

            int range = pieceData.RingOfFireRange;

            for (int dx = -range; dx <= range; dx++)
            {
                for (int dy = -range; dy <= range; dy++)
                {
                    if (dx == 0 && dy == 0) continue;

                    int distance = Mathf.Abs(dx) + Mathf.Abs(dy);

                    if (distance <= range)
                    {
                        var targetPos = new BoardCoord(Position.x + dx, Position.y + dy);

                        if (board.IsInBoard(targetPos))
                            cachedRingOfFireZones.Add(targetPos);
                    }
                }
            }
        }

        #endregion

        #region Helper Methods - Dùng trong subclasses

        /// <summary>
        /// Add moves in a direction - Helper cho subclasses
        /// </summary>
        protected void AddMovesInDirection(BoardCoord dir, int maxRange)
        {
            for (int distance = 1; distance <= maxRange; distance++)
            {
                var targetPos = Position + (dir * distance);

                if (!board.IsInBoard(targetPos) || !IsTerrainAllowed(targetPos) || cachedMoves.Contains(targetPos))
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

        protected void AddAttacksInDirection(BoardCoord dir, int maxRange)
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
                        cachedAttacks.Add(targetPos);
                    if (canBeBlocked) break;
                    continue;
                }
            }
        }

        /// <summary>
        /// Check if terrain is allowed for movement
        /// </summary>
        protected bool IsTerrainAllowed(BoardCoord targetPos)
        {
            if (pieceData.AllowedMoveTerrains == null || pieceData.AllowedMoveTerrains.Length == 0)
                return true; // No restrictions

            if (board.TryGetTerrain(targetPos, out Terrains terrain))
            {
                foreach (var allowedTerrain in pieceData.AllowedMoveTerrains)
                {
                    if (terrain == allowedTerrain)
                        return true;
                }
                return false; // Not in allowed list
            }

            return true; // No terrain data, allow by default
        }

        /// <summary>
        /// Check if position is occupied by ally or enemy
        /// </summary>
        protected bool IsOccupiedByAlly(BoardCoord pos, out BasePiece ally)
        {
            var ok = board.Pieces.TryGetValue(pos, out BasePiece occupant) && occupant.Team == Team;
            if (ok)
            {
                ally = occupant;
            }
            else
            {
                ally = null;
            }
            return ok;
        }

        protected bool IsOccupiedByEnemy(BoardCoord pos, out BasePiece enemy)
        {
            var ok = board.Pieces.TryGetValue(pos, out BasePiece occupant) && occupant.Team != Team;
            if (ok)
            {
                enemy = occupant;
            }
            else
            {
                enemy = null;
            }
            return ok;
        }

        #endregion

        #region Hero

        public virtual void BecomeHero()
        {
            if (!canBeHero) return;
            if (IsHero) return;
            IsHero = true;
            canMoveDiagonal = true;
            canAttackDiagonal = true;
            straightMoveRange += 1;
            diagonalMoveRange += 1;
            straightAttackRange += 1;
            diagonalAttackRange += 1;

            RecalculateCache();
        }

        #endregion
    }
}