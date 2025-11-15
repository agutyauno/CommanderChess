using CommanderChess.Domain;

namespace CommanderChess.Services
{
    /// <summary>
    /// Fired when a new turn starts (after EndTurn is called)
    /// </summary>
    public readonly struct TurnStartedEvent : IGameEvent
    {
        public readonly Team Team;
        public readonly int TurnNumber;
        
        public TurnStartedEvent(Team team, int turnNumber)
        {
            Team = team;
            TurnNumber = turnNumber;
        }
    }
}