using CommanderChess.Domain;

namespace CommanderChess.Services
{
    public readonly struct PieceSelectedEvent : IGameEvent
    {
        public readonly BasePiece Piece;

        public PieceSelectedEvent(BasePiece piece)
        {
            Piece = piece;
        }
    }

}