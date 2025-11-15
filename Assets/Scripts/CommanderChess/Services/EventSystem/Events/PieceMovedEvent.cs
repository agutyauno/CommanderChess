using CommanderChess.Domain;

namespace CommanderChess.Services
{
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
}