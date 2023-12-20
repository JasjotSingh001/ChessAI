using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessWPF.Game
{
    public class Game
    {
        private Board chessBoard = new Board();

        public Game()
        {
            chessBoard.LoadFromFEN("");
        }
    }
}
