using VContainer;
using CommanderChess.Domain;

namespace CommanderChess.Services
{
    public class ZoneProvider : BaseService
    {
        readonly ROFZone ROF;
        readonly CommanderZone commanderZone;
        
        [Inject] readonly CarryingSystem carryingSystem;

        [Inject]
        public ZoneProvider(Board board, CarryingSystem carryingSystem)
        {
            ROF = new(board);
            commanderZone = new(board, carryingSystem);
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

        public IZone GetROF()
        {
            return ROF;
        }

        public IZone GetCommanderZone()
        {
            return commanderZone;
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
            // Check if commander moved
            if (evt.Piece.Type == BasePiece.PieceType.Commander)
            {
                commanderZone.MarkDirty();
            }

            // Check if any carried piece is commander
            var carriedPieces = carryingSystem.GetAllCarriedPieces(evt.Piece);
            foreach (var passenger in carriedPieces)
            {
                if (passenger.Type == BasePiece.PieceType.Commander)
                {
                    commanderZone.MarkDirty();
                    break;
                }
            }

            // Check ROF zone
            if (evt.Piece.HadRingOfFire)
            {
                ROF.MarkDirty();
                return;
            }

            // Check carried pieces for ROF
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
            // Check if commander was captured
            if (evt.Defender.Type == BasePiece.PieceType.Commander)
            {
                commanderZone.MarkDirty();
            }

            // Check if defender was carrying commander
            var carriedPieces = carryingSystem.GetAllCarriedPieces(evt.Defender);
            foreach (var passenger in carriedPieces)
            {
                if (passenger.Type == BasePiece.PieceType.Commander)
                {
                    commanderZone.MarkDirty();
                    break;
                }
            }

            // Check ROF zone - defender destroyed
            if (evt.Defender.HadRingOfFire)
            {
                ROF.MarkDirty();
                return;
            }

            // Defender carry pieces với ROF
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
        /// Khi piece boarding - passenger có thể có ROF hoặc là commander
        /// </summary>
        void OnPieceBoarded(PieceBoardedEvent evt)
        {
            // Check if passenger is commander
            if (evt.Passenger.Type == BasePiece.PieceType.Commander)
            {
                commanderZone.MarkDirty();
            }

            // Check if carrier is commander
            if (evt.Carrier.Type == BasePiece.PieceType.Commander)
            {
                commanderZone.MarkDirty();
            }

            // Check nếu passenger đang carry commander (nested carrying)
            var nestedCarried = carryingSystem.GetAllCarriedPieces(evt.Passenger);
            foreach (var nested in nestedCarried)
            {
                if (nested.Type == BasePiece.PieceType.Commander)
                {
                    commanderZone.MarkDirty();
                    break;
                }
            }

            // Check ROF - passenger
            if (evt.Passenger.HadRingOfFire)
            {
                ROF.MarkDirty();
                return;
            }

            // Check nếu passenger đang carry pieces khác với ROF (nested carrying)
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
            // Check if passenger is commander
            if (evt.Passenger.Type == BasePiece.PieceType.Commander)
            {
                commanderZone.MarkDirty();
            }

            // Check nếu passenger đang carry commander
            var nestedCarried = carryingSystem.GetAllCarriedPieces(evt.Passenger);
            foreach (var nested in nestedCarried)
            {
                if (nested.Type == BasePiece.PieceType.Commander)
                {
                    commanderZone.MarkDirty();
                    break;
                }
            }

            // Check ROF - passenger
            if (evt.Passenger.HadRingOfFire)
            {
                ROF.MarkDirty();
                return;
            }

            // Check nếu passenger đang carry pieces khác với ROF
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
