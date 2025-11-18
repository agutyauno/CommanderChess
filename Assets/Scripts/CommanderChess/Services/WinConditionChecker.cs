using System.Linq;
using CommanderChess.Domain;
using CommanderChess.Events;
using UnityEngine;
using VContainer;

namespace CommanderChess.Services
{
    /// <summary>
    /// Checks win conditions with priority: Commander Death = Commander Confrontation > Unit Losses
    /// </summary>
    public class WinConditionChecker
    {
        private readonly Board board;
        private readonly GameStatsTracker statsTracker;
        private readonly EventBus eventBus;
        private readonly ZoneProvider zoneProvider;
        private readonly CarryingSystem carryingSystem;

        [Inject]
        public WinConditionChecker(Board board, GameStatsTracker statsTracker, EventBus eventBus, 
            ZoneProvider zoneProvider, CarryingSystem carryingSystem)
        {
            this.board = board;
            this.statsTracker = statsTracker;
            this.eventBus = eventBus;
            this.zoneProvider = zoneProvider;
            this.carryingSystem = carryingSystem;
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
                Debug.Log("Red Commander lost. Blue team wins!");
                return true;
            }
            if (CheckCommanderLost(Team.Blue, out var blueCommanderCondition))
            {
                PublishWin(Team.Red, Team.Blue, blueCommanderCondition);
                Debug.Log("Blue Commander lost. Red team wins!");
                return true;
            }

            // Priority 2: Unit loss conditions (equal priority, check all)
            if (CheckAllNavyLost(Team.Red))
            {
                PublishWin(Team.Blue, Team.Red, WinCondition.AllNavyLost);
                Debug.Log("Red team lost all navy units. Blue team wins!");
                return true;
            }
            if (CheckAllNavyLost(Team.Blue))
            {
                PublishWin(Team.Red, Team.Blue, WinCondition.AllNavyLost);
                Debug.Log("Blue team lost all navy units. Red team wins!");
                return true;
            }

            if (CheckAllAirforceLost(Team.Red))
            {
                PublishWin(Team.Blue, Team.Red, WinCondition.AllAirforceLost);
                Debug.Log("Red team lost all airforce units. Blue team wins!");
                return true;
            }
            if (CheckAllAirforceLost(Team.Blue))
            {
                PublishWin(Team.Red, Team.Blue, WinCondition.AllAirforceLost);
                Debug.Log("Blue team lost all airforce units. Red team wins!");
                return true;
            }

            if (CheckAllGroundUnitsLost(Team.Red))
            {
                PublishWin(Team.Blue, Team.Red, WinCondition.AllGroundUnitsLost);
                Debug.Log("Red team lost all ground units. Blue team wins!");
                return true;
            }
            if (CheckAllGroundUnitsLost(Team.Blue))
            {
                PublishWin(Team.Red, Team.Blue, WinCondition.AllGroundUnitsLost);
                Debug.Log("Blue team lost all ground units. Red team wins!");
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

        /// <summary>
        /// Check commander confrontation at end of turn
        /// Commander in enemy Commander Zone (not carried) loses the game
        /// </summary>
        public bool CheckCommanderConfrontation()
        {
            var commanderZone = zoneProvider.GetCommanderZone();
            if (commanderZone == null) return false;

            // Check Red Commander
            var redCommander = board.Pieces.Values
                .FirstOrDefault(p => p.Team == Team.Red && p.Type == BasePiece.PieceType.Commander);
            
            if (redCommander != null && !carryingSystem.IsCarried(redCommander))
            {
                var blueZone = commanderZone.GetZoneByTeam(Team.Blue);
                if (blueZone != null && blueZone.Contains(redCommander.Position))
                {
                    // Red Commander in Blue zone -> Red loses
                    PublishWin(Team.Blue, Team.Red, WinCondition.CommanderConfrontation);
                    Debug.Log("Red Commander in Blue Commander Zone! Blue team wins!");
                    return true;
                }
            }

            // Check Blue Commander
            var blueCommander = board.Pieces.Values
                .FirstOrDefault(p => p.Team == Team.Blue && p.Type == BasePiece.PieceType.Commander);
            
            if (blueCommander != null && !carryingSystem.IsCarried(blueCommander))
            {
                var redZone = commanderZone.GetZoneByTeam(Team.Red);
                if (redZone != null && redZone.Contains(blueCommander.Position))
                {
                    // Blue Commander in Red zone -> Blue loses
                    PublishWin(Team.Red, Team.Blue, WinCondition.CommanderConfrontation);
                    Debug.Log("Blue Commander in Red Commander Zone! Red team wins!");
                    return true;
                }
            }

            return false;
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
