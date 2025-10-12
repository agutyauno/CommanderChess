using System;
using System.Collections.Generic;
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
    public int RingOfFireRange { get => GetRingOfFireRange(); }
    public int StraightMoveRange { get => GetStraightMoveRange(); }
    public int DiagonalMoveRange { get => GetDiagonalMoveRange(); }
    public int StraightAttackRange { get => GetStraightAttackRange(); }
    public int DiagonalAttackRange { get => GetDiagonalAttackRange(); }
    public bool IsHero { get => isHero; }

    #endregion
    #region Range Getters
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
        foreach (var child in piece.CarryingPieces)
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
                foreach (var child in validPiece.CarryingPieces)
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
        CalculatePossibleMoves();
        CalculatePossibleAttacks();
        CalculateRingOfFireZones();
    }
    virtual protected void CalculatePossibleMoves()
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

                    if (pieceController.TryGetPiece(newPos, out var occupant))
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

                    if (pieceController.TryGetPiece(newPos, out var occupant)) break;
                    cachedMoves.Add(newPos);
                    step++;
                }
            }
        }
    }
    virtual protected void CalculatePossibleAttacks()
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
                    if (pieceController.TryGetPiece(newPos, out var occupant) && occupant.Team != Team)
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
                    if (pieceController.TryGetPiece(newPos, out var occupant) && occupant.Team != Team)
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
