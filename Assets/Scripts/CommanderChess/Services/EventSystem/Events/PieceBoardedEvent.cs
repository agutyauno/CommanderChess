using CommanderChess.Domain;

namespace CommanderChess.Services
{
    public readonly struct PieceBoardedEvent : IGameEvent
    {
        public readonly BasePiece Carrier;
        public readonly BasePiece Passenger;
        public readonly BoardCoord From;
        public readonly BoardCoord To;
        
        public PieceBoardedEvent(BasePiece carrier, BasePiece passenger, BoardCoord from, BoardCoord to)
        {
            Carrier = carrier;
            Passenger = passenger;
            From = from;
            To = to;
        }
    }
}