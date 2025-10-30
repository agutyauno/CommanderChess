using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// CarryingSystem - Hệ thống quản lý quan hệ mang/được mang giữa các quân cờ
/// Quy tắc chính:
/// - Tổng quân trong group (carrier + carrying + nested) < 3
/// - Tự động phân phối thông minh
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
        public List<BasePiece> Carrying { get; } = new(2); // Tối đa 2 quân

        public CarryingNode(BasePiece piece)
        {
            Piece = piece;
        }

        public bool HasFreeSlot => Carrying.Count < 2;
        public int FreeSlots => 2 - Carrying.Count;
        public bool IsCarried => Carrier != null;
    }

    #region Public API

    /// <summary>
    /// Đăng ký một quân cờ vào hệ thống
    /// </summary>
    public void RegisterPiece(BasePiece piece)
    {
        if (!carryingNodes.ContainsKey(piece))
        {
            carryingNodes[piece] = new CarryingNode(piece);
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
    /// TryAddCarry - Tự động xác định ai mang ai
    /// Đảm bảo group size < 3
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

        Debug.Log($"Attempting: {carrier.Type} carry {passenger.Type}");

        // Validate carrying
        if (!CanCarry(carrier, passenger, out string reason))
        {
            Debug.LogWarning($"Cannot carry: {reason}");
            return false;
        }

        // Thực hiện carrying
        return ExecuteCarry(carrier, passenger);
    }

    /// <summary>
    /// Tách một quân ra khỏi carrier
    /// Khi tách một quân, các quân nó đang mang cũng theo
    /// </summary>
    public bool Detach(BasePiece piece)
    {
        if (!carryingNodes.TryGetValue(piece, out var node))
            return false;

        if (node.Carrier == null)
            return false; // Không có carrier

        var carrier = node.Carrier;
        var carrierNode = carryingNodes[carrier];

        // Xóa piece khỏi carrier
        carrierNode.Carrying.Remove(piece);
        node.Carrier = null;

        Debug.Log($"Detached {piece.Type} from {carrier.Type}");

        // NOTE: Các quân mà piece đang mang vẫn giữ nguyên với piece

        return true;
    }

    /// <summary>
    /// Lấy tất cả quân mà piece đang mang (không bao gồm đệ quy)
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
    /// Lấy carrier gốc (top-level) của một quân
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
    /// Lấy carrier trực tiếp của piece
    /// </summary>
    public BasePiece GetCarrier(BasePiece piece)
    {
        return carryingNodes.TryGetValue(piece, out var node) ? node.Carrier : null;
    }

    public bool CanCarryDirectly(BasePiece carrier, BasePiece passenger)
    {
        return carrier.AllowedCarryTypes.Contains(passenger.Type);
    }

    #endregion

    #region Private Validation

    private bool CanCarry(BasePiece carrier, BasePiece passenger, out string reason)
    {
        reason = "";

        // Kiểm tra cả hai quân đều đã đăng ký
        if (!carryingNodes.ContainsKey(carrier) || !carryingNodes.ContainsKey(passenger))
        {
            reason = "Piece not registered";
            return false;
        }

        // Không thể tự mang chính mình
        if (carrier == passenger)
        {
            reason = "Cannot carry itself";
            return false;
        }

        // Kiểm tra cùng team
        if (carrier.Team != passenger.Team)
        {
            reason = "Different teams";
            return false;
        }

        // Kiểm tra carrier có được phép mang loại này không
        if (!carrier.AllowedCarryTypes.Contains(passenger.Type))
        {
            reason = $"{carrier.Type} cannot carry {passenger.Type}";
            return false;
        }

        var carrierNode = carryingNodes[carrier];
        
        // Kiểm tra không cho phép mang cùng loại quân
        if (carrierNode.Carrying.Any(p => p.Type == passenger.Type))
        {
            reason = "Already carrying same piece type";
            return false;
        }

        // Kiểm tra carrier không được là con của passenger (tránh vòng lặp)
        if (IsAncestorOf(passenger, carrier))
        {
            reason = "Would create circular reference";
            return false;
        }

        // LOGIC MỚI: Kiểm tra tổng group size
        // Group size = carrier's group + passenger's group
        int carrierGroupSize = CountGroupSize(carrier);
        int passengerGroupSize = CountGroupSize(passenger);
        int totalAfterCarry = carrierGroupSize + passengerGroupSize;

        Debug.Log($"  Group size check:");
        Debug.Log($"    Carrier ({carrier.Type}) group: {carrierGroupSize}");
        Debug.Log($"    Passenger ({passenger.Type}) group: {passengerGroupSize}");
        Debug.Log($"    Total after combine: {totalAfterCarry}");

        if (totalAfterCarry > 3)
        {
            reason = $"Group size would exceed limit: {carrierGroupSize} + {passengerGroupSize} = {totalAfterCarry} > 3";
            return false;
        }

        Debug.Log($"    Group size OK");
        return true;
    }

    /// <summary>
    /// Kiểm tra ancestor có phải là tổ tiên của descendant không
    /// </summary>
    private bool IsAncestorOf(BasePiece ancestor, BasePiece descendant)
    {
        if (!carryingNodes.TryGetValue(descendant, out var node))
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

    #region Private Execution

    private bool ExecuteCarry(BasePiece carrier, BasePiece passenger)
    {
        var passengerNode = carryingNodes[passenger];

        // Nếu passenger đã được mang bởi ai đó khác, tách ra trước
        if (passengerNode.Carrier != null && passengerNode.Carrier != carrier)
        {
            Detach(passenger);
        }

        bool placed = TryPlace(carrier, passenger);

        if (placed)
        {
            Debug.Log($"{carrier.Type} now carries {passenger.Type}");
            Debug.Log($"  Group structure:");
            PrintGroupStructure(carrier, "  ");
            return true;
        }

        return false;
    }

    /// <summary>
    /// Phân phối thông minh khi thêm passenger vào carrier
    /// </summary>
    private bool TryPlace(BasePiece carrier, BasePiece passenger)
    {
        var carrierNode = carryingNodes[carrier];
        var passengerNode = carryingNodes[passenger];

        // Case 1: Carrier còn slot trống -> Thêm trực tiếp vào carrier
        if (carrierNode.HasFreeSlot)
        {
            // Nếu passenger đang mang quân, thử redistribute
            if (passengerNode.Carrying.Count > 0)
            {
                return TryPlaceWithRedistribution(carrier, passenger);
            }
            else
            {
                // Passenger không mang gì, thêm trực tiếp
                carrierNode.Carrying.Add(passenger);
                passengerNode.Carrier = carrier;
                return true;
            }
        }

        // Case 2: Carrier đã đầy, thử cho một trong các quân đang mang carry passenger
        return TryPlaceInCarriedPieces(carrier, passenger);
    }

    /// <summary>
    /// Phân phối lại khi cần
    /// Logic: Khi thêm passenger, các quân carrier đang mang có thể bị chuyển sang passenger
    /// Ví dụ: Airforce mang Infantry, sau đó mang Tank (tank trống)
    /// Kết quả: Airforce carry [Tank], Tank carry [Infantry]
    /// </summary>
    private bool TryPlaceWithRedistribution(BasePiece carrier, BasePiece passenger)
    {
        var carrierNode = carryingNodes[carrier];
        var passengerNode = carryingNodes[passenger];

        // Lấy danh sách các quân carrier đang mang
        var carrierChildren = carrierNode.Carrying.ToList();
        
        // Lấy danh sách các quân passenger đang mang
        var passengerChildren = passengerNode.Carrying.ToList();

        // Tạo list tất cả pieces cần redistribute
        var piecesToRedistribute = new List<BasePiece>();
        piecesToRedistribute.AddRange(carrierChildren);
        piecesToRedistribute.AddRange(passengerChildren);

        // Tách tất cả relationships hiện tại
        foreach (var child in carrierChildren)
        {
            carrierNode.Carrying.Remove(child);
            carryingNodes[child].Carrier = null;
        }
        
        foreach (var child in passengerChildren)
        {
            passengerNode.Carrying.Remove(child);
            carryingNodes[child].Carrier = null;
        }

        // Thêm passenger vào carrier
        carrierNode.Carrying.Add(passenger);
        passengerNode.Carrier = carrier;

        Debug.Log($"    Redistributing {piecesToRedistribute.Count} pieces...");

        // Redistribute theo thứ tự ưu tiên:
        // 1. Thử cho passenger mang (nếu passenger có thể mang loại này)
        // 2. Thử cho carrier mang (nếu carrier còn slot)
        // 3. Thử cho các siblings khác
        foreach (var piece in piecesToRedistribute)
        {
            bool redistributed = false;

            // Priority 1: Passenger mang (nếu có thể)
            if (passenger.AllowedCarryTypes.Contains(piece.Type) && 
                passengerNode.HasFreeSlot &&
                !passengerNode.Carrying.Any(p => p.Type == piece.Type))
            {
                passengerNode.Carrying.Add(piece);
                carryingNodes[piece].Carrier = passenger;
                redistributed = true;
                Debug.Log($"      → {piece.Type} moved to {passenger.Type}");
            }
            // Priority 2: Carrier mang (nếu còn slot)
            else if (carrier.AllowedCarryTypes.Contains(piece.Type) && 
                     carrierNode.HasFreeSlot &&
                     !carrierNode.Carrying.Any(p => p.Type == piece.Type))
            {
                carrierNode.Carrying.Add(piece);
                carryingNodes[piece].Carrier = carrier;
                redistributed = true;
                Debug.Log($"      → {piece.Type} stays with {carrier.Type}");
            }
            // Priority 3: Siblings (các quân khác carrier đang mang)
            else
            {
                foreach (var sibling in carrierNode.Carrying.ToList())
                {
                    if (sibling == passenger) continue;

                    var siblingNode = carryingNodes[sibling];
                    if (sibling.AllowedCarryTypes.Contains(piece.Type) &&
                        siblingNode.HasFreeSlot &&
                        !siblingNode.Carrying.Any(p => p.Type == piece.Type))
                    {
                        siblingNode.Carrying.Add(piece);
                        carryingNodes[piece].Carrier = sibling;
                        redistributed = true;
                        Debug.Log($"      → {piece.Type} moved to {sibling.Type}");
                        break;
                    }
                }
            }

            if (!redistributed)
            {
                Debug.LogError($"      Failed to redistribute {piece.Type}! Rolling back...");
                
                // Rollback toàn bộ
                carrierNode.Carrying.Remove(passenger);
                passengerNode.Carrier = null;
                
                // Restore carrier's children
                foreach (var child in carrierChildren)
                {
                    carrierNode.Carrying.Add(child);
                    carryingNodes[child].Carrier = carrier;
                }
                
                // Restore passenger's children
                foreach (var child in passengerChildren)
                {
                    passengerNode.Carrying.Add(child);
                    carryingNodes[child].Carrier = passenger;
                }
                
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Thử đặt passenger vào một trong các quân carrier đang mang
    /// </summary>
    private bool TryPlaceInCarriedPieces(BasePiece carrier, BasePiece passenger)
    {
        var carrierNode = carryingNodes[carrier];

        foreach (var carried in carrierNode.Carrying)
        {
            var carriedNode = carryingNodes[carried];

            // Kiểm tra carried có thể mang passenger không
            if (!carried.AllowedCarryTypes.Contains(passenger.Type))
                continue;

            // Kiểm tra không duplicate type
            if (carriedNode.Carrying.Any(p => p.Type == passenger.Type))
                continue;

            // Kiểm tra có slot trống
            if (!carriedNode.HasFreeSlot)
                continue;

            // Thực hiện
            carriedNode.Carrying.Add(passenger);
            carryingNodes[passenger].Carrier = carried;
            
            Debug.Log($"    Placed {passenger.Type} in {carried.Type} (nested)");
            return true;
        }

        return false;
    }

    #endregion

    #region Debug & Visualization

    public void PrintGroupStructure(BasePiece root, string indent = "")
    {
        if (!carryingNodes.TryGetValue(root, out var node))
            return;

        Debug.Log($"{indent}{root.Type}");
        
        foreach (var child in node.Carrying)
        {
            PrintGroupStructure(child, indent + "  ├── ");
        }
    }

    public string GetGroupTreeString(BasePiece root)
    {
        var sb = new System.Text.StringBuilder();
        BuildTreeString(root, sb, "", true);
        return sb.ToString();
    }

    private void BuildTreeString(BasePiece piece, System.Text.StringBuilder sb, string indent, bool isLast)
    {
        if (!carryingNodes.TryGetValue(piece, out var node))
            return;

        sb.Append(indent);
        if (isLast)
        {
            sb.Append("└── ");
            indent += "    ";
        }
        else
        {
            sb.Append("├── ");
            indent += "│   ";
        }

        sb.AppendLine($"{piece.Type} (carrying: {node.Carrying.Count}/2)");

        for (int i = 0; i < node.Carrying.Count; i++)
        {
            bool lastChild = i == node.Carrying.Count - 1;
            BuildTreeString(node.Carrying[i], sb, indent, lastChild);
        }
    }

    #endregion
}