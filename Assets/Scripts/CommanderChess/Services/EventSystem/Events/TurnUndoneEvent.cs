using CommanderChess.Domain;

namespace CommanderChess.Services
{
    /// <summary>
    /// Fired when a turn is undone and restored to previous state
    /// </summary>
    public readonly struct TurnUndoneEvent : IGameEvent
    {
        public readonly int RestoredTurn;
        public readonly Team RestoredTeam;
        
        public TurnUndoneEvent(int restoredTurn, Team restoredTeam)
        {
            RestoredTurn = restoredTurn;
            RestoredTeam = restoredTeam;
        }
    }
}
