using CommanderChess.Domain;

namespace CommanderChess.Services
{
    /// <summary>
    /// Fired when turn changes from one team to another
    /// </summary>
    public readonly struct TurnChangedEvent : IGameEvent
    {
        public readonly Team NewTurn;
        public readonly Team PreviousTurn;
        public readonly int TurnNumber;
        
        public TurnChangedEvent(Team newTurn, Team previousTurn, int turnNumber)
        {
            NewTurn = newTurn;
            PreviousTurn = previousTurn;
            TurnNumber = turnNumber;
        }
    }
}