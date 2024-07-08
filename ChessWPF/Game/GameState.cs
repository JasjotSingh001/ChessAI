using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessWPF.Game
{
    public readonly struct GameState
    {
        public readonly bool canWhiteCastleKingside;
        public readonly bool canWhiteCastleQueenside;
        public readonly bool canBlackCastleKingside;
        public readonly bool canBlackCastleQueenside;

        public readonly int enpassantFile;

        public readonly int capturedPiece;

        //public readonly int fiftyMoveCounter;

        public bool CanWhiteCastle
        {
            get
            {
                return canWhiteCastleKingside || canWhiteCastleQueenside;
            }
        }

        public bool CanBlackCastle
        {
            get
            {
                return canBlackCastleKingside || canBlackCastleQueenside;
            }
        }

        public bool CanWhiteCastleKingside
        {
            get
            {
                return canWhiteCastleKingside;
            }
        }

        public bool CanWhiteCastleQueenside
        {
            get
            {
                return canWhiteCastleQueenside;
            }
        }

        public bool CanBlackCastleKingside
        {
            get
            {
                return canBlackCastleKingside;
            }
        }

        public bool CanBlackCastleQueenside
        {
            get
            {
                return canBlackCastleQueenside;
            }
        }

        public int EnpassantFile
        {
            get
            {
                return enpassantFile;
            }
        }

        public int CapturedPiece
        {
            get
            {
                return capturedPiece;
            }
        }

        /*public int FiftyMoveCounter
        {
            get
            {
                return fiftyMoveCounter;
            }
        }*/

        public GameState(bool cWK, bool cWQ, bool cBK, bool cBQ, int ePF, int cP)
        { 
            canWhiteCastleKingside = cWK;
            canWhiteCastleQueenside = cWQ;
            canBlackCastleKingside = cBQ;
            canBlackCastleQueenside = cBQ;
            enpassantFile = ePF;
            capturedPiece = cP;
            //fiftyMoveCounter = fifty;
        }

        public GameState(bool[] castleRights, int ePF, int cP)
        {
            canWhiteCastleKingside = castleRights[0];
            canWhiteCastleQueenside = castleRights[1];
            canBlackCastleKingside = castleRights[2];
            canBlackCastleQueenside = castleRights[3];
            enpassantFile = ePF;
            capturedPiece = cP;
            //fiftyMoveCounter = ePF;
        }
    }
}
