using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
public class GameManager : MonoBehaviour
{
    [SerializeField] List<Piece> pieces = new();
    [SerializeField] LayerMask pieceLayer;
    [Inject] PieceController pieceController;
    [Inject] Board board;
    [SerializeField] Vector3 screenPosition;

    void Awake()
    {
        board.Init();
        SetUpPieces();
    }

    void Update()
    {
        screenPosition = Mouse.current.position.ReadValue();
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            var worldPoint = Camera.main.ScreenToWorldPoint(screenPosition);
            var hit = Physics2D.OverlapCircle(worldPoint, 0.1f, pieceLayer);
            if (hit) 
            {
                if (hit.TryGetComponent<Piece>(out var piece))
                {
                    Debug.Log($"Clicked on piece: {piece.Type} at {piece.Position.ToLabel()}");
                }
            }
        }
    }

    //xép cờ lên bàn
    void SetUpPieces()
    {
        foreach (var piece in pieces)
        {
            piece.Init();
            var pos = piece.Position;
            if (!pieceController.PlacePiece(piece, pos))
            {
                Debug.LogError($"Failed placing piece {piece.Type} at {pos.ToLabel()} (occupied or invalid)");
            }
        }
    }
}
