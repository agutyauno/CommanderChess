using VContainer;
using CommanderChess.Domain;

namespace CommanderChess.Services
{
    public class ZoneProvider : BaseService
    {
        readonly ROFZone ROF;
        
        [Inject] readonly CarryingSystem carryingSystem;

        [Inject]
        public ZoneProvider(Board board, CarryingSystem carryingSystem)
        {
            ROF = new(board);
            this.carryingSystem = carryingSystem;
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

        protected override void SubscribeEvents()
        {
            eventBus.Subscribe<PieceMovedEvent>(OnPieceMoved);
            eventBus.Subscribe<PieceCapturedEvent>(OnPieceCaptured);
            eventBus.Subscribe<PieceBoardedEvent>(OnPieceBoarded);
            eventBus.Subscribe<PieceDetachedEvent>(OnPieceDetached);
        }

        protected override void UnsubscribeEvents()
        {
            eventBus.Unsubscribe<PieceMovedEvent>(OnPieceMoved);
            eventBus.Unsubscribe<PieceCapturedEvent>(OnPieceCaptured);
            eventBus.Unsubscribe<PieceBoardedEvent>(OnPieceBoarded);
            eventBus.Unsubscribe<PieceDetachedEvent>(OnPieceDetached);
        }

        /// <summary>
        /// Khi piece di chuyển - check piece và carried pieces
        /// </summary>
        void OnPieceMoved(PieceMovedEvent evt)
        {
            // Check piece chính
            if (evt.Piece.HadRingOfFire)
            {
                ROF.MarkDirty();
                return;
            }

            // Check carried pieces (nếu carrier di chuyển, passengers cũng move)
            var carriedPieces = carryingSystem.GetAllCarriedPieces(evt.Piece);
            foreach (var passenger in carriedPieces)
            {
                if (passenger.HadRingOfFire)
                {
                    ROF.MarkDirty();
                    return;
                }
            }
        }

        /// <summary>
        /// Khi piece bị capture - check defender (bị destroyed)
        /// </summary>
        void OnPieceCaptured(PieceCapturedEvent evt)
        {
            // Defender bị destroyed
            if (evt.Defender.HadRingOfFire)
            {
                ROF.MarkDirty();
                return;
            }

            // Defender carry pieces với ROF
            var carriedPieces = carryingSystem.GetAllCarriedPieces(evt.Defender);
            foreach (var passenger in carriedPieces)
            {
                if (passenger.HadRingOfFire)
                {
                    ROF.MarkDirty();
                    return;
                }
            }
        }

        /// <summary>
        /// Khi piece boarding - passenger có thể có ROF
        /// </summary>
        void OnPieceBoarded(PieceBoardedEvent evt)
        {
            // Check passenger
            if (evt.Passenger.HadRingOfFire)
            {
                ROF.MarkDirty();
                return;
            }

            // Check nếu passenger đang carry pieces khác (nested carrying)
            var nestedCarried = carryingSystem.GetAllCarriedPieces(evt.Passenger);
            foreach (var nested in nestedCarried)
            {
                if (nested.HadRingOfFire)
                {
                    ROF.MarkDirty();
                    return;
                }
            }

            // Check carrier (nếu carrier có ROF thì cũng cần update)
            if (evt.Carrier.HadRingOfFire)
            {
                ROF.MarkDirty();
            }
        }

        /// <summary>
        /// Khi piece detach - passenger rời carrier
        /// </summary>
        void OnPieceDetached(PieceDetachedEvent evt)
        {
            // Check passenger
            if (evt.Passenger.HadRingOfFire)
            {
                ROF.MarkDirty();
                return;
            }

            // Check nếu passenger đang carry pieces khác
            var nestedCarried = carryingSystem.GetAllCarriedPieces(evt.Passenger);
            foreach (var nested in nestedCarried)
            {
                if (nested.HadRingOfFire)
                {
                    ROF.MarkDirty();
                    return;
                }
            }
        }
    }
}
