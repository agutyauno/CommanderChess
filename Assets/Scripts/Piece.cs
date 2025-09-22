using System.Collections.Generic;
using UnityEngine;
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
    
    [SerializeField] Team team;
    BoardCoord position;
    bool hadRingOfFire;
    int straightMoveRange;
    bool canMoveDiagonal;
    int diagonalMoveRange;

    int straightAttackRange;
    bool canAttackDiagonal;
    int diagonalAttackRange;
    bool isHero;

    #region Properties
    public abstract PieceType Type { get; }
    public Team Team { get => team; }
    public BoardCoord Position { get => position; set => position = value; }
    public abstract BoardCoord InitialPosition { get; }

    public bool HadRingOfFire { get => hadRingOfFire; }
    public int StraightMoveRange { get => straightMoveRange; }
    public int DiagonalMoveRange { get => GetDiagonalMoveRange(); }

    public int StraightAttackRange { get => straightAttackRange; }
    public int DiagonalAttackRange { get => GetDiagonalAttackRange(); }
    public bool IsHero { get => isHero; }

    #endregion

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

    abstract public List<Vector2Int> GetPossibleMoves();
    abstract public List<Vector2Int> GetPossibleAttacks();
}

public enum Team
{
    Red,
    Blue
}
