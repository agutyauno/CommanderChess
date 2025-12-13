using UnityEngine;

namespace CommanderChess.Core
{
    internal class PieceData : ScriptableObject
    {
        [SerializeField] Terrain[] allowedTerrains;
        [SerializeField] PieceType[] allowedCarryTypes;
        [SerializeField] int maxCarryCapacity;

        [Header("Movement and Attack Capabilities")]
        [SerializeField] bool canMoveStraight = true;
        [Range(0, 20)] public int straightMoveRange = 1;
        [SerializeField] bool canAttackStraight = true;
        [Range(0, 20)] public int straightAttackRange = 1;

        [Space]
        [SerializeField] bool canMoveDiagonal = false;
        [Range(0, 20)] public int diagonalMoveRange = 1;
        [SerializeField] bool canAttackDiagonal = false;
        [Range(0, 20)] public int diagonalAttackRange = 1;

        [Space]
        [SerializeField] bool canBeBlockedByAllies;
        [SerializeField] bool canBeBlockedByEnemies;
        [SerializeField] bool canAttackOverPieces;

        [Header("Other"),Space]
        [SerializeField] bool ringOfFire;
        [SerializeField] int ringOfFireRange;
        [SerializeField] bool doMoveToTarget;

        #region Properties
        public Terrain[] AllowedTerrains => allowedTerrains;
        public bool CanMoveStraight => canMoveStraight;
        public int StraightMoveRange => straightMoveRange;
        public bool CanAttackStraight => canAttackStraight;
        public int StraightAttackRange => straightAttackRange;
        public bool CanMoveDiagonal => canMoveDiagonal;
        public int DiagonalMoveRange => diagonalMoveRange;
        public bool CanAttackDiagonal => canAttackDiagonal;
        public int DiagonalAttackRange => diagonalAttackRange;
        public bool CanBeBlockedByAllies => canBeBlockedByAllies;
        public bool CanBeBlockedByEnemies => canBeBlockedByEnemies;
        public bool CanAttackOverPieces => canAttackOverPieces;
        public bool RingOfFire => ringOfFire;
        public int RingOfFireRange => ringOfFireRange;
        public bool DoMoveToTarget => doMoveToTarget;

        public PieceType[] AllowedCarryTypes { get => allowedCarryTypes; set => allowedCarryTypes = value; }
        public int MaxCarryCapacity { get => maxCarryCapacity; set => maxCarryCapacity = value; }
        #endregion
    }
}