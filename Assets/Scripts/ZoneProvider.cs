public class ZoneProvider
{
    ROFZone ROF = new();
    public IZone GetDangerZonesForPiece(Piece piece)
    {
        switch (piece.Type)
        {
            case Piece.PieceType.AirForce:
                return ROF;
            default:
                return null;
        }
    }
}
