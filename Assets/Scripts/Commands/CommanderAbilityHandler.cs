using System.Collections.Generic;
using CommanderChess.Board;
using CommanderChess.Core;
using CommanderChess.Pieces;

namespace CommanderChess.Commands
{
    /// <summary>
    /// Handles Commander special abilities
    /// </summary>
    public class CommanderAbilityHandler
    {
        private readonly ChessBoard _board;

        // Track which pieces have been boosted by Rally this turn
        private HashSet<BoardPosition> _ralliedPieces = new HashSet<BoardPosition>();
        
        // Track which pieces are protected by Shield
        private HashSet<BoardPosition> _shieldedPieces = new HashSet<BoardPosition>();
        
        // Track if a piece has been given extra move by Inspire
        private ChessPiece _inspiredPiece;
        private bool _inspiredPieceUsedExtraMove;

        public CommanderAbilityHandler(ChessBoard board)
        {
            _board = board;
        }

        /// <summary>
        /// Execute a commander ability
        /// </summary>
        public bool ExecuteAbility(Commander commander, CommanderAbility ability, BoardPosition? targetPosition = null)
        {
            if (!commander.CanUseAbility(ability))
                return false;

            bool success = ability switch
            {
                CommanderAbility.Rally => ExecuteRally(commander),
                CommanderAbility.Charge => ExecuteCharge(commander, targetPosition),
                CommanderAbility.Shield => ExecuteShield(commander),
                CommanderAbility.Tactics => ExecuteTactics(commander, targetPosition),
                CommanderAbility.Inspire => ExecuteInspire(commander, targetPosition),
                _ => false
            };

            if (success)
            {
                commander.UseAbility(ability);
            }

            return success;
        }

        /// <summary>
        /// Rally: Boost nearby allied pieces (they can capture on the same turn)
        /// </summary>
        private bool ExecuteRally(Commander commander)
        {
            var targets = commander.GetRallyTargets(_board.GetBoardState());
            
            if (targets.Count == 0)
                return false;

            _ralliedPieces.Clear();
            foreach (var pos in targets)
            {
                _ralliedPieces.Add(pos);
            }

            return true;
        }

        /// <summary>
        /// Charge: Commander can move up to 3 extra squares in one direction
        /// </summary>
        private bool ExecuteCharge(Commander commander, BoardPosition? targetPosition)
        {
            if (!targetPosition.HasValue)
                return false;

            var target = targetPosition.Value;
            
            // Validate charge move (straight line, up to 3 extra squares)
            var from = commander.Position;
            int rowDiff = target.Row - from.Row;
            int colDiff = target.Column - from.Column;

            // Must be in a straight line (horizontal, vertical, or diagonal)
            bool isHorizontal = rowDiff == 0 && colDiff != 0;
            bool isVertical = colDiff == 0 && rowDiff != 0;
            bool isDiagonal = System.Math.Abs(rowDiff) == System.Math.Abs(colDiff);

            if (!isHorizontal && !isVertical && !isDiagonal)
                return false;

            // Max distance is commander's normal move + 3
            int maxDist = 5; // Commander can already move like a knight, so charge adds 3
            int distance = System.Math.Max(System.Math.Abs(rowDiff), System.Math.Abs(colDiff));
            
            if (distance > maxDist)
                return false;

            // Check path is clear
            int rowStep = rowDiff == 0 ? 0 : rowDiff / System.Math.Abs(rowDiff);
            int colStep = colDiff == 0 ? 0 : colDiff / System.Math.Abs(colDiff);

            var board = _board.GetBoardState();
            for (int i = 1; i < distance; i++)
            {
                var checkPos = new BoardPosition(from.Row + rowStep * i, from.Column + colStep * i);
                if (board[checkPos.Row, checkPos.Column] != null)
                    return false;
            }

            // Target must be empty or enemy
            var targetPiece = board[target.Row, target.Column];
            if (targetPiece != null && targetPiece.Side == commander.Side)
                return false;

            return true;
        }

        /// <summary>
        /// Shield: Protect adjacent pieces from capture for one turn
        /// </summary>
        private bool ExecuteShield(Commander commander)
        {
            var targets = commander.GetRallyTargets(_board.GetBoardState());
            
            _shieldedPieces.Clear();
            foreach (var pos in targets)
            {
                _shieldedPieces.Add(pos);
            }

            // Also protect the commander itself
            _shieldedPieces.Add(commander.Position);

            return true;
        }

        /// <summary>
        /// Tactics: Swap positions with a friendly piece within 2 squares
        /// </summary>
        private bool ExecuteTactics(Commander commander, BoardPosition? targetPosition)
        {
            if (!targetPosition.HasValue)
                return false;

            var target = targetPosition.Value;
            var targets = commander.GetTacticsTargets(_board.GetBoardState());

            if (!targets.Contains(target))
                return false;

            var targetPiece = _board.GetPieceAt(target);
            if (targetPiece == null || targetPiece.Side != commander.Side)
                return false;

            // Swap positions
            var commanderPos = commander.Position;
            var board = _board.GetBoardState();

            board[commanderPos.Row, commanderPos.Column] = targetPiece;
            board[target.Row, target.Column] = commander;

            commander.Position = target;
            targetPiece.Position = commanderPos;

            return true;
        }

        /// <summary>
        /// Inspire: Give an adjacent friendly piece an extra move this turn
        /// </summary>
        private bool ExecuteInspire(Commander commander, BoardPosition? targetPosition)
        {
            if (!targetPosition.HasValue)
                return false;

            var target = targetPosition.Value;
            var targets = commander.GetInspireTargets(_board.GetBoardState());

            if (!targets.Contains(target))
                return false;

            var targetPiece = _board.GetPieceAt(target);
            if (targetPiece == null)
                return false;

            _inspiredPiece = targetPiece;
            _inspiredPieceUsedExtraMove = false;

            return true;
        }

        /// <summary>
        /// Check if a piece is protected by Shield
        /// </summary>
        public bool IsPieceShielded(BoardPosition position)
        {
            return _shieldedPieces.Contains(position);
        }

        /// <summary>
        /// Check if a piece has been rallied
        /// </summary>
        public bool IsPieceRallied(BoardPosition position)
        {
            return _ralliedPieces.Contains(position);
        }

        /// <summary>
        /// Check if a piece can make an extra move (from Inspire)
        /// </summary>
        public bool CanMakeInspiredMove(ChessPiece piece)
        {
            return _inspiredPiece == piece && !_inspiredPieceUsedExtraMove;
        }

        /// <summary>
        /// Mark the inspired extra move as used
        /// </summary>
        public void UseInspiredMove()
        {
            _inspiredPieceUsedExtraMove = true;
        }

        /// <summary>
        /// Reset ability effects at the start of a turn
        /// </summary>
        public void OnTurnStart(PlayerSide side)
        {
            _ralliedPieces.Clear();
            _shieldedPieces.Clear();
            _inspiredPiece = null;
            _inspiredPieceUsedExtraMove = false;

            // Update commander cooldowns
            var commander = _board.GetCommander(side);
            if (commander != null)
            {
                commander.OnTurnStart();
            }
        }

        /// <summary>
        /// Get available abilities for a commander
        /// </summary>
        public List<CommanderAbility> GetAvailableAbilities(Commander commander)
        {
            var abilities = new List<CommanderAbility>();

            if (commander == null || commander.IsCaptured)
                return abilities;

            var allAbilities = new[] 
            { 
                CommanderAbility.Rally, 
                CommanderAbility.Charge, 
                CommanderAbility.Shield,
                CommanderAbility.Tactics,
                CommanderAbility.Inspire
            };

            foreach (var ability in allAbilities)
            {
                if (commander.CanUseAbility(ability))
                {
                    abilities.Add(ability);
                }
            }

            return abilities;
        }

        /// <summary>
        /// Get description of an ability
        /// </summary>
        public static string GetAbilityDescription(CommanderAbility ability)
        {
            return ability switch
            {
                CommanderAbility.Rally => "Rally: Boost nearby allied pieces for this turn",
                CommanderAbility.Charge => "Charge: Move up to 3 extra squares in a straight line",
                CommanderAbility.Shield => "Shield: Protect adjacent pieces from capture this turn",
                CommanderAbility.Tactics => "Tactics: Swap positions with a friendly piece within 2 squares",
                CommanderAbility.Inspire => "Inspire: Give an adjacent friendly piece an extra move",
                _ => "Unknown ability"
            };
        }
    }
}
