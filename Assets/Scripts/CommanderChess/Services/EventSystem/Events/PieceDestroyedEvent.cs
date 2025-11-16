using CommanderChess.Domain;

namespace CommanderChess.Services
{
    /// <summary>
    /// Event fired when a piece is destroyed (e.g., carrier destroyed in ROF zone)
    /// </summary>
    public readonly struct PieceDestroyedEvent : IGameEvent
    {
        public readonly BasePiece DestroyedPiece;
        public readonly BoardCoord Position;
        
        public PieceDestroyedEvent(BasePiece destroyedPiece, BoardCoord position)
        {
            DestroyedPiece = destroyedPiece;
            Position = position;
        }
    }
}
