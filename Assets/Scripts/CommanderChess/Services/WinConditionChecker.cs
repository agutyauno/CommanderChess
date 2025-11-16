using System.Linq;
using CommanderChess.Domain;
using CommanderChess.Events;
using VContainer;

namespace CommanderChess.Services
{
    /// <summary>
    /// Checks win conditions with priority: Commander Death > All Navy Lost = All Airforce Lost = All Ground Units Lost
    /// </summary>
    public class WinConditionChecker
    {
        private readonly Board board;
        private readonly GameStatsTracker statsTracker;
        private readonly EventBus eventBus;

        [Inject]
        public WinConditionChecker(Board board, GameStatsTracker statsTracker, EventBus eventBus)
        {
            this.board = board;
            this.statsTracker = statsTracker;
            this.eventBus = eventBus;
        }

        /// <summary>
        /// Check win conditions after command execution. Priority: Commander Death > Unit Losses
        /// </summary>
        public bool CheckWinConditions()
        {
            // Priority 1: Commander death (instant win)
            if (CheckCommanderLost(Team.Red, out var redCommanderCondition))
            {
                PublishWin(Team.Blue, Team.Red, redCommanderCondition);
                return true;
            }
            if (CheckCommanderLost(Team.Blue, out var blueCommanderCondition))
            {
                PublishWin(Team.Red, Team.Blue, blueCommanderCondition);
                return true;
            }

            // Priority 2: Unit loss conditions (equal priority, check all)
            if (CheckAllNavyLost(Team.Red))
            {
                PublishWin(Team.Blue, Team.Red, WinCondition.AllNavyLost);
                return true;
            }
            if (CheckAllNavyLost(Team.Blue))
            {
                PublishWin(Team.Red, Team.Blue, WinCondition.AllNavyLost);
                return true;
            }

            if (CheckAllAirforceLost(Team.Red))
            {
                PublishWin(Team.Blue, Team.Red, WinCondition.AllAirforceLost);
                return true;
            }
            if (CheckAllAirforceLost(Team.Blue))
            {
                PublishWin(Team.Red, Team.Blue, WinCondition.AllAirforceLost);
                return true;
            }

            if (CheckAllGroundUnitsLost(Team.Red))
            {
                PublishWin(Team.Blue, Team.Red, WinCondition.AllGroundUnitsLost);
                return true;
            }
            if (CheckAllGroundUnitsLost(Team.Blue))
            {
                PublishWin(Team.Red, Team.Blue, WinCondition.AllGroundUnitsLost);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Handle surrender action
        /// </summary>
        public void Surrender(Team surrenderingTeam)
        {
            Team winner = surrenderingTeam == Team.Red ? Team.Blue : Team.Red;
            PublishWin(winner, surrenderingTeam, WinCondition.Surrender);
        }

        private bool CheckCommanderLost(Team team, out WinCondition condition)
        {
            condition = WinCondition.CommanderKilled;
            var losses = statsTracker.GetLosses(team);
            return losses.CommanderLost;
        }

        private bool CheckAllNavyLost(Team team)
        {
            // Check if all navy pieces are lost
            var allNavy = board.Pieces.Values
                .Where(p => p.Team == team && p.Type == BasePiece.PieceType.Navy)
                .ToList();
            
            return allNavy.Count == 0;
        }

        private bool CheckAllAirforceLost(Team team)
        {
            // Check if all airforce pieces are lost
            var allAirforce = board.Pieces.Values
                .Where(p => p.Team == team && p.Type == BasePiece.PieceType.AirForce)
                .ToList();
            
            return allAirforce.Count == 0;
        }

        private bool CheckAllGroundUnitsLost(Team team)
        {
            // Check if ALL infantry + tank + artillery are lost
            var allGroundUnits = board.Pieces.Values
                .Where(p => p.Team == team && 
                           (p.Type == BasePiece.PieceType.Infantry || 
                            p.Type == BasePiece.PieceType.Tank || 
                            p.Type == BasePiece.PieceType.Artillery))
                .ToList();
            
            return allGroundUnits.Count == 0;
        }

        private void PublishWin(Team winner, Team loser, WinCondition condition)
        {
            eventBus.Publish(new GameWonEvent(winner, loser, condition));
        }
    }
}
