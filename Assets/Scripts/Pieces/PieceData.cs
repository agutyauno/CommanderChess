using UnityEngine;

[CreateAssetMenu(fileName = "PieceData", menuName = "ScriptableObjects/PieceData", order = 1)]
public class PieceData : ScriptableObject
{

    [Tooltip("Initial intersection (1-based). A1 => (1,1)")]
    public Vector2Int initialPosition = new Vector2Int(1, 1);

    [Header("Movement")]
    public bool canMoveStraight = true;
    [Range(1, 100)] public int straightMoveRange = 1;
    public bool canMoveDiagonal = false;
    [Range(1, 100)] public int diagonalMoveRange = 1;

    [Header("Attack")]
    public bool canAttackStraight = true;
    [Range(1, 100)] public int straightAttackRange = 1;
    public bool canAttackDiagonal = false;
    [Range(1, 100)] public int diagonalAttackRange = 1;

    [Header("Ring of Fire")]
    public bool hadRingOfFire = false;
    [Range(1, 100)] public int ringOfFireRange = 1;

    [Header("Carrying")]
    public Piece.PieceType[] allowedCarryTypes = {};

    [Header("Other")]
    public bool doMoveToTarget = true;
}

