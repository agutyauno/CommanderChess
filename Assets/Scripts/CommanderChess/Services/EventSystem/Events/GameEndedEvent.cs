using CommanderChess.Domain;

namespace CommanderChess.Services
{
    /// <summary>
    /// Fired when game ends (win/draw/forfeit)
    /// </summary>
    public readonly struct GameEndedEvent : IGameEvent
    {
        public readonly Team? Winner; // null = draw
        public readonly string Reason;
        
        public GameEndedEvent(Team? winner, string reason)
        {
            Winner = winner;
            Reason = reason;
        }
    }
}