using UnityEngine;
using UnityEngine.InputSystem;
public class InputHandler : MonoBehaviour
{
    [SerializeField] LayerMask pieceLayer;
    Vector2 screenPosition;
    Mouse mouse = Mouse.current;
    
    void Update()
    {
        screenPosition = mouse.position.ReadValue();
        if (mouse.leftButton.wasPressedThisFrame)
        {
            var worldPoint = Camera.main.ScreenToWorldPoint(screenPosition);
            var hit = Physics2D.OverlapCircle(worldPoint, 0.1f, pieceLayer);
            if (hit) 
            {
                if (hit.TryGetComponent<Piece>(out var piece))
                {
                    Debug.Log($"Clicked on piece: {piece.Type} at {piece.Position.ToLabel()}");
                   //todo: xử lý chọn piece
                }
            }
        }
    }
}
