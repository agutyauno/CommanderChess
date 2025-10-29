using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;
public abstract class Piece : MonoBehaviour
{
    public enum PieceType
    {
        Commander, Infantry, Tank, Militia, Engineer, Artillery, AntiAircraft, Rocket, AirForce, Navy, Headquarters
    }
    #region Fields
    protected (int x, int y)[] sdirs = new (int dx, int dy)[] { (1, 0), (-1, 0), (0, 1), (0, -1) };
    protected (int x, int y)[] ddirs = new (int dx, int dy)[] { (1, 1), (1, -1), (-1, 1), (-1, -1) };

    readonly List<BoardCoord> cachedMoves = new();
    readonly List<BoardCoord> cachedAttacks = new();
    readonly List<BoardCoord> cachedRingOfFireZones = new();

    PieceData pieceData;
    [SerializeField] Team team;

    [Inject] protected Board board;
    [Inject] protected CarryingSystem carryingSystem;

    readonly HashSet<PieceType> allowedCarryTypes = new();
    BoardCoord position;
    Terrains[] allowedMoveTerrains = Array.Empty<Terrains>();
    bool doMoveToTarget;
    bool hadRingOfFire;
    int ringOfFireRange;

    bool canMoveStraight;
    int straightMoveRange;

    bool canMoveDiagonal;
    int diagonalMoveRange;

    bool canAttackStraight;
    int straightAttackRange;

    bool canAttackDiagonal;
    int diagonalAttackRange;
    bool isHero;
    #endregion
    #region Properties
    public List<BoardCoord> PossibleMoves { get => cachedMoves; }
    public List<BoardCoord> PossibleAttacks { get => cachedAttacks; }
    public List<BoardCoord> RingOfFireZones { get => cachedRingOfFireZones; }
    public abstract PieceType Type { get; }
    public Team Team { get => team; set => team = value; }
    public BoardCoord Position { get => position; set => position = value; }
    public HashSet<PieceType> AllowedCarryTypes { get => allowedCarryTypes; }
    public bool DoMoveToTarget { get => doMoveToTarget; }
    public bool HadRingOfFire { get => hadRingOfFire; }
    public bool IsHero { get => isHero; set => isHero = value; }
    public PieceData PieceData { get => pieceData; set => pieceData = value; }

    #endregion

    #region Initialization
    public void Init()
    {
        if (pieceData != null)
            ApplyPieceData(pieceData);
        carryingSystem.RegisterPiece(this);
        RecalculateCache();
    }
    private void ApplyPieceData(PieceData data)
    {
        if (data == null) return;

        canMoveStraight = data.CanMoveStraight;
        straightMoveRange = data.StraightMoveRange;
        canMoveDiagonal = data.CanMoveDiagonal;
        diagonalMoveRange = data.DiagonalMoveRange;

        canAttackStraight = data.CanAttackStraight;
        straightAttackRange = data.StraightAttackRange;
        canAttackDiagonal = data.CanAttackDiagonal;
        diagonalAttackRange = data.DiagonalAttackRange;
        hadRingOfFire = data.HadRingOfFire;
        ringOfFireRange = data.RingOfFireRange;

        foreach (var t in data.AllowedCarryTypes)
        {
            allowedCarryTypes.Add(t);
        }

        allowedMoveTerrains = data.AllowedMoveTerrains;
        doMoveToTarget = data.DoMoveToTarget;
    }
    #endregion
    #region cache calculation
    public void RecalculateCache()
    {
        UpdateMoveCache();
        UpdateAttackCache();
        CalculateRingOfFireZones();
    }
    virtual protected void UpdateMoveCache()
    {
        cachedMoves.Clear();
        if (board == null) return;

        if (canMoveStraight)
        {
            foreach (var (x, y) in sdirs)
            {
                cachedMoves.AddRange(CaculateMoves((x, y), straightMoveRange, allowedMoveTerrains).ToList());
            }
        }

        if (canMoveDiagonal)
        {
            foreach (var (x, y) in ddirs)
            {
                cachedMoves.AddRange(CaculateMoves((x, y), diagonalMoveRange, allowedMoveTerrains).ToList());
            }
        }
    }
    virtual protected void UpdateAttackCache()
    {
        cachedAttacks.Clear();
        if (board == null) return;

        if (canAttackStraight)
        {
            foreach (var (x, y) in sdirs)
            {
                cachedAttacks.AddRange(CaculateAttacks((x, y), straightAttackRange).ToList());
            }
        }

        if (canAttackDiagonal)
        {
            foreach (var (x, y) in ddirs)
            {
                cachedAttacks.AddRange(CaculateAttacks((x, y), diagonalAttackRange).ToList());
            }
        }
    }
    protected IEnumerable<BoardCoord> CaculateMoves((int x, int y) dir, int range, Terrains[] avoidTypes, bool canBeBlocked = true)
    {
        foreach (var step in Enumerable.Range(1, range))
        {
            var newPos = new BoardCoord(position.x + dir.x * step, position.y + dir.y * step);
            if (!board.IsInBoard(newPos)) yield break;

            board.TryGetTerrain(newPos, out var posType);
            if (avoidTypes.Any(t => posType == t)) yield break;

            if (board.TryGetPiece(newPos, out var occupant))
            {
                if (canBeBlocked)
                {
                    yield break;
                }

                if (occupant.Team != Team) yield break;
                if (!occupant.allowedCarryTypes.Contains(Type)) yield break;
            }

            yield return newPos;
        }
    }
    protected IEnumerable<BoardCoord> CaculateAttacks((int x, int y) dir, int range)
    {
        foreach (var step in Enumerable.Range(1, range))
        {
            var newPos = new BoardCoord(position.x + dir.x * step, position.y + dir.y * step);
            if (!board.IsInBoard(newPos)) yield break;

            if (board.TryGetPiece(newPos, out var occupant))
            {
                if (occupant.team == team) yield break;
                yield return newPos;
            }
        }
    }
    void CalculateRingOfFireZones()
    {
        if (!hadRingOfFire || board == null) return;
        cachedRingOfFireZones.Clear();
        for (int dx = -ringOfFireRange; dx <= ringOfFireRange; dx++)
        {
            for (int dy = -ringOfFireRange; dy <= ringOfFireRange; dy++)
            {
                if (Math.Abs(dx) + Math.Abs(dy) > ringOfFireRange) continue;
                var newPos = new BoardCoord(position.x + dx, position.y + dy);
                if (!board.IsInBoard(newPos)) continue;
                cachedRingOfFireZones.Add(newPos);
            }
        }
    }
    #endregion
    
    public void BecomeHero()
    {
        if (isHero) return;

        isHero = true;
        canMoveDiagonal = true;
        diagonalMoveRange += 1;
        straightMoveRange += 1;
        diagonalAttackRange += 1;
        straightAttackRange += 1;
    }
}
