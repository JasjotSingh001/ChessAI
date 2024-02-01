using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessWPF.Game
{
    public static class BoardRepresentation
    {
        public static int RankIndex(int squareIndex)
        {
            return squareIndex >> 3;
        }

        public static int FileIndex(int squareIndex)
        {
            //return squareIndex & 0b000111;
            return squareIndex % 8;
        }

        public static int IndexFromCoord(int fileIndex, int rankIndex)
        {
            return rankIndex * 8 + fileIndex;
        }
    }
}
