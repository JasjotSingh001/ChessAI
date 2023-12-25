using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessWPF.Game
{
    public class ChessGame
    {
        private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public Board ChessBoard = new Board();

        public ChessGame()
        {
            ChessBoard.LoadFromFEN("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
        }
    }
}
