using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
public abstract class Piece : MonoBehaviour
{
    public enum PieceType
    {
        Commander,
        Indantry,
        Tank,
        Militia,
        Engineer,
        Artillery,
        AntiAircraft,
        Rocket,
        AirForce,
        Navy,
        Headquarters
    }

    protected (int x, int y)[] sdirs = new (int dx, int dy)[] { (1, 0), (-1, 0), (0, 1), (0, -1) };
    protected (int x, int y)[] ddirs = new (int dx, int dy)[] { (1, 1), (1, -1), (-1, 1), (-1, -1) };

    readonly List<BoardCoord> cachedMoves = new();
    readonly List<BoardCoord> cachedAttacks = new();
    readonly List<BoardCoord> cachedRingOfFireZones = new(); 

    [SerializeField] PieceData pieceData;
    [SerializeField] Team team;

    [Inject] protected Board board;

    Piece[] carryingPiece = new Piece[2]; // tối đa mang 2 đơn vị
    readonly HashSet<PieceType> allowedCarryTypes = new();
    BoardCoord initialPosition;
    BoardCoord position;
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

    #region Properties
    public List<BoardCoord> PossibleMoves { get => cachedMoves; }
    public List<BoardCoord> PossibleAttacks { get => cachedAttacks; }
    public List<BoardCoord> RingOfFireZones {get => cachedRingOfFireZones; }
    public abstract PieceType Type { get; }
    public Team Team { get => team; }
    public BoardCoord InitialPosition { get; }
    public BoardCoord Position { get => position; set => position = value; }
    public Piece[] CarryingPiece { get => carryingPiece; }
    public HashSet<PieceType> AllowedCarryTypes { get => allowedCarryTypes; }
    public bool DoMoveToTarget { get => doMoveToTarget; }
    public bool HadRingOfFire { get => hadRingOfFire; }
    public int RingOfFireRange { get => GetRingOfFireRange(); }
    public int StraightMoveRange { get => GetStraightMoveRange(); }
    public int DiagonalMoveRange { get => GetDiagonalMoveRange(); }
    public int StraightAttackRange { get => GetStraightAttackRange(); }
    public int DiagonalAttackRange { get => GetDiagonalAttackRange(); }
    public bool IsHero { get => isHero; }

    #endregion
    int GetStraightMoveRange()
    {
        if (!canMoveStraight)
        {
            return 0;
        }
        return straightMoveRange;
    }
    int GetStraightAttackRange()
    {
        if (!canAttackStraight)
        {
            return 0;
        }
        return straightAttackRange;
    }
    int GetDiagonalMoveRange()
    {
        if (!canMoveDiagonal)
        {
            return 0;
        }
        return diagonalMoveRange;
    }
    int GetDiagonalAttackRange()
    {
        if (!canAttackDiagonal)
        {
            return 0;
        }
        return diagonalAttackRange;
    }
    int GetRingOfFireRange()
    {
        if (!hadRingOfFire)
        {
            return 0;
        }
        return ringOfFireRange;
    }
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

    public void Init()
    {
        if (pieceData != null)
            ApplyPieceData(pieceData);
        position = initialPosition;
        RecalculateCache();
    }

    private void ApplyPieceData(PieceData data)
    {
        if (data == null) return;
        initialPosition = data.initialPosition;

        canMoveStraight = data.canMoveStraight;
        straightMoveRange = data.straightMoveRange;
        canMoveDiagonal = data.canMoveDiagonal;
        diagonalMoveRange = data.diagonalMoveRange;

        canAttackStraight = data.canAttackStraight;
        straightAttackRange = data.straightAttackRange;
        canAttackDiagonal = data.canAttackDiagonal;
        diagonalAttackRange = data.diagonalAttackRange;
        hadRingOfFire = data.hadRingOfFire;
        ringOfFireRange = data.ringOfFireRange;

        foreach (var t in data.allowedCarryTypes)
        {
            allowedCarryTypes.Add(t);
        }
        doMoveToTarget = data.doMoveToTarget;
    }

    protected virtual Piece CheckVaildCarryPiece(Piece piece)
    {
        if (piece == null) return null;
        // nếu không có loại nào được cấu hình thì không được mang
        if (allowedCarryTypes.Count == 0) return null;
        return allowedCarryTypes.Contains(piece.Type) ? piece : null;
    }

    public bool TryAddCarryingPiece(Piece piece)
    {
        var validPiece = CheckVaildCarryPiece(piece);
        if (validPiece == null) return false;

        // tránh tự mang chính mình
        if (ReferenceEquals(validPiece, this)) return false;

        for (int i = 0; i < carryingPiece.Length; i++)
        {
            if (carryingPiece[i] == null)
            {
                carryingPiece[i] = validPiece;
                return true;
            }
            else if (carryingPiece[i] == validPiece)
            {
                // đã mang rồi
                return false;
            }
            else
            {
                // thử để piece đang mang tiếp tục mang validPiece
                if (carryingPiece[i].TryAddCarryingPiece(validPiece))
                    continue;
            }
        }
        return false;
    }

    public void RecalculateCache()
    {
        CalulatePossibleMoves();
        CalulatePossibleAttacks();
        CalculateRingOfFireZones();
    }

    virtual protected void CalulatePossibleMoves()
    {
        cachedMoves.Clear();
        if (board == null) return;

        if (canMoveStraight)
        {
            foreach (var (x, y) in sdirs)
            {
                int step = 1;
                while (step <= straightMoveRange)
                {
                    var newPos = new BoardCoord(position.x + x * step, position.y + y * step);
                    if (!board.IsInBoard(newPos)) break;
    
                    board.TryGetZone(newPos, out var posType);
                    if (posType == PositionType.Sea) break;

                    if (board.TryGetPiece(newPos, out var occupant))
                    {
                        if (occupant.team != team) break;
                        if (!occupant.allowedCarryTypes.Contains(Type)) break;
                    }
    
                    cachedMoves.Add(newPos);
                    step++;
                }
            }
        }

        if (canMoveDiagonal)
        {
            foreach (var (x, y) in ddirs)
            {
                int step = 1;
                while (step <= diagonalMoveRange)
                {
                    var newPos = new BoardCoord(position.x + x * step, position.y + y * step);
                    if (!board.IsInBoard(newPos)) break;

                    board.TryGetZone(newPos, out var posType);
                    if (posType == PositionType.Sea) break;

                    if (board.TryGetPiece(newPos, out var occupant)) break;
                    cachedMoves.Add(newPos);
                    step++;
                }
            }
        }
    }

    virtual protected void CalulatePossibleAttacks()
    {
        cachedAttacks.Clear();
        if (board == null) return;

        if (canAttackStraight)
        {
            foreach (var (x, y) in sdirs)
            {
                int step = 1;
                while (step <= straightAttackRange)
                {
                    var newPos = new BoardCoord(position.x + x * step, position.y + y * step);
                    if (!board.IsInBoard(newPos)) break;
                    if (board.TryGetPiece(newPos, out var occupant) && occupant.Team != Team)
                        cachedAttacks.Add(newPos);
                    step++;
                }
            }
        }

        if (canAttackDiagonal)
        {
            foreach (var (x, y) in ddirs)
            {
                int step = 1;
                while (step <= diagonalAttackRange)
                {
                    var newPos = new BoardCoord(position.x + x * step, position.y + y * step);
                    if (!board.IsInBoard(newPos)) break;
                    if (board.TryGetPiece(newPos, out var occupant) && occupant.Team != Team)
                        cachedAttacks.Add(newPos);
                    step++;
                }
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
    public void OnCaptured()
    {
        Destroy(gameObject);
    }
}
