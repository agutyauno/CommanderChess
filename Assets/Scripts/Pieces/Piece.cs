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

    [SerializeField] PieceData pieceData;
    [SerializeField] Team team;

    [Inject] protected Board board;
    [Inject] protected PieceController pieceController;

    Piece[] carryingPieces = new Piece[2]; // tối đa mang 2 đơn vị
    Piece carrier; // reference tới parent đang mang mình (nếu có)
    readonly HashSet<PieceType> allowedCarryTypes = new();
    BoardCoord initialPosition;
    BoardCoord position;
    PositionType[] allowedMoveTerrains = Array.Empty<PositionType>();
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
    public Team Team { get => team; }
    public BoardCoord InitialPosition { get; }
    public BoardCoord Position { get => position; set => position = value; }
    public Piece[] CarryingPieces { get => carryingPieces; }
    public Piece Carrier => carrier;
    public bool IsCarried => carrier != null;
    public HashSet<PieceType> AllowedCarryTypes { get => allowedCarryTypes; }
    public bool DoMoveToTarget { get => doMoveToTarget; }
    public bool HadRingOfFire { get => hadRingOfFire; }
    public bool IsHero { get => isHero; }

    #endregion

    #region Initialization
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

        allowedMoveTerrains = data.AllowedMoveTerrains;
        doMoveToTarget = data.doMoveToTarget;
    }
    #endregion
    #region Carry Logic
    protected virtual Piece CheckVaildCarryPiece(Piece piece)
    {
        if (piece == null) return null;
        if (piece.Team != Team) return null;
        // nếu không có loại nào được cấu hình thì không được mang
        if (allowedCarryTypes.Count == 0) return null;
        return allowedCarryTypes.Contains(piece.Type) ? piece : null;
    }

    // Kiểm tra xem có đủ chỗ để thêm piece và tất cả piece nó đang mang không
    private bool CanAcceptPieceAndItsChildren(Piece piece)
    {
        // Đếm số slot trống hiện có
        int freeSlots = 0;
        for (int i = 0; i < carryingPieces.Length; i++)
        {
            if (carryingPieces[i] == null) freeSlots++;
        }

        // Đếm số piece cần thêm (piece + các piece nó đang mang)
        int neededSlots = 1; // cho chính piece
        foreach (var child in piece.carryingPieces)
        {
            if (child != null)
            {
                // Kiểm tra xem child có hợp lệ để mang không
                if (CheckVaildCarryPiece(child) == null) return false;
                neededSlots++;
            }
        }

        return freeSlots >= neededSlots;
    }

    public bool TryAddCarryingPiece(Piece piece, bool changeCarrier = true)
    {

        // kiểm tra piece có thể mang được không
        var validPiece = CheckVaildCarryPiece(piece);
        if (validPiece == null || ReferenceEquals(validPiece, this) || IsAlreadyCarring(piece))
        {
            return false;
        }

        // Kiểm tra có đủ chỗ cho piece và children của nó không
        if (!CanAcceptPieceAndItsChildren(validPiece)) return false;

        //kiểm tra quân đang mang có mang được không
        for (int i = 0; i < carryingPieces.Length; i++)
        {
            if (carryingPieces[i] != null)
            {
                if (carryingPieces[i].TryAddCarryingPiece(piece))
                    break;
            }
        }

        //tìm slot trống để mang
        for (int i = 0; i < carryingPieces.Length; i++)
        {
            if (carryingPieces[i] == null)
            {
                carryingPieces[i] = validPiece;
                if (changeCarrier) validPiece.carrier = this;

                // Thêm các children của piece
                foreach (var child in validPiece.carryingPieces)
                {
                    if (child != null) TryAddCarryingPiece(child, false);
                }
                //nếu là slot thứ 2 thì thử coi mang slot thứ nhất được không
                if (i > 0)
                {
                    piece.TryAddCarryingPiece(carryingPieces[i - 1], false);
                }
                return true;
            }
        }
        return false;
    }

    private bool IsAlreadyCarring(Piece piece)
    {
        foreach (var p in carryingPieces)
        {
            if (p != null)
                if (p.Type == piece.Type) return true;
        }
        return false;
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
                cachedMoves.AddRange(CaculateMoves((x, y), straightMoveRange, allowedMoveTerrains).ToList());
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
                cachedAttacks.AddRange(CaculateAttacks((x, y), straightAttackRange).ToList());
            }
        }
    }

    protected IEnumerable<BoardCoord> CaculateMoves((int x, int y) dir, int range, PositionType[] avoidTypes, bool canBeBlocked = true)
    {
        foreach (var step in Enumerable.Range(1, range))
        {
            var newPos = new BoardCoord(position.x + dir.x * step, position.y + dir.y * step);
            if (!board.IsInBoard(newPos)) yield break;

            board.TryGetZone(newPos, out var posType);
            if (avoidTypes.Any(t => posType == t)) yield break;

            if (pieceController.TryGetPiece(newPos, out var occupant))
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

            if (pieceController.TryGetPiece(newPos, out var occupant))
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
    public void OnCaptured()
    {
        Destroy(gameObject);
        foreach (var child in carryingPieces)
        {
            if (child != null)
            {
                Destroy(child.gameObject);
            }
        }
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

}
