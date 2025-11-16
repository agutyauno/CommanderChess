using CommanderChess.Domain;

namespace CommanderChess.Services
{
    /// <summary>
    /// Fired when player confirms end turn (Confirm button clicked)
    /// </summary>
    public readonly struct TurnEndedEvent : IGameEvent
    {
        public readonly Team Team;
        public readonly int TurnNumber;

        public TurnEndedEvent(Team team, int turnNumber)
        {
            Team = team;
            TurnNumber = turnNumber;
        }
    }
}
