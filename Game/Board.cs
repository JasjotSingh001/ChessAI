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

        public const int WhiteIndex = 0;
        public const int BlackIndex = 1;

        private int[] board = new int[64];

        private int[] kingIndex = new int[2];
        private PieceList[] pawns = new PieceList[2];
        private PieceList[] knights = new PieceList[2];
        private PieceList[] bishops = new PieceList[2];
        private PieceList[] rooks = new PieceList[2];
        private PieceList[] queens = new PieceList[2];

        private bool canWhiteCastleKingside = true;
        private bool canWhiteCastleQueenside = true;
        private bool canBlackCastleKingside = true;
        private bool canBlackCastleQueenside = true;
        private bool isWhiteToMove = true;

        //-1 represents no potential enpassant capture
        private int enPassantFile = -1;
        private byte movesWithoutCaptureOrPawnMove;
        private Move previousMove;

        public Board()
        {
            pawns[0] = new PieceList(8); pawns[1] = new PieceList(8);
            knights[0] = new PieceList(10); knights[1] = new PieceList(10);
            bishops[0] = new PieceList(10); bishops[1] = new PieceList(10);
            rooks[0] = new PieceList(10); rooks[1] = new PieceList(10);
            queens[0] = new PieceList(9); queens[1] = new PieceList(9);
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
                        file += (int)char.GetNumericValue(symbol);
                    } else
                    {
                        int pieceColour = (char.IsUpper(symbol)) ? Piece.White : Piece.Black;
                        int pieceType = pieceTypeFromSymbol[char.ToLower(symbol)];
                        board[(rank * 8) + file] = pieceColour | pieceType;

                        if (pieceColour == Piece.White)
                        {
                            switch (pieceType)
                            {
                                case Piece.Pawn:
                                    pawns[0].AddPieceAtIndex(rank * 8 + file);
                                    break;
                                case Piece.Knight:
                                    knights[0].AddPieceAtIndex(rank * 8 + file);
                                    break;
                                case Piece.Bishop:
                                    bishops[0].AddPieceAtIndex(rank * 8 + file);
                                    break;
                                case Piece.Rook:
                                    rooks[0].AddPieceAtIndex(rank * 8 + file);
                                    break;
                                case Piece.Queen:
                                    queens[0].AddPieceAtIndex(rank * 8 + file);
                                    break;
                                case Piece.King:
                                    kingIndex[0] = rank * 8 + file;
                                    break;
                            }
                        } else
                        {
                            switch (pieceType)
                            {
                                case Piece.Pawn:
                                    pawns[1].AddPieceAtIndex(rank * 8 + file);
                                    break;
                                case Piece.Knight:
                                    knights[1].AddPieceAtIndex(rank * 8 + file);
                                    break;
                                case Piece.Bishop:
                                    bishops[1].AddPieceAtIndex(rank * 8 + file);
                                    break;
                                case Piece.Rook:
                                    rooks[1].AddPieceAtIndex(rank * 8 + file);
                                    break;
                                case Piece.Queen:
                                    queens[1].AddPieceAtIndex(rank * 8 + file);
                                    break;
                                case Piece.King:
                                    kingIndex[1] = rank * 8 + file;
                                    break;
                            }
                        }
                        file++;
                    }
                }
            }
        }

        public void MakeMove(Move move)
        {
            int startSquare = move.StartSquare; 
            int endSquare = move.EndSquare;

            int colourToMoveIndex = ColourToMoveIndex();
            int pieceToMove = board[startSquare];

            PieceList pieceToMovePieceList;
            PieceList pieceToBeCapturedList;

            if (Piece.PieceType(pieceToMove) == Piece.King)
            {
                kingIndex[colourToMoveIndex] = endSquare;
            } else
            {
                pieceToMovePieceList = GetPieceList(colourToMoveIndex, pieceToMove);
                pieceToMovePieceList.MovePiece(startSquare, endSquare);
            }

            enPassantFile = -1;
            if (move.MoveFlag == Move.Flag.EnPassantCapture)
            {
                int posOfCapturedPawn = endSquare + (colourToMoveIndex == 0 ? -8 : 8);

                pawns[1 - colourToMoveIndex].RemovePieceAtIndex(posOfCapturedPawn);
                board[posOfCapturedPawn] = Piece.None;
            }
            if (move.MoveFlag == Move.Flag.PawnTwoForward)
            {
                enPassantFile = BoardRepresentation.FileIndex(startSquare);
            }

            if (move.MoveFlag == Move.Flag.Castling)
            {
                if (endSquare == 6 || endSquare == 62)
                {
                    kingIndex[colourToMoveIndex] = endSquare;
                    
                    if (colourToMoveIndex == 0)
                    {
                        rooks[0].MovePiece(7, 5);
                        board[5] = board[7];
                        board[7] = Piece.None;
                    } else
                    {
                        rooks[1].MovePiece(63, 61);
                        board[61] = board[63];
                        board[63] = Piece.None;
                    }
                } else
                {
                    kingIndex[colourToMoveIndex] = endSquare;

                    if (colourToMoveIndex == 0)
                    {
                        rooks[0].MovePiece(0, 3);
                        board[3] = board[0];
                        board[0] = Piece.None;
                    } else
                    {
                        rooks[1].MovePiece(56, 59);
                        board[59] = board[56];
                        board[56] = Piece.None;
                    }
                }
            }

            if (board[endSquare] != Piece.None)
            {
                int pieceToBeCaptured = board[endSquare];
                pieceToBeCapturedList = GetPieceList(1 - colourToMoveIndex, pieceToBeCaptured);
                pieceToBeCapturedList.RemovePieceAtIndex(endSquare);
            }

            board[endSquare] = board[startSquare];
            board[startSquare] = Piece.None;

            isWhiteToMove = !isWhiteToMove;
        }

        public void UnmakeMove()
        {

        }

        public int[] GetBoard()
        {
            return board;
        }

        public int GetPiece(int index)
        {
            return board[index];
        }

        public int[] GetKingList()
        {
            return kingIndex;
        } 

        public int GetKingIndex(int colourIndex)
        {
            return kingIndex[colourIndex];
        }

        public PieceList GetPieceList(int colourIndex, int pieceType)
        {
            PieceList pieceList;

            switch (Piece.PieceType(pieceType))
            {
                case Piece.Pawn:
                    pieceList = pawns[colourIndex];
                    break;
                case Piece.Knight:
                    pieceList = knights[colourIndex];
                    break;
                case Piece.Bishop: 
                    pieceList = bishops[colourIndex];
                    break;
                case Piece.Rook:
                    pieceList = rooks[colourIndex];
                    break;
                case Piece.Queen:
                    pieceList = queens[colourIndex];
                    break;
                default:
                    pieceList = queens[colourIndex]; 
                    break;
            }
            
            return pieceList;
        }

        public Move GetPreviousMove()
        {
            return previousMove;
        }

        public bool IsWhiteToMove()
        {
            return isWhiteToMove;
        }

        public int ColourToMove()
        {
            return isWhiteToMove ? Piece.White : Piece.Black;
        }

        public int ColourToMoveIndex()
        {
            return isWhiteToMove ? WhiteIndex : BlackIndex;
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

        public bool CanCastleKingside
        {
            get
            {
                return canWhiteCastleKingside || canBlackCastleKingside;
            }
        }

        public bool CanCastleQueenside
        {
            get
            {
                return canWhiteCastleQueenside || canBlackCastleQueenside;
            }
        }

        public int EnpassantFile
        {
            get
            {
                return enPassantFile;
            }
        }
    }
}
