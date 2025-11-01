using UnityEngine;

[CreateAssetMenu(fileName = "PieceData", menuName = "ScriptableObjects/PieceData", order = 1)]
public class PieceData : ScriptableObject
{
    [Header("Movement")]
    [SerializeField] bool canBeBlocked = true;
    [SerializeField] private bool canMoveStraight = true;
    [Range(1, 100), SerializeField] private int straightMoveRange = 1;
    [SerializeField] private bool canMoveDiagonal = false;
    [Range(1, 100), SerializeField] private int diagonalMoveRange = 1;
    [SerializeField] private Terrains[] allowedMoveTerrains;

    [Header("Attack")]
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

    #region properties
    public bool CanMoveStraight { get => canMoveStraight; set => canMoveStraight = value; }
    public int StraightMoveRange { get => straightMoveRange; set => straightMoveRange = value; }
    public bool CanMoveDiagonal { get => canMoveDiagonal; set => canMoveDiagonal = value; }
    public int DiagonalMoveRange { get => diagonalMoveRange; set => diagonalMoveRange = value; }
    public Terrains[] AllowedMoveTerrains { get => allowedMoveTerrains; set => allowedMoveTerrains = value; }
    public bool CanAttackStraight { get => canAttackStraight; set => canAttackStraight = value; }
    public int StraightAttackRange { get => straightAttackRange; set => straightAttackRange = value; }
    public bool CanAttackDiagonal { get => canAttackDiagonal; set => canAttackDiagonal = value; }
    public int DiagonalAttackRange { get => diagonalAttackRange; set => diagonalAttackRange = value; }
    public bool HadRingOfFire { get => hadRingOfFire; set => hadRingOfFire = value; }
    public int RingOfFireRange { get => ringOfFireRange; set => ringOfFireRange = value; }
    public BasePiece.PieceType[] AllowedCarryTypes { get => allowedCarryTypes; set => allowedCarryTypes = value; }
    public bool DoMoveToTarget { get => doMoveToTarget; set => doMoveToTarget = value; }
    public bool CanBeBlocked { get => canBeBlocked; }
    public bool CanBeHero { get => canBeHero;}
    #endregion
}

