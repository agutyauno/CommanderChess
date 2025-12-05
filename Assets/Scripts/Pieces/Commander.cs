using System.Collections.Generic;
using CommanderChess.Core;

namespace CommanderChess.Pieces
{
    /// <summary>
    /// Commander piece - unique to Commander Chess
    /// The Commander has special abilities and can move like a Knight or one square in any direction
    /// </summary>
    public class Commander : ChessPiece
    {
        // Knight-like moves
        private static readonly int[] KnightRowOffsets = { 2, 2, 1, 1, -1, -1, -2, -2 };
        private static readonly int[] KnightColOffsets = { 1, -1, 2, -2, 2, -2, 1, -1 };

        // King-like moves (one square in any direction)
        private static readonly int[] KingRowOffsets = { 1, 1, 1, 0, 0, -1, -1, -1 };
        private static readonly int[] KingColOffsets = { -1, 0, 1, -1, 1, -1, 0, 1 };

        public CommanderAbility ActiveAbility { get; private set; }
        public int AbilityCooldown { get; private set; }
        public bool AbilityUsedThisTurn { get; set; }

        public Commander(PlayerSide side, BoardPosition position) 
            : base(PieceType.Commander, side, position)
        {
            ActiveAbility = CommanderAbility.None;
            AbilityCooldown = 0;
            AbilityUsedThisTurn = false;
        }

        public override List<BoardPosition> GetPossibleMoves(ChessPiece[,] board)
        {
            var moves = new List<BoardPosition>();

            // Knight-like moves
            for (int i = 0; i < 8; i++)
            {
                var newPos = new BoardPosition(
                    Position.Row + KnightRowOffsets[i],
                    Position.Column + KnightColOffsets[i]
                );

                if (newPos.IsValid && IsEmptyOrEnemy(newPos, board))
                {
                    moves.Add(newPos);
                }
            }

            // King-like moves (one square in any direction)
            for (int i = 0; i < 8; i++)
            {
                var newPos = new BoardPosition(
                    Position.Row + KingRowOffsets[i],
                    Position.Column + KingColOffsets[i]
                );

                if (newPos.IsValid && IsEmptyOrEnemy(newPos, board))
                {
                    if (!moves.Contains(newPos))
                    {
                        moves.Add(newPos);
                    }
                }
            }

            return moves;
        }

        /// <summary>
        /// Check if an ability can be used
        /// </summary>
        public bool CanUseAbility(CommanderAbility ability)
        {
            if (AbilityCooldown > 0)
                return false;

            if (AbilityUsedThisTurn)
                return false;

            return ability != CommanderAbility.None;
        }

        /// <summary>
        /// Use an ability
        /// </summary>
        public void UseAbility(CommanderAbility ability)
        {
            ActiveAbility = ability;
            AbilityUsedThisTurn = true;
            AbilityCooldown = GetAbilityCooldown(ability);
        }

        /// <summary>
        /// Called at the start of owner's turn
        /// </summary>
        public void OnTurnStart()
        {
            AbilityUsedThisTurn = false;
            if (AbilityCooldown > 0)
            {
                AbilityCooldown--;
            }
        }

        /// <summary>
        /// Get the cooldown duration for an ability
        /// </summary>
        private int GetAbilityCooldown(CommanderAbility ability)
        {
            return ability switch
            {
                CommanderAbility.Rally => 3,
                CommanderAbility.Charge => 2,
                CommanderAbility.Shield => 3,
                CommanderAbility.Tactics => 2,
                CommanderAbility.Inspire => 3,
                _ => 0
            };
        }

        /// <summary>
        /// Get all pieces affected by Rally ability (adjacent friendly pieces)
        /// </summary>
        public List<BoardPosition> GetRallyTargets(ChessPiece[,] board)
        {
            var targets = new List<BoardPosition>();

            for (int dRow = -1; dRow <= 1; dRow++)
            {
                for (int dCol = -1; dCol <= 1; dCol++)
                {
                    if (dRow == 0 && dCol == 0) continue;

                    var pos = new BoardPosition(Position.Row + dRow, Position.Column + dCol);
                    if (pos.IsValid)
                    {
                        var piece = board[pos.Row, pos.Column];
                        if (piece != null && piece.Side == Side)
                        {
                            targets.Add(pos);
                        }
                    }
                }
            }

            return targets;
        }

        /// <summary>
        /// Get pieces that can be swapped using Tactics ability
        /// </summary>
        public List<BoardPosition> GetTacticsTargets(ChessPiece[,] board)
        {
            var targets = new List<BoardPosition>();

            // Can swap with any friendly piece within 2 squares
            for (int dRow = -2; dRow <= 2; dRow++)
            {
                for (int dCol = -2; dCol <= 2; dCol++)
                {
                    if (dRow == 0 && dCol == 0) continue;

                    var pos = new BoardPosition(Position.Row + dRow, Position.Column + dCol);
                    if (pos.IsValid)
                    {
                        var piece = board[pos.Row, pos.Column];
                        if (piece != null && piece.Side == Side && piece.Type != PieceType.King)
                        {
                            targets.Add(pos);
                        }
                    }
                }
            }

            return targets;
        }

        /// <summary>
        /// Get pieces that can receive extra move from Inspire ability
        /// </summary>
        public List<BoardPosition> GetInspireTargets(ChessPiece[,] board)
        {
            var targets = new List<BoardPosition>();

            // Adjacent friendly pieces (excluding king and commander)
            for (int dRow = -1; dRow <= 1; dRow++)
            {
                for (int dCol = -1; dCol <= 1; dCol++)
                {
                    if (dRow == 0 && dCol == 0) continue;

                    var pos = new BoardPosition(Position.Row + dRow, Position.Column + dCol);
                    if (pos.IsValid)
                    {
                        var piece = board[pos.Row, pos.Column];
                        if (piece != null && piece.Side == Side && 
                            piece.Type != PieceType.King && piece.Type != PieceType.Commander)
                        {
                            targets.Add(pos);
                        }
                    }
                }
            }

            return targets;
        }
    }
}
