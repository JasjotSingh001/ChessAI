using ChessWPF.Game;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessWPF.AI
{
    public class AIGameHandler
    {
        private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private ChessGame chessGame = new ChessGame();
        //private AIPlayer aiPlayer;

        private bool whiteToMove;
        private int aiColour = Board.BlackIndex;
        private int playerColour = Board.WhiteIndex;

        public AIGameHandler() 
        {
            whiteToMove = chessGame.WhiteToMove;
        }

        public Move ChooseMove()
        { 
            Search search = new Search(chessGame.ChessBoard);
            Move move = search.StartSearch();

            if (move.moveValue == 0b0000000000000000)
            {
                logger.Info("Invalid move");
                move = ChooseRandomMove();
            } else
            {
                MakeAIMove(move);
            }

            return move;
        }

        public Move ChooseRandomMove()
        {
            Random random = new Random();
            Move move = chessGame.Moves[random.Next(0, chessGame.Moves.Count)];
            chessGame.MakeMove(move);

            return move;
        }

        public bool CanPlayerMakeMove(int startIndex, int endIndex)
        {
            return chessGame.CanMakeMove(startIndex, endIndex);
        }

        public int MakePlayerMove(int startIndex, int endIndex)
        {
            return chessGame.MakeMove(startIndex, endIndex);
        }

        public void MakeAIMove(Move move)
        {
            chessGame.MakeMove(move);
        }

        public bool WhiteToMove
        {
            get
            {
                return chessGame.WhiteToMove;
            }
        }

        public Board ChessBoard
        {
            get
            {
                return chessGame.ChessBoard;
            }
        }

        public ChessGame Game
        {
            get
            {
                return chessGame;
            }
        }

        public bool IsGameOver
        {
            get
            {
                return chessGame.Moves.Count < 1; 
            }
        }
    }
}
