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

        public const string StartFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        public Board ChessBoard = new Board();

        public ChessGame()
        {
            ChessBoard.LoadFromFEN(StartFEN);
        }

        public bool CanMakeMove(int start, int end)
        {
            return true;
        }

        public void MakeMove()
        {

        }
    }
}
