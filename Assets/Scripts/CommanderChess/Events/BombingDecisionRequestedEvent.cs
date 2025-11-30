using CommanderChess.Domain;

namespace CommanderChess.Services
{
    /// <summary>
    /// Event triggered when Airforce needs to make bombing decision (Stay or Return)
    /// </summary>
    public readonly struct BombingDecisionRequestedEvent : IGameEvent
    {
        public readonly BasePiece Airforce;
        public readonly BoardCoord OriginalPosition;

        public BombingDecisionRequestedEvent(BasePiece airforce, BoardCoord originalPosition)
        {
            Airforce = airforce;
            OriginalPosition = originalPosition;
        }
    }
}
