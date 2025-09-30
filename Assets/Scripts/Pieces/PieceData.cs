using UnityEngine;

[CreateAssetMenu(fileName = "PieceData", menuName = "ScriptableObjects/PieceData", order = 1)]
public class PieceData : ScriptableObject
{

    [Tooltip("Initial intersection (1-based). A1 => (1,1)")]
    public Vector2Int initialPosition = new Vector2Int(1, 1);

    [Header("Movement")]
    public bool canMoveStraight = true;
    [Range(0, 100)] public int straightMoveRange = 1;
    public bool canMoveDiagonal = false;
    [Range(0, 100)] public int diagonalMoveRange = 0;

    [Header("Attack")]
    public bool canAttackStraight = true;
    [Range(0, 100)] public int straightAttackRange = 1;
    public bool canAttackDiagonal = false;
    [Range(0, 100)] public int diagonalAttackRange = 0;

    [Header("Ring of Fire")]
    public bool hadRingOfFire = false;
    [Range(0, 100)] public int ringOfFireRange = 1;

    [Header("Other")]
    public bool doMoveToTarget = false;

    [Header("Visual / Prefab")]
    public Sprite icon;
}

