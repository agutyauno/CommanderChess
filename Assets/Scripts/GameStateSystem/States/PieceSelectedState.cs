using UnityEngine;

/// <summary>
/// PieceSelectedState - Trạng thái sau khi chọn quân
/// Hiển thị valid moves/attacks và xử lý action
/// </summary>
public class PieceSelectedState : IGameState
{
    public GameStateData Data { get; }
    public GameStateManager Manager { get; }

    public PieceSelectedState(GameStateData data, GameStateManager manager)
    {
        Data = data;
        Manager = manager;
    }

    public void Enter()
    {
        Debug.Log($"Entered PieceSelectedState with {Data.SelectedPiece.Type}");

        if (Data.SelectedPiece == null)
        {
            Debug.LogError("PieceSelectedState entered with null SelectedPiece!");
            Manager.ChangeState(GameState.Idle);
            return;
        }

        // Highlight valid moves and attacks
        var piece = Data.SelectedPiece;
        piece.RecalculateCache();

        if (piece == null)
        {
            Debug.LogError("SelectedPiece is null in PieceSelectedState.Enter()");
        }
        else
        {
            Debug.Log($"Selected piece: {piece.Type} at {piece.Position.ToLabel()}");
        }

        if (piece.PossibleMoves == null && piece.PossibleAttacks == null)
        {
            Debug.LogError("PossibleMoves or PossibleAttacks is null in PieceSelectedState.Enter()");
        }
        else if(piece.PossibleMoves.Count == 0 && piece.PossibleAttacks.Count == 0)
        {
            Debug.LogWarning("No possible moves or attacks for selected piece.");
        }
        else
        {
            Debug.Log($"Possible moves count: {piece.PossibleMoves.Count}, Possible attacks count: {piece.PossibleAttacks.Count}");
        }
        
        Data.HighlightedMoves.Clear();
        Data.HighlightedMoves.AddRange(piece.PossibleMoves);
        
        Data.HighlightedAttacks.Clear();
        Data.HighlightedAttacks.AddRange(piece.PossibleAttacks);

        // Send to visual system
        Manager.HighlightMoves(Data.HighlightedMoves);
        Manager.HighlightAttacks(Data.HighlightedAttacks);
        Manager.HighlightSelected(Data.SelectedPosition);

        // Emit event
        Manager.NotifyPieceSelected(piece);
    }

    public void Exit()
    {
        Debug.Log("Exited PieceSelectedState");
        
        // Clear highlights
        Manager.ClearHighlights();
    }

    public void HandleBoardClick(BoardCoord coord)
    {
        // Clicked on empty position - check if it's valid move or cancel selection
        
        var piece = Data.SelectedPiece;
        if (piece == null)
        {
            Manager.ChangeState(GameState.Idle);
            return;
        }

        // Kiểm tra xem có phải valid move không
        if (piece.PossibleMoves.Contains(coord))
        {
            // Execute move
            TryExecuteMove(coord);
            return;
        }

        // Nếu không phải valid move, deselect
        Debug.Log($"Clicked {coord.ToLabel()} - not a valid move, deselecting");
        Manager.ChangeState(GameState.Idle);
    }

    public void HandlePieceClick(BasePiece clickedPiece)
    {
        var selectedPiece = Data.SelectedPiece;
        if (selectedPiece == null)
        {
            Manager.ChangeState(GameState.Idle);
            return;
        }

        // Case 1: Clicked on same piece -> deselect
        if (clickedPiece == selectedPiece)
        {
            Debug.Log($"Clicked same piece - deselecting");
            Manager.ChangeState(GameState.Idle);
            return;
        }

        // Case 2: Clicked on ally piece
        if (clickedPiece.Team == selectedPiece.Team)
        {
            // Case 2a: Valid boarding target
            if (selectedPiece.PossibleMoves.Contains(clickedPiece.Position))
            {
                TryExecuteBoarding(clickedPiece.Position);
                return;
            }

            // Case 2b: Change selection to new piece (if not carried)
            if (!Manager.CarryingSystem.IsCarried(clickedPiece) && 
                Manager.TurnManager.IsCurrentPlayerPiece(clickedPiece))
            {
                TryChangePieceSelection(clickedPiece);
                return;
            }

            // Case 2c: Invalid ally click
            Debug.LogWarning($"Clicked ally {clickedPiece.Type} but not valid for boarding or selection");
            return;
        }

        // Case 3: Clicked on enemy piece
        if (clickedPiece.Team != selectedPiece.Team)
        {
            // Check if valid attack
            if (selectedPiece.PossibleAttacks.Contains(clickedPiece.Position))
            {
                TryExecuteCapture(clickedPiece.Position);
                return;
            }

            Debug.LogWarning($"Clicked enemy {clickedPiece.Type} but not in attack range");
            return;
        }
    }

    public void HandleCancel()
    {
        Debug.Log("Cancel in PieceSelectedState - deselecting piece");
        Manager.ChangeState(GameState.Idle);
    }

    public void Update()
    {
        // PieceSelectedState không cần update logic
    }

    #region Private Helper Methods

    private void TryChangePieceSelection(BasePiece newPiece)
    {
        Debug.Log($"Changing selection to {newPiece.Type} at {newPiece.Position.ToLabel()}");
        
        Data.SelectedPiece = newPiece;
        Data.SelectedPosition = newPiece.Position;
        
        // Re-enter state để update highlights
        Exit();
        Enter();
    }

    private void TryExecuteMove(BoardCoord to)
    {
        var piece = Data.SelectedPiece;
        var from = piece.Position;

        Debug.Log($"Attempting move: {piece.Type} from {from.ToLabel()} to {to.ToLabel()}");

        // Validate
        var validation = Manager.ActionValidator.ValidateMove(piece, to);
        if (!validation.IsValid)
        {
            Debug.LogWarning($"Move validation failed: {validation.Reason}");
            return;
        }

        // Create and execute command
        var command = Manager.CommandManager.CreateMoveCommand(from, to);
        if (Manager.CommandManager.Execute(command))
        {
            Debug.Log($"Move executed successfully");
            Manager.ChangeState(GameState.ExecutingAction);
        }
        else
        {
            Debug.LogError($"Move execution failed");
            Manager.ChangeState(GameState.Idle);
        }
    }

    private void TryExecuteCapture(BoardCoord to)
    {
        var piece = Data.SelectedPiece;
        var from = piece.Position;

        Debug.Log($"Attempting capture: {piece.Type} from {from.ToLabel()} to {to.ToLabel()}");

        // Validate
        var validation = Manager.ActionValidator.ValidateCapture(piece, to);
        if (!validation.IsValid)
        {
            Debug.LogWarning($"Capture validation failed: {validation.Reason}");
            return;
        }

        // Create and execute command
        var command = Manager.CommandManager.CreateCaptureCommand(from, to);
        if (Manager.CommandManager.Execute(command))
        {
            Debug.Log($"Capture executed successfully");
            Manager.ChangeState(GameState.ExecutingAction);
        }
        else
        {
            Debug.LogError($"Capture execution failed");
            Manager.ChangeState(GameState.Idle);
        }
    }

    private void TryExecuteBoarding(BoardCoord to)
    {
        var piece = Data.SelectedPiece;
        var from = piece.Position;

        Debug.Log($"Attempting boarding: {piece.Type} from {from.ToLabel()} to {to.ToLabel()}");

        // Validate
        var validation = Manager.ActionValidator.ValidateBoarding(piece, to);
        if (!validation.IsValid)
        {
            Debug.LogWarning($"Boarding validation failed: {validation.Reason}");
            return;
        }

        // Create and execute command
        var command = Manager.CommandManager.CreateBoardingCommand(from, to);
        if (Manager.CommandManager.Execute(command))
        {
            Debug.Log($"Boarding executed successfully");
            Manager.ChangeState(GameState.ExecutingAction);
        }
        else
        {
            Debug.LogError($"Boarding execution failed");
            Manager.ChangeState(GameState.Idle);
        }
    }

    #endregion
}