using UnityEngine;
using CommanderChess.Domain;

[CreateAssetMenu(fileName = "PieceData", menuName = "ScriptableObjects/PieceData", order = 1)]
public class PieceData : ScriptableObject
{
    [Header("Movement")]
    [SerializeField] bool moveCanBeBlocked = true;
    [SerializeField] private bool canMoveStraight = true;
    [Range(1, 100), SerializeField] private int straightMoveRange = 1;
    [SerializeField] private bool canMoveDiagonal = false;
    [Range(1, 100), SerializeField] private int diagonalMoveRange = 1;
    [SerializeField] private Terrains[] allowedMoveTerrains;

    [Header("Attack")]
    [SerializeField] bool attackCanBeBlocked = true;
    [SerializeField] private bool canAttackStraight = true;
    [Range(1, 100), SerializeField] private int straightAttackRange = 1;
    [SerializeField] private bool canAttackDiagonal = false;
    [Range(1, 100), SerializeField] private int diagonalAttackRange = 1;

    [Header("Ring of Fire")]
    [SerializeField] private bool hadRingOfFire = false;
    [Range(1, 100), SerializeField] private int ringOfFireRange = 1;

    [Header("Carrying")]
    [SerializeField] private BasePiece.PieceType[] allowedCarryTypes = { };

    [Header("Other")]
    [SerializeField] private bool doMoveToTarget = true;
    [SerializeField] bool canBeHero = true;
    [SerializeField] private int maxCarryCapacity = 0;

    #region properties
    public bool CanMoveStraight { get => canMoveStraight; }
    public int StraightMoveRange { get => straightMoveRange; }
    public bool CanMoveDiagonal { get => canMoveDiagonal; }
    public int DiagonalMoveRange { get => diagonalMoveRange; }
    public Terrains[] AllowedMoveTerrains { get => allowedMoveTerrains; }
    public bool CanAttackStraight { get => canAttackStraight; }
    public int StraightAttackRange { get => straightAttackRange; }
    public bool CanAttackDiagonal { get => canAttackDiagonal; }
    public int DiagonalAttackRange { get => diagonalAttackRange; }
    public bool HadRingOfFire { get => hadRingOfFire; }
    public int RingOfFireRange { get => ringOfFireRange; }
    public BasePiece.PieceType[] AllowedCarryTypes { get => allowedCarryTypes; }
    public bool DoMoveToTarget { get => doMoveToTarget; }
    public bool CanBeBlocked { get => moveCanBeBlocked; }
    public bool CanBeHero { get => canBeHero;}
    public bool AttackCanBeBlocked { get => attackCanBeBlocked; }
    public int MaxCarryCapacity { get => maxCarryCapacity; }
    #endregion
}

