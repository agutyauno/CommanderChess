using CommanderChess.Domain;

namespace CommanderChess.Services
{
    /// <summary>
    /// Fired when a detach occurred and only specific piece can continue to act
    /// </summary>
    public readonly struct TurnDetachOccurredEvent : IGameEvent
    {
        public readonly BasePiece AllowedPiece; // The carrier that can still act
        public readonly Team Team;
        public readonly int TurnNumber;

        public TurnDetachOccurredEvent(BasePiece allowedPiece, Team team, int turnNumber)
        {
            AllowedPiece = allowedPiece;
            Team = team;
            TurnNumber = turnNumber;
        }
    }
}
