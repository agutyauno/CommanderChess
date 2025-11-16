using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CommanderChess.Domain;

namespace CommanderChess.Services
{
    /// <summary>
    /// CarryingSystem - Hệ thống quản lý quan hệ mang/được mang giữa các quân cờ
    /// Quy tắc chính:
    /// - Mỗi piece có maxCarryCapacity riêng (từ PieceData khi register)
    /// - Tổng quân trong group (carrier + carrying + nested) <= 3
    /// - Tự động phân phối thông minh khi boarding
    /// - Hỗ trợ TryAddCarry(A, B) tự động xác định ai mang ai
    /// </summary>
    public class CarryingSystem
    {
        // Lưu thông tin về quan hệ mang/được mang
        private readonly Dictionary<BasePiece, CarryingNode> carryingNodes = new();

        /// <summary>
        /// Node đại diện cho một quân cờ trong hệ thống mang
        /// </summary>
        public class CarryingNode
        {
            public BasePiece Piece { get; }
            public BasePiece Carrier { get; set; } // Quân đang mang mình
            public List<BasePiece> Carrying { get; } // Dynamic capacity
            
            private readonly int maxCapacity;

            public CarryingNode(BasePiece piece, int capacity)
            {
                Piece = piece;
                maxCapacity = capacity;
                Carrying = new List<BasePiece>(capacity);
            }

            public int MaxCapacity => maxCapacity;
            public int CurrentLoad => Carrying.Count;
            public bool HasFreeSlot => Carrying.Count < maxCapacity;
            public int FreeSlots => maxCapacity - Carrying.Count;
            public bool IsCarried => Carrier != null;
        }

        #region Public API

        /// <summary>
        /// Đăng ký một quân cờ vào hệ thống với capacity từ PieceData
        /// </summary>
        public void RegisterPiece(BasePiece piece)
        {
            if (!carryingNodes.ContainsKey(piece))
            {
                int capacity = piece.MaxCarryCapacity; // Lấy từ PieceData
                carryingNodes[piece] = new CarryingNode(piece, capacity);
            }
        }

        /// <summary>
        /// Hủy đăng ký quân cờ khỏi hệ thống
        /// </summary>
        public void UnregisterPiece(BasePiece piece)
        {
            if (carryingNodes.TryGetValue(piece, out var node))
            {
                // Tách tất cả quân đang mang
                var carried = node.Carrying.ToList();
                foreach (var p in carried)
                {
                    Detach(p);
                }

                // Tách khỏi carrier nếu có
                if (node.Carrier != null)
                {
                    Detach(piece);
                }

                carryingNodes.Remove(piece);
            }
        }

        /// <summary>
        /// TryAddCarry - Tự động xác định ai mang ai và phân phối thông minh
        /// </summary>
        public bool TryAddCarry(BasePiece A, BasePiece B)
        {
            BasePiece passenger;
            BasePiece carrier;

            // Xác định carrier và passenger
            if (CanCarryDirectly(A, B))
            {
                carrier = A;
                passenger = B;
            }
            else if (CanCarryDirectly(B, A))
            {
                carrier = B;
                passenger = A;
            }
            else
            {
                Debug.LogWarning($"Neither {A.Type} nor {B.Type} can carry the other");
                return false;
            }

            if (!carrier.CanCarryOthers)
            {
                Debug.LogWarning($"{carrier.Type} cannot carry others");
                return false;
            }

            var carrierNode = carryingNodes[carrier];
            Debug.Log($"Attempting: {carrier.Type} (capacity {carrierNode.CurrentLoad}/{carrierNode.MaxCapacity}) carry {passenger.Type}");

            // Validate carrying
            if (!CanCarry(carrier, passenger, out string reason))
            {
                Debug.LogWarning($"Cannot carry: {reason}");
                return false;
            }

            // Thực hiện carrying với redistribution
            return ExecuteCarryWithRedistribution(carrier, passenger);
        }

        /// <summary>
        /// Tách một quân ra khỏi carrier
        /// </summary>
        public bool Detach(BasePiece piece)
        {
            if (!carryingNodes.TryGetValue(piece, out var node))
                return false;

            if (node.Carrier == null)
                return false;

            var carrier = node.Carrier;
            var carrierNode = carryingNodes[carrier];

            carrierNode.Carrying.Remove(piece);
            node.Carrier = null;

            Debug.Log($"Detached {piece.Type} from {carrier.Type} (carrier now: {carrierNode.CurrentLoad}/{carrierNode.MaxCapacity})");
            return true;
        }

        /// <summary>
        /// Lấy tất cả quân mà piece đang mang (không đệ quy)
        /// </summary>
        public List<BasePiece> GetDirectCarrying(BasePiece piece)
        {
            return carryingNodes.TryGetValue(piece, out var node)
                ? new List<BasePiece>(node.Carrying)
                : new List<BasePiece>();
        }

        /// <summary>
        /// Lấy tất cả quân trong group (đệ quy)
        /// </summary>
        public List<BasePiece> GetAllCarriedPieces(BasePiece piece)
        {
            var result = new List<BasePiece>();
            if (!carryingNodes.TryGetValue(piece, out var node))
                return result;

            foreach (var carried in node.Carrying)
            {
                result.Add(carried);
                result.AddRange(GetAllCarriedPieces(carried));
            }

            return result;
        }

        /// <summary>
        /// Đếm tổng số quân trong group
        /// </summary>
        public int CountGroupSize(BasePiece piece)
        {
            return 1 + GetAllCarriedPieces(piece).Count;
        }

        /// <summary>
        /// Lấy thông tin capacity của piece
        /// </summary>
        public (int current, int max) GetCapacityInfo(BasePiece piece)
        {
            if (carryingNodes.TryGetValue(piece, out var node))
            {
                return (node.CurrentLoad, node.MaxCapacity);
            }
            return (0, 0);
        }

        /// <summary>
        /// Lấy carrier gốc (top-level)
        /// </summary>
        public BasePiece GetRootCarrier(BasePiece piece)
        {
            if (!carryingNodes.TryGetValue(piece, out var node))
                return piece;

            var current = piece;
            while (carryingNodes[current].Carrier != null)
            {
                current = carryingNodes[current].Carrier;
            }

            return current;
        }

        /// <summary>
        /// Kiểm tra xem piece có đang được mang không
        /// </summary>
        public bool IsCarried(BasePiece piece)
        {
            return carryingNodes.TryGetValue(piece, out var node) && node.Carrier != null;
        }

        /// <summary>
        /// Lấy carrier trực tiếp
        /// </summary>
        public BasePiece GetCarrier(BasePiece piece)
        {
            return carryingNodes.TryGetValue(piece, out var node) ? node.Carrier : null;
        }

        /// <summary>
        /// Kiểm tra carrier có thể mang passenger không (theo type)
        /// </summary>
        public bool CanCarryDirectly(BasePiece carrier, BasePiece passenger)
        {
            return carrier.AllowedCarryTypes.Contains(passenger.Type);
        }

        #endregion

        #region Private Validation

        private bool CanCarry(BasePiece carrier, BasePiece passenger, out string reason)
        {
            reason = "";

            if (!carryingNodes.ContainsKey(carrier) || !carryingNodes.ContainsKey(passenger))
            {
                reason = "Piece not registered";
                return false;
            }

            if (carrier == passenger)
            {
                reason = "Cannot carry itself";
                return false;
            }

            if (carrier.Team != passenger.Team)
            {
                reason = "Different teams";
                return false;
            }

            if (!carrier.AllowedCarryTypes.Contains(passenger.Type))
            {
                reason = $"{carrier.Type} cannot carry {passenger.Type}";
                return false;
            }

            var carrierNode = carryingNodes[carrier];

            if (carrierNode.MaxCapacity == 0)
            {
                reason = $"{carrier.Type} has 0 capacity";
                return false;
            }

            if (carrierNode.Carrying.Any(p => p.Type == passenger.Type))
            {
                reason = "Already carrying same piece type";
                return false;
            }

            if (IsAncestorOf(passenger, carrier))
            {
                reason = "Would create circular reference";
                return false;
            }

            int carrierGroupSize = CountGroupSize(carrier);
            int passengerGroupSize = CountGroupSize(passenger);
            int totalAfterCarry = carrierGroupSize + passengerGroupSize;

            if (totalAfterCarry > 3)
            {
                reason = $"Group size would exceed 3 ({totalAfterCarry})";
                return false;
            }

            return true;
        }

        private bool IsAncestorOf(BasePiece ancestor, BasePiece descendant)
        {
            if (!carryingNodes.TryGetValue(descendant, out _))
                return false;

            var current = descendant;
            while (carryingNodes[current].Carrier != null)
            {
                current = carryingNodes[current].Carrier;
                if (current == ancestor)
                    return true;
            }

            return false;
        }

        #endregion

        #region Private Execution - REDISTRIBUTION LOGIC

        /// <summary>
        /// Thực hiện carrying với redistribution tự động
        /// </summary>
        private bool ExecuteCarryWithRedistribution(BasePiece carrier, BasePiece passenger)
        {
            var carrierNode = carryingNodes[carrier];
            var passengerNode = carryingNodes[passenger];

            Debug.Log($"Before redistribution:");
            Debug.Log($"  Carrier {carrier.Type}: {carrierNode.CurrentLoad}/{carrierNode.MaxCapacity}");
            Debug.Log($"  Passenger {passenger.Type}: {passengerNode.CurrentLoad}/{passengerNode.MaxCapacity}");

            // Tách passenger khỏi carrier cũ (nếu có)
            if (passengerNode.Carrier != null && passengerNode.Carrier != carrier)
            {
                Detach(passenger);
            }

            // Thu thập tất cả pieces cần redistribute
            var allPieces = new List<BasePiece>();

            // Pieces từ carrier hiện tại
            allPieces.AddRange(carrierNode.Carrying.ToList());

            // Pieces từ passenger
            allPieces.AddRange(passengerNode.Carrying.ToList());

            // Tách tất cả relationships hiện tại
            foreach (var piece in allPieces.ToList())
            {
                Detach(piece);
            }

            // Thêm passenger vào carrier trước
            carrierNode.Carrying.Add(passenger);
            passengerNode.Carrier = carrier;

            Debug.Log($"Added {passenger.Type} to {carrier.Type} ({carrierNode.CurrentLoad}/{carrierNode.MaxCapacity})");

            // Redistribution theo thứ tự ưu tiên
            foreach (var piece in allPieces)
            {
                if (!RedistributePiece(piece, carrier, passenger))
                {
                    Debug.LogError($"Failed to redistribute {piece.Type}! Rolling back...");
                    RollbackCarrying(carrier, passenger, allPieces);
                    return false;
                }
            }

            Debug.Log("Redistribution complete:");
            return true;
        }

        /// <summary>
        /// Redistribute một piece theo thứ tự ưu tiên:
        /// 1. Passenger mang (nếu có capacity)
        /// 2. Carrier mang (nếu còn slot)
        /// 3. Sibling khác mang (có capacity)
        /// 4. Slot trống của bất kỳ ai trong group
        /// </summary>
        private bool RedistributePiece(BasePiece piece, BasePiece carrier, BasePiece passenger)
        {
            var carrierNode = carryingNodes[carrier];
            var passengerNode = carryingNodes[passenger];

            Debug.Log($"  Redistributing {piece.Type}:");

            // Priority 1: Passenger mang (nếu có capacity)
            if (CanPlaceInPiece(passenger, piece))
            {
                passengerNode.Carrying.Add(piece);
                carryingNodes[piece].Carrier = passenger;
                Debug.Log($"    → Placed in passenger {passenger.Type} ({passengerNode.CurrentLoad}/{passengerNode.MaxCapacity})");
                return true;
            }

            // Priority 2: Carrier mang (nếu còn slot)
            if (CanPlaceInPiece(carrier, piece))
            {
                carrierNode.Carrying.Add(piece);
                carryingNodes[piece].Carrier = carrier;
                Debug.Log($"    → Placed in carrier {carrier.Type} ({carrierNode.CurrentLoad}/{carrierNode.MaxCapacity})");
                return true;
            }

            // Priority 3: Siblings (các quân khác mà carrier đang mang, không phải passenger)
            foreach (var sibling in carrierNode.Carrying.ToList())
            {
                if (sibling == passenger) continue;

                if (CanPlaceInPiece(sibling, piece))
                {
                    var siblingNode = carryingNodes[sibling];
                    siblingNode.Carrying.Add(piece);
                    carryingNodes[piece].Carrier = sibling;
                    Debug.Log($"    → Placed in sibling {sibling.Type} ({siblingNode.CurrentLoad}/{siblingNode.MaxCapacity})");
                    return true;
                }
            }

            // Priority 4: Bất kỳ slot trống nào trong toàn bộ group
            var allInGroup = new List<BasePiece> { carrier };
            allInGroup.AddRange(GetAllCarriedPieces(carrier));

            foreach (var candidate in allInGroup)
            {
                if (candidate == piece) continue; // Không thể tự mang mình

                if (CanPlaceInPiece(candidate, piece))
                {
                    var candidateNode = carryingNodes[candidate];
                    candidateNode.Carrying.Add(piece);
                    carryingNodes[piece].Carrier = candidate;
                    Debug.Log($"    → Placed in {candidate.Type} ({candidateNode.CurrentLoad}/{candidateNode.MaxCapacity})");
                    return true;
                }
            }

            Debug.LogError($"    ✗ No valid placement found for {piece.Type}");
            return false;
        }

        /// <summary>
        /// Kiểm tra xem có thể đặt piece vào holder không
        /// </summary>
        private bool CanPlaceInPiece(BasePiece holder, BasePiece piece)
        {
            if (!carryingNodes.TryGetValue(holder, out var holderNode))
                return false;

            if (!holderNode.HasFreeSlot)
            {
                Debug.Log($"      {holder.Type} has no free slots ({holderNode.CurrentLoad}/{holderNode.MaxCapacity})");
                return false;
            }

            // Kiểm tra type compatibility
            if (!holder.AllowedCarryTypes.Contains(piece.Type))
            {
                Debug.Log($"      {holder.Type} cannot carry {piece.Type}");
                return false;
            }

            // Kiểm tra không duplicate type
            if (holderNode.Carrying.Any(p => p.Type == piece.Type))
            {
                Debug.Log($"      {holder.Type} already carrying {piece.Type}");
                return false;
            }

            // Kiểm tra không tạo circular reference
            if (IsAncestorOf(piece, holder))
            {
                Debug.Log($"      Would create circular reference");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Rollback khi redistribution thất bại
        /// </summary>
        private void RollbackCarrying(BasePiece carrier, BasePiece passenger, List<BasePiece> originalPieces)
        {
            Debug.LogWarning("Rolling back carrying operation...");

            // Tách passenger
            Detach(passenger);

            // Restore original relationships
            // Note: Trong thực tế, cần lưu snapshot của relationships trước khi redistribute
            // Để đơn giản, ta chỉ tách tất cả
            foreach (var piece in originalPieces)
            {
                Detach(piece);
            }
        }

        #endregion
    }
}