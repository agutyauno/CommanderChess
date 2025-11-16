using CommanderChess.Domain;

namespace CommanderChess.Services
{
    /// <summary>
    /// Fired when player cancels end turn confirmation (Cancel button clicked)
    /// Turn is restored to snapshot at turn start
    /// </summary>
    public readonly struct TurnEndCancelledEvent : IGameEvent
    {
        public readonly Team Team;
        public readonly int TurnNumber;

        public TurnEndCancelledEvent(Team team, int turnNumber)
        {
            Team = team;
            TurnNumber = turnNumber;
        }
    }
}
