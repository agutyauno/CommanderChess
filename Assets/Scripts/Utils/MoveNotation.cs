using System.Collections.Generic;
using CommanderChess.Core;
using CommanderChess.Pieces;

namespace CommanderChess.Utils
{
    /// <summary>
    /// Utility methods for move notation and parsing
    /// </summary>
    public static class MoveNotation
    {
        /// <summary>
        /// Convert a move to Standard Algebraic Notation (SAN)
        /// </summary>
        public static string ToSAN(Move move, ChessPiece piece, ChessPiece[,] board, bool isCapture, bool isCheck, bool isCheckmate)
        {
            // Handle castling
            if (move.Type == MoveType.Castling)
            {
                return move.To.Column > move.From.Column ? "O-O" : "O-O-O";
            }

            var notation = new System.Text.StringBuilder();

            // Piece symbol (not for pawns)
            if (piece.Type != PieceType.Pawn)
            {
                notation.Append(GetPieceSymbol(piece.Type));
            }

            // Disambiguation if needed
            string disambiguation = GetDisambiguation(move, piece, board);
            notation.Append(disambiguation);

            // Capture symbol
            if (isCapture || move.Type == MoveType.EnPassant)
            {
                if (piece.Type == PieceType.Pawn)
                {
                    notation.Append((char)('a' + move.From.Column));
                }
                notation.Append('x');
            }

            // Destination square
            notation.Append(move.To.ToNotation());

            // Promotion
            if (move.Type == MoveType.Promotion)
            {
                notation.Append('=');
                notation.Append(GetPieceSymbol(move.PromotionPiece));
            }

            // En passant indicator
            if (move.Type == MoveType.EnPassant)
            {
                notation.Append(" e.p.");
            }

            // Check/Checkmate
            if (isCheckmate)
            {
                notation.Append('#');
            }
            else if (isCheck)
            {
                notation.Append('+');
            }

            return notation.ToString();
        }

        /// <summary>
        /// Convert a move to Long Algebraic Notation (LAN)
        /// </summary>
        public static string ToLAN(Move move, ChessPiece piece, bool isCapture)
        {
            // Handle castling
            if (move.Type == MoveType.Castling)
            {
                return move.To.Column > move.From.Column ? "O-O" : "O-O-O";
            }

            var notation = new System.Text.StringBuilder();

            // Piece symbol (not for pawns)
            if (piece.Type != PieceType.Pawn)
            {
                notation.Append(GetPieceSymbol(piece.Type));
            }

            // From square
            notation.Append(move.From.ToNotation());

            // Capture or move symbol
            notation.Append(isCapture ? 'x' : '-');

            // To square
            notation.Append(move.To.ToNotation());

            // Promotion
            if (move.Type == MoveType.Promotion)
            {
                notation.Append('=');
                notation.Append(GetPieceSymbol(move.PromotionPiece));
            }

            return notation.ToString();
        }

        /// <summary>
        /// Convert a move to UCI notation (used by chess engines)
        /// </summary>
        public static string ToUCI(Move move)
        {
            string uci = move.From.ToNotation() + move.To.ToNotation();

            if (move.Type == MoveType.Promotion)
            {
                uci += char.ToLower(GetPieceSymbol(move.PromotionPiece));
            }

            return uci;
        }

        /// <summary>
        /// Parse UCI notation to a Move object
        /// </summary>
        public static Move FromUCI(string uci)
        {
            if (string.IsNullOrEmpty(uci) || uci.Length < 4)
                return null;

            var from = BoardPosition.FromNotation(uci.Substring(0, 2));
            var to = BoardPosition.FromNotation(uci.Substring(2, 2));

            if (!from.IsValid || !to.IsValid)
                return null;

            var move = new Move(from, to);

            // Check for promotion
            if (uci.Length >= 5)
            {
                char promoChar = char.ToLower(uci[4]);
                PieceType promoType = promoChar switch
                {
                    'q' => PieceType.Queen,
                    'r' => PieceType.Rook,
                    'b' => PieceType.Bishop,
                    'n' => PieceType.Knight,
                    _ => PieceType.Queen
                };

                move = Move.CreatePromotion(from, to, promoType);
            }

            return move;
        }

        private static char GetPieceSymbol(PieceType type)
        {
            return type switch
            {
                PieceType.King => 'K',
                PieceType.Queen => 'Q',
                PieceType.Rook => 'R',
                PieceType.Bishop => 'B',
                PieceType.Knight => 'N',
                PieceType.Commander => 'C',
                _ => ' '
            };
        }

        private static string GetDisambiguation(Move move, ChessPiece piece, ChessPiece[,] board)
        {
            // Find if there are other pieces of the same type that can move to the same square
            List<ChessPiece> ambiguousPieces = new List<ChessPiece>();

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    var other = board[row, col];
                    if (other != null && other != piece && 
                        other.Type == piece.Type && other.Side == piece.Side)
                    {
                        var moves = other.GetPossibleMoves(board);
                        if (moves.Contains(move.To))
                        {
                            ambiguousPieces.Add(other);
                        }
                    }
                }
            }

            if (ambiguousPieces.Count == 0)
                return "";

            // Check if file is unique
            bool fileUnique = true;
            bool rankUnique = true;

            foreach (var other in ambiguousPieces)
            {
                if (other.Position.Column == piece.Position.Column)
                    fileUnique = false;
                if (other.Position.Row == piece.Position.Row)
                    rankUnique = false;
            }

            if (fileUnique)
                return ((char)('a' + piece.Position.Column)).ToString();
            if (rankUnique)
                return ((char)('1' + piece.Position.Row)).ToString();

            return piece.Position.ToNotation();
        }
    }
}
