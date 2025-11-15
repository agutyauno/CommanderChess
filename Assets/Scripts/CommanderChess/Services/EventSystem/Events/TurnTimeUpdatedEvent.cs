using CommanderChess.Domain;

namespace CommanderChess.Services
{
    /// <summary>
    /// Fired when turn timer updates (for future timed games)
    /// </summary>
    public readonly struct TurnTimeUpdatedEvent : IGameEvent
    {
        public readonly Team Team;
        public readonly float RemainingSeconds;
        
        public TurnTimeUpdatedEvent(Team team, float remainingSeconds)
        {
            Team = team;
            RemainingSeconds = remainingSeconds;
        }
    }
}