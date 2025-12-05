using System;
using System.Collections.Generic;
using CommanderChess.Core;
using CommanderChess.Pieces;
using UnityEngine;

namespace CommanderChess.Board
{
    /// <summary>
    /// Manages the chess board state and piece positions
    /// </summary>
    public class ChessBoard
    {
        private ChessPiece[,] _board;
        private List<ChessPiece> _whitePieces;
        private List<ChessPiece> _blackPieces;
        private List<Move> _moveHistory;
        
        public King WhiteKing { get; private set; }
        public King BlackKing { get; private set; }
        public Commander WhiteCommander { get; private set; }
        public Commander BlackCommander { get; private set; }
        public BoardPosition? EnPassantTarget { get; private set; }

        public ChessBoard()
        {
            _board = new ChessPiece[8, 8];
            _whitePieces = new List<ChessPiece>();
            _blackPieces = new List<ChessPiece>();
            _moveHistory = new List<Move>();
        }

        /// <summary>
        /// Initialize board with standard Commander Chess setup
        /// </summary>
        public void SetupStandardGame()
        {
            Clear();

            // White pieces (row 0 and 1)
            SetupPiecesForSide(PlayerSide.White);

            // Black pieces (row 6 and 7)
            SetupPiecesForSide(PlayerSide.Black);
        }

        private void SetupPiecesForSide(PlayerSide side)
        {
            int backRow = side == PlayerSide.White ? 0 : 7;
            int pawnRow = side == PlayerSide.White ? 1 : 6;

            // Rooks
            AddPiece(new Rook(side, new BoardPosition(backRow, 0)));
            AddPiece(new Rook(side, new BoardPosition(backRow, 7)));

            // Knights
            AddPiece(new Knight(side, new BoardPosition(backRow, 1)));
            AddPiece(new Knight(side, new BoardPosition(backRow, 6)));

            // Bishops
            AddPiece(new Bishop(side, new BoardPosition(backRow, 2)));
            AddPiece(new Bishop(side, new BoardPosition(backRow, 5)));

            // Queen
            AddPiece(new Queen(side, new BoardPosition(backRow, 3)));

            // King
            var king = new King(side, new BoardPosition(backRow, 4));
            AddPiece(king);
            if (side == PlayerSide.White)
                WhiteKing = king;
            else
                BlackKing = king;

            // Pawns
            for (int col = 0; col < 8; col++)
            {
                AddPiece(new Pawn(side, new BoardPosition(pawnRow, col)));
            }
        }

        /// <summary>
        /// Setup game with Commanders (variant mode)
        /// </summary>
        public void SetupCommanderGame()
        {
            Clear();

            // Setup standard pieces first
            SetupPiecesForSideWithCommander(PlayerSide.White);
            SetupPiecesForSideWithCommander(PlayerSide.Black);
        }

        private void SetupPiecesForSideWithCommander(PlayerSide side)
        {
            int backRow = side == PlayerSide.White ? 0 : 7;
            int pawnRow = side == PlayerSide.White ? 1 : 6;

            // Rooks
            AddPiece(new Rook(side, new BoardPosition(backRow, 0)));
            AddPiece(new Rook(side, new BoardPosition(backRow, 7)));

            // Knights (only one knight, commander replaces the other)
            AddPiece(new Knight(side, new BoardPosition(backRow, 1)));

            // Commander (replaces one knight)
            var commander = new Commander(side, new BoardPosition(backRow, 6));
            AddPiece(commander);
            if (side == PlayerSide.White)
                WhiteCommander = commander;
            else
                BlackCommander = commander;

            // Bishops
            AddPiece(new Bishop(side, new BoardPosition(backRow, 2)));
            AddPiece(new Bishop(side, new BoardPosition(backRow, 5)));

            // Queen
            AddPiece(new Queen(side, new BoardPosition(backRow, 3)));

            // King
            var king = new King(side, new BoardPosition(backRow, 4));
            AddPiece(king);
            if (side == PlayerSide.White)
                WhiteKing = king;
            else
                BlackKing = king;

            // Pawns
            for (int col = 0; col < 8; col++)
            {
                AddPiece(new Pawn(side, new BoardPosition(pawnRow, col)));
            }
        }

        public void Clear()
        {
            _board = new ChessPiece[8, 8];
            _whitePieces.Clear();
            _blackPieces.Clear();
            _moveHistory.Clear();
            WhiteKing = null;
            BlackKing = null;
            WhiteCommander = null;
            BlackCommander = null;
            EnPassantTarget = null;
        }

        public void AddPiece(ChessPiece piece)
        {
            if (!piece.Position.IsValid)
                return;

            _board[piece.Position.Row, piece.Position.Column] = piece;

            if (piece.Side == PlayerSide.White)
                _whitePieces.Add(piece);
            else
                _blackPieces.Add(piece);
        }

        public void RemovePiece(BoardPosition position)
        {
            if (!position.IsValid)
                return;

            var piece = _board[position.Row, position.Column];
            if (piece != null)
            {
                piece.IsCaptured = true;
                if (piece.Side == PlayerSide.White)
                    _whitePieces.Remove(piece);
                else
                    _blackPieces.Remove(piece);
            }

            _board[position.Row, position.Column] = null;
        }

        public ChessPiece GetPieceAt(BoardPosition position)
        {
            if (!position.IsValid)
                return null;

            return _board[position.Row, position.Column];
        }

        public ChessPiece[,] GetBoardState()
        {
            return _board;
        }

        public List<ChessPiece> GetPieces(PlayerSide side)
        {
            return side == PlayerSide.White ? _whitePieces : _blackPieces;
        }

        public King GetKing(PlayerSide side)
        {
            return side == PlayerSide.White ? WhiteKing : BlackKing;
        }

        public Commander GetCommander(PlayerSide side)
        {
            return side == PlayerSide.White ? WhiteCommander : BlackCommander;
        }

        /// <summary>
        /// Execute a move on the board
        /// </summary>
        public void ExecuteMove(Move move)
        {
            var piece = GetPieceAt(move.From);
            if (piece == null)
                return;

            // Store move info for undo
            move.WasFirstMove = !piece.HasMoved;
            var capturedPiece = GetPieceAt(move.To);
            if (capturedPiece != null)
            {
                move.CapturedPiece = capturedPiece.Type;
                move.CapturedPieceSide = capturedPiece.Side;
            }

            // Handle special moves
            switch (move.Type)
            {
                case MoveType.EnPassant:
                    HandleEnPassant(move);
                    break;
                case MoveType.Castling:
                    HandleCastling(move);
                    break;
                case MoveType.Promotion:
                    HandlePromotion(move, piece);
                    break;
                default:
                    HandleNormalMove(move, piece);
                    break;
            }

            // Update en passant target
            UpdateEnPassantTarget(move, piece);

            piece.HasMoved = true;
            _moveHistory.Add(move);
        }

        private void HandleNormalMove(Move move, ChessPiece piece)
        {
            // Capture if there's an enemy piece
            if (GetPieceAt(move.To) != null)
            {
                RemovePiece(move.To);
            }

            // Move the piece
            _board[piece.Position.Row, piece.Position.Column] = null;
            piece.Position = move.To;
            _board[move.To.Row, move.To.Column] = piece;
        }

        private void HandleEnPassant(Move move)
        {
            var piece = GetPieceAt(move.From);
            var capturePos = move.EnPassantCapturePosition ?? 
                new BoardPosition(move.From.Row, move.To.Column);

            RemovePiece(capturePos);

            _board[piece.Position.Row, piece.Position.Column] = null;
            piece.Position = move.To;
            _board[move.To.Row, move.To.Column] = piece;
        }

        private void HandleCastling(Move move)
        {
            var king = GetPieceAt(move.From);
            
            // Determine if kingside or queenside
            bool isKingside = move.To.Column > move.From.Column;
            int rookFromCol = isKingside ? 7 : 0;
            int rookToCol = isKingside ? 5 : 3;

            // Move king
            _board[king.Position.Row, king.Position.Column] = null;
            king.Position = move.To;
            _board[move.To.Row, move.To.Column] = king;

            // Move rook
            var rook = _board[move.From.Row, rookFromCol];
            _board[move.From.Row, rookFromCol] = null;
            rook.Position = new BoardPosition(move.From.Row, rookToCol);
            _board[move.From.Row, rookToCol] = rook;
            rook.HasMoved = true;
        }

        private void HandlePromotion(Move move, ChessPiece pawn)
        {
            // Capture if there's an enemy piece
            if (GetPieceAt(move.To) != null)
            {
                RemovePiece(move.To);
            }

            // Remove pawn
            RemovePiece(move.From);

            // Add promoted piece
            var promotedPiece = ChessPiece.Create(
                move.PromotionPiece, 
                pawn.Side, 
                move.To
            );
            promotedPiece.HasMoved = true;
            AddPiece(promotedPiece);
        }

        private void UpdateEnPassantTarget(Move move, ChessPiece piece)
        {
            EnPassantTarget = null;

            // Check if pawn moved two squares
            if (piece.Type == PieceType.Pawn)
            {
                int rowDiff = Math.Abs(move.To.Row - move.From.Row);
                if (rowDiff == 2)
                {
                    // Set en passant target to the square the pawn passed over
                    int midRow = (move.From.Row + move.To.Row) / 2;
                    EnPassantTarget = new BoardPosition(midRow, move.From.Column);
                }
            }

            // Update pawns' en passant awareness
            UpdatePawnEnPassant();
        }

        private void UpdatePawnEnPassant()
        {
            foreach (var piece in _whitePieces)
            {
                if (piece is Pawn pawn)
                {
                    pawn.EnPassantTarget = EnPassantTarget;
                }
            }

            foreach (var piece in _blackPieces)
            {
                if (piece is Pawn pawn)
                {
                    pawn.EnPassantTarget = EnPassantTarget;
                }
            }
        }

        /// <summary>
        /// Undo the last move
        /// </summary>
        public void UndoLastMove()
        {
            if (_moveHistory.Count == 0)
                return;

            var move = _moveHistory[^1];
            _moveHistory.RemoveAt(_moveHistory.Count - 1);

            // Implementation would depend on storing full state or reverse operations
            // For now, we store captured piece info in the Move class
        }

        public List<Move> GetMoveHistory()
        {
            return new List<Move>(_moveHistory);
        }

        /// <summary>
        /// Get board state as FEN-like string (simplified)
        /// </summary>
        public string GetBoardString()
        {
            var sb = new System.Text.StringBuilder();

            for (int row = 7; row >= 0; row--)
            {
                int emptyCount = 0;
                for (int col = 0; col < 8; col++)
                {
                    var piece = _board[row, col];
                    if (piece == null)
                    {
                        emptyCount++;
                    }
                    else
                    {
                        if (emptyCount > 0)
                        {
                            sb.Append(emptyCount);
                            emptyCount = 0;
                        }
                        sb.Append(piece.GetSymbol());
                    }
                }
                if (emptyCount > 0)
                {
                    sb.Append(emptyCount);
                }
                if (row > 0)
                {
                    sb.Append('/');
                }
            }

            return sb.ToString();
        }
    }
}
