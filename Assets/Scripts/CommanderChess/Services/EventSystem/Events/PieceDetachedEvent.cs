using CommanderChess.Domain;

namespace CommanderChess.Services
{
    public readonly struct PieceDetachedEvent : IGameEvent
    {
        public readonly BasePiece Passenger;
        public readonly BoardCoord CarrierPosition;
        public readonly BoardCoord DetachPosition;
        
        public PieceDetachedEvent(BasePiece passenger, BoardCoord carrierPos, BoardCoord detachPos)
        {
            Passenger = passenger;
            CarrierPosition = carrierPos;
            DetachPosition = detachPos;
        }
    }

}