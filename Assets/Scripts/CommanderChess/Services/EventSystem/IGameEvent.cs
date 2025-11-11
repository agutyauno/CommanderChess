using System.Collections.Generic;
using CommanderChess.Domain;

namespace CommanderChess.Services
{
    public interface IGameEvent
    {
    }
    
    // Movement Events
    public readonly struct PieceMovedEvent : IGameEvent
    {
        public readonly BasePiece Piece;
        public readonly BoardCoord From;
        public readonly BoardCoord To;
        
        public PieceMovedEvent(BasePiece piece, BoardCoord from, BoardCoord to)
        {
            Piece = piece;
            From = from;
            To = to;
        }
    }

    public readonly struct PieceCapturedEvent : IGameEvent
    {
        public readonly BasePiece Attacker;
        public readonly BasePiece Defender;
        public readonly BoardCoord From;
        public readonly BoardCoord To;
        
        public PieceCapturedEvent(BasePiece attacker, BasePiece defender, BoardCoord from, BoardCoord to)
        {
            Attacker = attacker;
            Defender = defender;
            From = from;
            To = to;
        }
    }
    
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
