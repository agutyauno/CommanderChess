using System.Collections.Generic;
using CommanderChess.Board;
using CommanderChess.Core;
using CommanderChess.Pieces;

namespace CommanderChess.Managers
{
    /// <summary>
    /// Validates moves and enforces chess rules
    /// </summary>
    public class MoveValidator
    {
        private readonly ChessBoard _board;

        public MoveValidator(ChessBoard board)
        {
            _board = board;
        }

        /// <summary>
        /// Get all legal moves for a piece (considering check)
        /// </summary>
        public List<Move> GetLegalMoves(ChessPiece piece)
        {
            var legalMoves = new List<Move>();
            
            if (piece == null || piece.IsCaptured)
                return legalMoves;

            var possibleMoves = piece.GetPossibleMoves(_board.GetBoardState());

            foreach (var target in possibleMoves)
            {
                var move = CreateMove(piece, target);
                if (IsLegalMove(move))
                {
                    legalMoves.Add(move);
                }
            }

            // Add castling moves for king
            if (piece is King king)
            {
                AddCastlingMoves(king, legalMoves);
            }

            return legalMoves;
        }

        /// <summary>
        /// Check if a specific move is legal
        /// </summary>
        public bool IsLegalMove(Move move)
        {
            var piece = _board.GetPieceAt(move.From);
            if (piece == null)
                return false;

            // Cannot capture own piece
            var targetPiece = _board.GetPieceAt(move.To);
            if (targetPiece != null && targetPiece.Side == piece.Side)
                return false;

            // Simulate the move
            if (!SimulateMoveAndCheckKingSafety(move, piece.Side))
                return false;

            return true;
        }

        /// <summary>
        /// Simulate a move and check if king remains safe
        /// </summary>
        private bool SimulateMoveAndCheckKingSafety(Move move, PlayerSide side)
        {
            var boardState = _board.GetBoardState();
            var piece = boardState[move.From.Row, move.From.Column];
            var capturedPiece = boardState[move.To.Row, move.To.Column];

            // Simulate the move
            boardState[move.From.Row, move.From.Column] = null;
            boardState[move.To.Row, move.To.Column] = piece;

            // Handle en passant capture
            if (move.Type == MoveType.EnPassant && move.EnPassantCapturePosition.HasValue)
            {
                var epPos = move.EnPassantCapturePosition.Value;
                boardState[epPos.Row, epPos.Column] = null;
            }

            // Find king position
            var kingPos = FindKingPosition(side, boardState, piece, move.To);

            // Check if king is safe
            bool isSafe = !IsPositionUnderAttack(kingPos, side, boardState);

            // Restore the board state
            boardState[move.From.Row, move.From.Column] = piece;
            boardState[move.To.Row, move.To.Column] = capturedPiece;

            if (move.Type == MoveType.EnPassant && move.EnPassantCapturePosition.HasValue)
            {
                var epPos = move.EnPassantCapturePosition.Value;
                // Restore captured pawn (we need to track this separately)
            }

            return isSafe;
        }

        private BoardPosition FindKingPosition(PlayerSide side, ChessPiece[,] board, ChessPiece movedPiece, BoardPosition movedTo)
        {
            // If we moved the king, use its new position
            if (movedPiece.Type == PieceType.King)
            {
                return movedTo;
            }

            // Find king on board
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    var piece = board[row, col];
                    if (piece != null && piece.Type == PieceType.King && piece.Side == side)
                    {
                        return new BoardPosition(row, col);
                    }
                }
            }

            return BoardPosition.Invalid;
        }

        /// <summary>
        /// Check if a position is under attack by the opposing side
        /// </summary>
        public bool IsPositionUnderAttack(BoardPosition position, PlayerSide defendingSide, ChessPiece[,] board = null)
        {
            board ??= _board.GetBoardState();
            var attackingSide = defendingSide == PlayerSide.White ? PlayerSide.Black : PlayerSide.White;

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    var piece = board[row, col];
                    if (piece != null && piece.Side == attackingSide)
                    {
                        if (CanPieceAttackPosition(piece, position, board))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private bool CanPieceAttackPosition(ChessPiece attacker, BoardPosition target, ChessPiece[,] board)
        {
            // Special handling for pawn attacks (they attack diagonally, not forward)
            if (attacker is Pawn)
            {
                return CanPawnAttack(attacker as Pawn, target);
            }

            // For other pieces, check if target is in their possible moves
            var originalPos = attacker.Position;
            var moves = attacker.GetPossibleMoves(board);
            return moves.Contains(target);
        }

        private bool CanPawnAttack(Pawn pawn, BoardPosition target)
        {
            int direction = pawn.Side == PlayerSide.White ? 1 : -1;
            int attackRow = pawn.Position.Row + direction;

            return target.Row == attackRow && 
                   (target.Column == pawn.Position.Column - 1 || target.Column == pawn.Position.Column + 1);
        }

        /// <summary>
        /// Check if the king of the given side is in check
        /// </summary>
        public bool IsKingInCheck(PlayerSide side)
        {
            var king = _board.GetKing(side);
            if (king == null)
                return false;

            return IsPositionUnderAttack(king.Position, side);
        }

        /// <summary>
        /// Check if the given side has any legal moves
        /// </summary>
        public bool HasLegalMoves(PlayerSide side)
        {
            var pieces = _board.GetPieces(side);

            foreach (var piece in pieces)
            {
                if (!piece.IsCaptured)
                {
                    var moves = GetLegalMoves(piece);
                    if (moves.Count > 0)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check for checkmate
        /// </summary>
        public bool IsCheckmate(PlayerSide side)
        {
            return IsKingInCheck(side) && !HasLegalMoves(side);
        }

        /// <summary>
        /// Check for stalemate
        /// </summary>
        public bool IsStalemate(PlayerSide side)
        {
            return !IsKingInCheck(side) && !HasLegalMoves(side);
        }

        private Move CreateMove(ChessPiece piece, BoardPosition target)
        {
            var targetPiece = _board.GetPieceAt(target);
            MoveType type = MoveType.Normal;

            if (targetPiece != null)
            {
                type = MoveType.Capture;
            }

            // Check for en passant
            if (piece is Pawn pawn && pawn.EnPassantTarget == target)
            {
                var move = Move.CreateEnPassant(piece.Position, target, 
                    new BoardPosition(piece.Position.Row, target.Column));
                return move;
            }

            // Check for promotion
            if (piece is Pawn p && p.IsPromotionMove(target))
            {
                return Move.CreatePromotion(piece.Position, target, PieceType.Queen);
            }

            return new Move(piece.Position, target, type);
        }

        private void AddCastlingMoves(King king, List<Move> moves)
        {
            var board = _board.GetBoardState();

            // Cannot castle while in check
            if (IsKingInCheck(king.Side))
                return;

            // Kingside castling
            if (king.CanCastleKingside(board))
            {
                int row = king.Position.Row;
                
                // Check if squares king passes through are not under attack
                if (!IsPositionUnderAttack(new BoardPosition(row, 5), king.Side) &&
                    !IsPositionUnderAttack(new BoardPosition(row, 6), king.Side))
                {
                    moves.Add(Move.CreateCastling(king.Position, new BoardPosition(row, 6)));
                }
            }

            // Queenside castling
            if (king.CanCastleQueenside(board))
            {
                int row = king.Position.Row;

                // Check if squares king passes through are not under attack
                if (!IsPositionUnderAttack(new BoardPosition(row, 3), king.Side) &&
                    !IsPositionUnderAttack(new BoardPosition(row, 2), king.Side))
                {
                    moves.Add(Move.CreateCastling(king.Position, new BoardPosition(row, 2)));
                }
            }
        }
    }
}
