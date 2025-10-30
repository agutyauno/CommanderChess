using VContainer;

public class ZoneProvider
{
    ROFZone ROF;

    [Inject]
    public ZoneProvider(Board board)
    {
        ROF = new(board);
    }
    public IZone GetDangerZonesForPiece(BasePiece piece)
    {
        switch (piece.Type)
        {
            case BasePiece.PieceType.AirForce:
                return ROF;
            default:
                return null;
        }
    }
}
