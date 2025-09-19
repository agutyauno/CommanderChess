using System;
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
    
    BoardManager boardManager;
    PieceType pieceType;
    Team team;
    Vector2Int position;

    bool hadRingOfFire;
    int straightMoveRange;
    bool canMoveDiagonal;
    int diagonalMoveRange;

    int straightAttackRange;
    bool canAttackDiagonal;
    int diagonalAttackRange;
    bool isHero;

    #region Properties
    public PieceType PieceType1 { get => pieceType; }
    public Team Team { get => team; }
    public Vector2Int Position { get => position; }
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

public class Commander : Piece
{
    public override List<Vector2Int> GetPossibleAttacks()
    {
        throw new NotImplementedException();
    }

    public override List<Vector2Int> GetPossibleMoves()
    {
        throw new NotImplementedException();
    }
}

public class Indantry : Piece
{
    public override List<Vector2Int> GetPossibleAttacks()
    {
        throw new NotImplementedException();
    }

    public override List<Vector2Int> GetPossibleMoves()
    {
        throw new NotImplementedException();
    }
}
public class Tank : Piece
{
    public override List<Vector2Int> GetPossibleAttacks()
    {
        throw new NotImplementedException();
    }

    public override List<Vector2Int> GetPossibleMoves()
    {
        throw new NotImplementedException();
    }
}

public class Militia : Piece
{
    public override List<Vector2Int> GetPossibleAttacks()
    {
        throw new NotImplementedException();
    }

    public override List<Vector2Int> GetPossibleMoves()
    {
        throw new NotImplementedException();
    }
}
public class Engineer : Piece
{
    public override List<Vector2Int> GetPossibleAttacks()
    {
        throw new NotImplementedException();
    }

    public override List<Vector2Int> GetPossibleMoves()
    {
        throw new NotImplementedException();
    }
}
public class Artillery : Piece
{
    public override List<Vector2Int> GetPossibleAttacks()
    {
        throw new NotImplementedException();
    }

    public override List<Vector2Int> GetPossibleMoves()
    {
        throw new NotImplementedException();
    }
}
public class AntiAircraft : Piece
{
    public override List<Vector2Int> GetPossibleAttacks()
    {
        throw new NotImplementedException();
    }

    public override List<Vector2Int> GetPossibleMoves()
    {
        throw new NotImplementedException();
    }
}
public class Rocket : Piece
{
    public override List<Vector2Int> GetPossibleAttacks()
    {
        throw new NotImplementedException();
    }

    public override List<Vector2Int> GetPossibleMoves()
    {
        throw new NotImplementedException();
    }
}
public class AirForce : Piece
{
    public override List<Vector2Int> GetPossibleAttacks()
    {
        throw new NotImplementedException();
    }

    public override List<Vector2Int> GetPossibleMoves()
    {
        throw new NotImplementedException();
    }
}
public class Navy : Piece
{
    public override List<Vector2Int> GetPossibleAttacks()
    {
        throw new NotImplementedException();
    }

    public override List<Vector2Int> GetPossibleMoves()
    {
        throw new NotImplementedException();
    }
}
public class Headquarters : Piece
{
    public override List<Vector2Int> GetPossibleAttacks()
    {
        throw new NotImplementedException();
    }

    public override List<Vector2Int> GetPossibleMoves()
    {
        throw new NotImplementedException();
    }
}
