using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessWPF.Game
{
    public class Board
    {
        private byte[] board = new byte[64];

        private bool canCastleKingside = true;
        private bool canCastleQueenside = true;

        private byte movesWithoutCaptureOrPawnMove = 0;

        //some way to figure out if repetition or 50 move rule
        public Board()
        {

        }

        public void LoadFromFEN(String fenString)
        {
            for (int i = 0; i < 7; i++)
            {
                while (true)
                {

                }
            }
        }
    }
}
