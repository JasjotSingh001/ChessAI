using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessWPF.Game
{
    public class Board
    {
        private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private int[] board = new int[64];

        private bool canCastleKingside = true;
        private bool canCastleQueenside = true;

        private byte movesWithoutCaptureOrPawnMove = 0;

        //some way to figure out if repetition or 50 move rule
        public Board()
        {

        }

        public void LoadFromFEN(String fen)
        {
            var pieceTypeFromSymbol = new Dictionary<char, int>()
            {
                ['k'] = Piece.King, 
                ['p'] = Piece.Pawn,
                ['n'] = Piece.Knight,
                ['b'] = Piece.Bishop,
                ['r'] = Piece.Rook,
                ['q'] = Piece.Queen
            };

            String fenBoard = fen.Split(' ')[0];
            logger.Info(fenBoard);
            int file = 0, rank = 7;

            for (int i = 0; i < fenBoard.Length; i++)
            {
                char symbol = fenBoard[i];
                logger.Info(symbol);
                if (symbol == '/')
                {
                    file = 0;
                    rank--;
                } else
                {
                    if (char.IsDigit(symbol))
                    {
                        file += (int) char.GetNumericValue(symbol);
                    } else
                    {
                        int pieceColour = (char.IsUpper(symbol)) ? Piece.White : Piece.Black;
                        int pieceType = pieceTypeFromSymbol[char.ToLower(symbol)];
                        board[(rank * 8) + file] = pieceColour | pieceType;
                        file++;
                    }
                }
            }

            for (int i = 0; i < board.Length; i++)
            {
                logger.Info(i + ": " + board[i]);
            }
        }

        public int[] GetBoard()
        {
            return board;
        }
    }
}
