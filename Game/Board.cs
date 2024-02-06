using NLog.Fluent;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
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

        private Stack<GameState> gameStateHistory = new Stack<GameState>();

        private bool canWhiteCastleKingside = true;
        private bool canWhiteCastleQueenside = true;
        private bool canBlackCastleKingside = true;
        private bool canBlackCastleQueenside = true;
        private int enPassantFile = -1; //-1 represents no potential enpassant capture
        private int capturedPiece = Piece.None;
        private int fiftyMoveCounter = 0;

        private Move previousMove;

        private bool isWhiteToMove = true;

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

            int capturedPiece = Piece.None;
            GameState currentGameState = new GameState(
                canWhiteCastleKingside, canWhiteCastleQueenside, canBlackCastleKingside, canBlackCastleQueenside,
                enPassantFile, capturedPiece
            );
            //gameStateHistory.Push(currentGameState);
        }

        public void MakeMove(Move move)
        {
            int startSquare = move.StartSquare; 
            int endSquare = move.EndSquare;

            int colourToMoveIndex = ColourToMoveIndex();

            int pieceToMove = board[startSquare];
            int capturedPiece = Piece.None;

            enPassantFile = -1;

            if (board[endSquare] != Piece.None)
            {
                capturedPiece = board[endSquare];
                this.capturedPiece = capturedPiece;
                GetPieceList(1 - colourToMoveIndex, board[endSquare]).RemovePieceAtIndex(endSquare);

                if (Piece.PieceType(capturedPiece) == Piece.Rook)
                {
                    switch (endSquare)
                    {
                        case 0:
                            canWhiteCastleQueenside = false;
                            break;
                        case 7:
                            canWhiteCastleKingside = false;
                            break;
                        case 56:
                            canBlackCastleQueenside= false;
                            break;
                        case 63:
                            canBlackCastleKingside= false;
                            break;
                    }
                }
            }

            if (move.MoveFlag == Move.Flag.EnPassantCapture)
            {
                int posOfCapturedPawn = endSquare + (colourToMoveIndex == 0 ? -8 : 8);

                pawns[colourToMoveIndex].MovePiece(startSquare, endSquare);
                board[endSquare] = board[startSquare];
                board[startSquare] = Piece.None;

                pawns[1 - colourToMoveIndex].RemovePieceAtIndex(posOfCapturedPawn);
                board[posOfCapturedPawn] = Piece.None;
            } else if (move.MoveFlag == Move.Flag.PawnTwoForward)
            {
                pawns[colourToMoveIndex].MovePiece(startSquare, endSquare);
                board[endSquare] = board[startSquare];
                board[startSquare] = Piece.None;

                enPassantFile = BoardRepresentation.FileIndex(startSquare);
            } else if (move.MoveFlag == Move.Flag.Castling)
            {
                board[endSquare] = board[startSquare];
                board[startSquare] = Piece.None;

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

                if (colourToMoveIndex == 0)
                {
                    canWhiteCastleKingside = false;
                    canWhiteCastleQueenside = false;
                } else
                {
                    canBlackCastleKingside = false;
                    canBlackCastleQueenside = false;
                }
            } else if (move.IsPromotion) 
            {
                switch (move.MoveFlag)
                {
                    case Move.Flag.PromoteToQueen:
                        pawns[colourToMoveIndex].RemovePieceAtIndex(startSquare);
                        queens[colourToMoveIndex].AddPieceAtIndex(endSquare);

                        board[endSquare] = (colourToMoveIndex == 0) ? Piece.WhiteQueen : Piece.BlackQueen;
                        board[startSquare] = Piece.None;
                        break;
                    case Move.Flag.PromoteToKnight:
                        pawns[colourToMoveIndex].RemovePieceAtIndex(startSquare);
                        knights[colourToMoveIndex].AddPieceAtIndex(endSquare);

                        board[endSquare] = (colourToMoveIndex == 0) ? Piece.WhiteKnight : Piece.BlackKnight;
                        board[startSquare] = Piece.None;
                        break;
                    case Move.Flag.PromoteToBishop:
                        pawns[colourToMoveIndex].RemovePieceAtIndex(startSquare);
                        bishops[colourToMoveIndex].AddPieceAtIndex(endSquare);

                        board[endSquare] = (colourToMoveIndex == 0) ? Piece.WhiteBishop : Piece.BlackBishop;
                        board[startSquare] = Piece.None;
                        break;
                    case Move.Flag.PromoteToRook:
                        pawns[colourToMoveIndex].RemovePieceAtIndex(startSquare);
                        rooks[colourToMoveIndex].AddPieceAtIndex(endSquare);

                        board[endSquare] = (colourToMoveIndex == 0) ? Piece.WhiteRook : Piece.BlackRook;
                        board[startSquare] = Piece.None;
                        break;
                }
            } else if (Piece.PieceType(pieceToMove) == Piece.King)
            {
                kingIndex[colourToMoveIndex] = endSquare;
                board[endSquare] = board[startSquare];
                board[startSquare] = Piece.None;

                if (colourToMoveIndex == 0)
                {
                    canWhiteCastleKingside = false;
                    canWhiteCastleQueenside = false;
                } else
                {
                    canBlackCastleKingside = false;
                    canBlackCastleQueenside = false;
                }
            } else
            {
                if (Piece.PieceType(pieceToMove) == Piece.Rook)
                {
                    rooks[colourToMoveIndex].MovePiece(startSquare, endSquare);

                    switch (startSquare)
                    {
                        case 0:
                            canWhiteCastleQueenside = false;
                            break;
                        case 7:
                            canWhiteCastleKingside = false;
                            break;
                        case 56:
                            canBlackCastleQueenside = false;
                            break;
                        case 63:
                            canBlackCastleKingside = false;
                            break;
                    }
                } else
                {
                    GetPieceList(colourToMoveIndex, pieceToMove).MovePiece(startSquare, endSquare);
                }

                board[endSquare] = board[startSquare];
                board[startSquare] = Piece.None;
            }

            GameState currentGameState = new GameState(
                canWhiteCastleKingside, canWhiteCastleQueenside, canBlackCastleKingside, canBlackCastleQueenside,
                enPassantFile, capturedPiece
            );
            gameStateHistory.Push(currentGameState);

            isWhiteToMove = !isWhiteToMove;
        }

        public void UnmakeMove(Move move)
        {
            isWhiteToMove = !isWhiteToMove;

            GameState currentGameState1 = gameStateHistory.Peek();

            canWhiteCastleKingside = currentGameState1.CanWhiteCastleKingside;
            canWhiteCastleQueenside = currentGameState1.CanWhiteCastleQueenside;
            canBlackCastleKingside = currentGameState1.CanBlackCastleKingside;
            canBlackCastleQueenside = currentGameState1.CanBlackCastleQueenside;

            enPassantFile = currentGameState1.EnpassantFile;
            //fiftyMoveCounter = currentGameState1.FiftyMoveCounter;
            capturedPiece = currentGameState1.CapturedPiece;

            int startSquare = move.StartSquare;
            int endSquare = move.EndSquare;
            int pieceToMove = board[endSquare];
            int colourToMoveIndex = ColourToMoveIndex();

            if (move.MoveFlag == Move.Flag.EnPassantCapture)
            {
                int posOfCapturedPawn = endSquare + (colourToMoveIndex == 0 ? -8 : 8);

                pawns[colourToMoveIndex].MovePiece(endSquare, startSquare);
                board[startSquare] = board[endSquare];
                board[endSquare] = Piece.None;

                pawns[1 - colourToMoveIndex].AddPieceAtIndex(posOfCapturedPawn);

                if (colourToMoveIndex == 0)
                {
                    board[posOfCapturedPawn] = Piece.BlackPawn;
                } else
                {
                    board[posOfCapturedPawn] = Piece.WhitePawn;
                }
            } else if (move.MoveFlag == Move.Flag.PawnTwoForward)
            {
                pawns[colourToMoveIndex].MovePiece(endSquare, startSquare);
                board[startSquare] = board[endSquare];
                board[endSquare] = Piece.None;
            } else if (move.MoveFlag == Move.Flag.Castling)
            {
                board[startSquare] = board[endSquare];
                board[endSquare] = Piece.None;

                if (endSquare == 6 || endSquare == 62)
                {
                    kingIndex[colourToMoveIndex] = startSquare;

                    if (colourToMoveIndex == 0)
                    {
                        rooks[0].MovePiece(5, 7);
                        board[7] = board[5];
                        board[5] = Piece.None;
                    }
                    else
                    {
                        rooks[1].MovePiece(61, 63);
                        board[63] = board[61];
                        board[61] = Piece.None;
                    }
                }
                else
                {
                    kingIndex[colourToMoveIndex] = startSquare;

                    if (colourToMoveIndex == 0)
                    {
                        rooks[0].MovePiece(3, 0);
                        board[0] = board[3];
                        board[3] = Piece.None;
                    }
                    else
                    {
                        rooks[1].MovePiece(59, 56);
                        board[56] = board[59];
                        board[59] = Piece.None;
                    }
                }
            } else if (move.IsPromotion)
            {
                switch (move.MoveFlag)
                {
                    case Move.Flag.PromoteToQueen:
                        pawns[colourToMoveIndex].AddPieceAtIndex(startSquare);
                        queens[colourToMoveIndex].RemovePieceAtIndex(endSquare);

                        board[endSquare] = Piece.None;
                        if (colourToMoveIndex == 0)
                        {
                            board[startSquare] = Piece.WhitePawn;
                        } else
                        {
                            board[startSquare] = Piece.BlackPawn;
                        }
                        break;
                    case Move.Flag.PromoteToKnight:
                        pawns[colourToMoveIndex].AddPieceAtIndex(startSquare);
                        knights[colourToMoveIndex].RemovePieceAtIndex(endSquare);

                        board[endSquare] = Piece.None;
                        if (colourToMoveIndex == 0)
                        {
                            board[startSquare] = Piece.WhitePawn;
                        }
                        else
                        {
                            board[startSquare] = Piece.BlackPawn;
                        }
                        break;
                    case Move.Flag.PromoteToBishop:
                        pawns[colourToMoveIndex].AddPieceAtIndex(startSquare);
                        bishops[colourToMoveIndex].RemovePieceAtIndex(endSquare);

                        board[endSquare] = Piece.None;
                        if (colourToMoveIndex == 0)
                        {
                            board[startSquare] = Piece.WhitePawn;
                        }
                        else
                        {
                            board[startSquare] = Piece.BlackPawn;
                        }
                        break;
                    case Move.Flag.PromoteToRook:
                        pawns[colourToMoveIndex].AddPieceAtIndex(startSquare);
                        rooks[colourToMoveIndex].RemovePieceAtIndex(endSquare);

                        board[endSquare] = Piece.None;
                        if (colourToMoveIndex == 0)
                        {
                            board[startSquare] = Piece.WhitePawn;
                        }
                        else
                        {
                            board[startSquare] = Piece.BlackPawn;
                        }
                        break;
                }
            } else if (Piece.PieceType(pieceToMove) == Piece.King) 
            {
                board[startSquare] = board[endSquare];
                board[endSquare] = Piece.None;

                kingIndex[colourToMoveIndex] = startSquare;
            } else
            {
                board[startSquare] = board[endSquare];
                board[endSquare] = Piece.None;

                GetPieceList(colourToMoveIndex, pieceToMove).MovePiece(endSquare, startSquare);
            }

            if (capturedPiece != Piece.None)
            {
                logger.Info(endSquare);
                board[endSquare] = capturedPiece;
                GetPieceList(1 - colourToMoveIndex, capturedPiece).AddPieceAtIndex(endSquare);
            }

            gameStateHistory.Pop();
            GameState currentGameState = gameStateHistory.Peek();

            canWhiteCastleKingside = currentGameState.CanWhiteCastleKingside;
            canWhiteCastleQueenside = currentGameState.CanWhiteCastleQueenside;
            canBlackCastleKingside = currentGameState.CanBlackCastleKingside;
            canBlackCastleQueenside = currentGameState.CanBlackCastleQueenside;

            enPassantFile = currentGameState.EnpassantFile;
            //fiftyMoveCounter = currentGameState.FiftyMoveCounter;
            capturedPiece = currentGameState.CapturedPiece;
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
            switch (Piece.PieceType(pieceType))
            {
                case Piece.Pawn:
                    return pawns[colourIndex];
                case Piece.Knight:
                    return knights[colourIndex];
                case Piece.Bishop: 
                    return bishops[colourIndex];
                case Piece.Rook:
                    return rooks[colourIndex];
                case Piece.Queen:
                    return queens[colourIndex];
            }
            
            return rooks[colourIndex];
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
