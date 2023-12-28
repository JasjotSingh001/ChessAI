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

        private bool CanWhiteCastleKingside = true;
        private bool CanWhiteCastleQueenside = true;
        private bool CanBlackCastleKingside = true;
        private bool CanBlackCastleQueenside = true;
        private bool WhiteToMove = true;
        private byte MovesWithoutCaptureOrPawnMove = 0;

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
            int file = 0, rank = 7;

            for (int i = 0; i < fenBoard.Length; i++)
            {
                char symbol = fenBoard[i];
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
        }

        public void MakeMove()
        {

        }

        public void UnmakeMove()
        {

        }

        public int[] GetBoard()
        {
            return board;
        }
    }
}
