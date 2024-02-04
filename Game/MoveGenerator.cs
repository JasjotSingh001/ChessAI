using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChessWPF.Game.PrecomputedMoveData;

namespace ChessWPF.Game
{
    public class MoveGenerator
    {
        private Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private Board board;
        private bool includeQuietMoves = true;

        private List<Move> moves = new List<Move>();

        private bool isWhiteToMove;
        private int friendlyColour;
        private int opponentColour;
        private int friendlyKingIndex;
        private int friendlyColourIndex;
        private int opponentColourIndex;

        private bool inCheck;
        private bool inDoubleCheck;
        private bool pinsExistInPosition;

        private ulong checkRayBitmask;
        private ulong pinRayBitmask;
        private ulong opponentKnightAttacks;
        private ulong opponentAttackMapNoPawns;
        private ulong opponentSlidingAttackMap;

        public ulong opponentAttackMap;
        public ulong opponentPawnAttackMap;

        public List<Move> GenerateMoves(Board board, bool includeQuietMoves = true)
        {
            this.board = board;
            this.includeQuietMoves = includeQuietMoves;

            ResetData();
            logger.Info(isWhiteToMove); //check this

            CalculateAttackData();
            GenerateKingMoves();

            if (inDoubleCheck)
            {
                logger.Info("In double check");
                return moves;
            }

            GenerateSlidingMoves();
            GenerateKnightMoves();
            GeneratePawnMoves();

            return moves;
        }

        private void ResetData()
        {
            moves = new List<Move>();
            inCheck = false;
            inDoubleCheck = false;
            pinsExistInPosition = false;
            checkRayBitmask = 0;
            pinRayBitmask = 0;

            isWhiteToMove = board.IsWhiteToMove();
            friendlyColour = board.ColourToMove();
            opponentColour = friendlyColour == Piece.White ? Piece.Black : Piece.White;
            friendlyKingIndex = board.GetKingIndex(board.ColourToMoveIndex());
            friendlyColourIndex = board.IsWhiteToMove() ? Board.WhiteIndex : Board.BlackIndex;
            opponentColourIndex = 1 - friendlyColourIndex;
        }

        private void CalculateAttackData()
        {   
            GenerateSlidingAttackMap();

            //We will search squares in all directions around our king for checks/pins
            int startDirIndex = 0;
            int endDirIndex = 8;

            //Which directions to check based on queen, bishop, rook count
            if (board.GetPieceList(opponentColourIndex, Piece.Queen).GetCount() == 0)
            {
                startDirIndex = (board.GetPieceList(opponentColourIndex, Piece.Rook).GetCount() > 0) ? 0 : 4;
                endDirIndex = (board.GetPieceList(opponentColourIndex, Piece.Bishop).GetCount() > 0) ? 8 : 4;
            }

            for (int dir = startDirIndex; dir < endDirIndex; dir++)
            {
                bool isDiagonal = dir > 3;

                int n = NumSquaresToEdge[friendlyKingIndex][dir];
                int directionOffset = DirectionOffsets[dir];
                bool isFriendlyPieceAlongRay = false;
                ulong rayMask = 0;

                for (int i = 0; i < n; i++)
                {
                    int squareIndex = friendlyKingIndex + directionOffset * (i + 1);
                    rayMask |= 1ul << squareIndex;
                    int piece = board.GetBoard()[squareIndex];

                    // This square contains a piece
                    if (piece != Piece.None)
                    {
                        if (Piece.IsColour(piece, friendlyColour))
                        {
                            // First friendly piece we have come across in this direction, so it might be pinned
                            if (!isFriendlyPieceAlongRay)
                            {
                                isFriendlyPieceAlongRay = true;
                            }
                            // This is the second friendly piece we've found in this direction, therefore pin is not possible
                            else
                            {
                                break;
                            }
                        }
                        // This square contains an enemy piece
                        else
                        {
                            int pieceType = Piece.PieceType(piece);

                            // Check if piece is in bitmask of pieces able to move in current direction
                            if (isDiagonal && Piece.IsBishopOrQueen(pieceType) || !isDiagonal && Piece.IsRookOrQueen(pieceType))
                            {
                                // Friendly piece blocks the check, so this is a pin
                                if (isFriendlyPieceAlongRay)
                                {
                                    pinsExistInPosition = true;
                                    pinRayBitmask |= rayMask;
                                }
                                // No friendly piece blocking the attack, so this is a check
                                else
                                {
                                    checkRayBitmask |= rayMask;
                                    inDoubleCheck = inCheck; // if already in check, then this is double check
                                    inCheck = true;
                                }
                                break;
                            }
                            else
                            {
                                // This enemy piece is not able to move in the current direction, and so is blocking any checks/pins
                                break;
                            }
                        }
                    }
                }

                // Stop searching for pins if in double check, as the king is the only piece able to move in that case anyway
                if (inDoubleCheck)
                {
                    break;
                }
            }

            //Knight attacks
            PieceList opponentKnights = board.GetPieceList(opponentColourIndex, Piece.Knight);
            opponentKnightAttacks = 0;
            bool isKnightCheck = false;

            for (int knightIndex = 0; knightIndex < opponentKnights.GetCount(); knightIndex++)
            {
                int startSquare = opponentKnights[knightIndex];
                opponentKnightAttacks |= KnightAttackBitboards[startSquare];

                if (!isKnightCheck && ContainsSquare(opponentKnightAttacks, friendlyKingIndex))
                {
                    isKnightCheck = true;
                    inDoubleCheck = inCheck; // if already in check, then this is double check
                    inCheck = true;
                    checkRayBitmask |= 1ul << startSquare;
                }
            }

            // Pawn attacks
            PieceList opponentPawns = board.GetPieceList(opponentColourIndex, Piece.Pawn);
            opponentPawnAttackMap = 0;
            bool isPawnCheck = false;

            for (int pawnIndex = 0; pawnIndex < opponentPawns.GetCount(); pawnIndex++)
            {
                int pawnSquare = opponentPawns[pawnIndex];
                ulong pawnAttacks = PawnAttackBitboards[pawnSquare][opponentColourIndex];
                opponentPawnAttackMap |= pawnAttacks;

                if (!isPawnCheck && ContainsSquare(pawnAttacks, friendlyKingIndex))
                {
                    isPawnCheck = true;
                    inDoubleCheck = inCheck; // if already in check, then this is double check
                    inCheck = true;
                    checkRayBitmask |= 1ul << pawnSquare;
                }
            }

            int enemyKingSquare = board.GetKingList()[opponentColourIndex];

            opponentAttackMapNoPawns = opponentSlidingAttackMap | opponentKnightAttacks | KingAttackBitboards[enemyKingSquare];
            opponentAttackMap = opponentAttackMapNoPawns | opponentPawnAttackMap;

            logger.Info("In check?: " + inCheck);
        }

        private void GenerateSlidingAttackMap()
        {
            opponentSlidingAttackMap = 0;

            PieceList enemyRooks = board.GetPieceList(opponentColourIndex, Piece.Rook);
            for (int i = 0; i < enemyRooks.GetCount(); i++)
            {
                UpdateSlidingAttackPiece(enemyRooks[i], 0, 4);
            }

            PieceList enemyQueens = board.GetPieceList(opponentColourIndex, Piece.Queen);
            for (int i = 0; i < enemyQueens.GetCount(); i++)
            {
                UpdateSlidingAttackPiece(enemyQueens[i], 0, 8);
            }

            PieceList enemyBishops = board.GetPieceList(opponentColourIndex, Piece.Bishop);
            for (int i = 0; i < enemyBishops.GetCount(); i++)
            {
                UpdateSlidingAttackPiece(enemyBishops[i], 4, 8);
            }
        }

        private void UpdateSlidingAttackPiece(int startSquare, int startDirIndex, int endDirIndex)
        {
            for (int directionIndex = startDirIndex; directionIndex < endDirIndex; directionIndex++)
            {
                int currentDirOffset = DirectionOffsets[directionIndex];
                for (int n = 0; n < NumSquaresToEdge[startSquare][directionIndex]; n++)
                {
                    int targetSquare = startSquare + currentDirOffset * (n + 1);
                    int targetSquarePiece = board.GetBoard()[targetSquare];
                    opponentSlidingAttackMap |= 1ul << targetSquare;
                    if (targetSquare != friendlyKingIndex)
                    {
                        if (targetSquarePiece != Piece.None)
                        {
                            break;
                        }
                    }
                }
            }
        }

        private void GenerateKingMoves()
        {
            for (int i = 0; i < KingMoves[friendlyKingIndex].Length; i++)
            {
                int targetSquare = KingMoves[friendlyKingIndex][i];
                int pieceOnTargetSquare = board.GetBoard()[targetSquare];

                // Skip squares occupied by friendly pieces
                if (Piece.IsColour(pieceOnTargetSquare, friendlyColour))
                {
                    continue;
                }

                bool isCapture = Piece.IsColour(pieceOnTargetSquare, opponentColour);
                if (!isCapture)
                {
                    // King can't move to square marked as under enemy control, unless he is capturing that piece
                    // Also skip if not generating quiet moves
                    if (!includeQuietMoves || SquareIsInCheckRay(targetSquare))
                    {
                        continue;
                    }
                }

                // Safe for king to move to this square
                if (!SquareIsAttacked(targetSquare))
                {
                    moves.Add(new Move(friendlyKingIndex, targetSquare));

                    // Castling:
                    if (!inCheck && !isCapture)
                    {
                        // Castle kingside
                        if ((targetSquare == 5 || targetSquare == 61) && board.CanCastleKingside)
                        {
                            int castleKingsideSquare = targetSquare + 1;
                            if (board.GetBoard()[castleKingsideSquare] == Piece.None)
                            {
                                if (!SquareIsAttacked(castleKingsideSquare))
                                {
                                    moves.Add(new Move(friendlyKingIndex, castleKingsideSquare, Move.Flag.Castling));
                                }
                            }
                        }
                        // Castle queenside
                        else if ((targetSquare == 3 || targetSquare == 59) && board.CanCastleQueenside)
                        {
                            int castleQueensideSquare = targetSquare - 1;
                            if (board.GetBoard()[castleQueensideSquare] == Piece.None && board.GetBoard()[castleQueensideSquare - 1] == Piece.None)
                            {
                                if (!SquareIsAttacked(castleQueensideSquare))
                                {
                                    moves.Add(new Move(friendlyKingIndex, castleQueensideSquare, Move.Flag.Castling));
                                }
                            }
                        }
                    }
                }
            }
        }
        private void GenerateSlidingMoves()
        {
            PieceList rooks = board.GetPieceList(friendlyColourIndex, Piece.Rook);
            for (int i = 0; i < rooks.GetCount(); i++)
            {
                GenerateSlidingPieceMoves(rooks[i], 0, 4);
            }

            PieceList bishops = board.GetPieceList(friendlyColourIndex, Piece.Bishop);
            for (int i = 0; i < bishops.GetCount(); i++)
            {
                GenerateSlidingPieceMoves(bishops[i], 4, 8);
            }

            PieceList queens = board.GetPieceList(friendlyColourIndex, Piece.Queen);
            for (int i = 0; i < queens.GetCount(); i++)
            {
                GenerateSlidingPieceMoves(queens[i], 0, 8);
            }
        }

        private void GenerateSlidingPieceMoves(int startSquare, int startDirIndex, int endDirIndex)
        {
            bool isPinned = IsPinned(startSquare);

            // If this piece is pinned, and the king is in check, this piece cannot move
            if (inCheck && isPinned)
            {
                return;
            }

            for (int directionIndex = startDirIndex; directionIndex < endDirIndex; directionIndex++)
            {
                int currentDirOffset = DirectionOffsets[directionIndex];

                // If pinned, this piece can only move along the ray towards/away from the friendly king, so skip other directions
                if (isPinned && !IsMovingAlongRay(currentDirOffset, friendlyKingIndex, startSquare))
                {
                    continue;
                }

                for (int n = 0; n < NumSquaresToEdge[startSquare][directionIndex]; n++)
                {
                    int targetSquare = startSquare + currentDirOffset * (n + 1);
                    int targetSquarePiece = board.GetBoard()[targetSquare];

                    // Blocked by friendly piece, so stop looking in this direction
                    if (Piece.IsColour(targetSquarePiece, friendlyColour))
                    {
                        break;
                    }
                    bool isCapture = targetSquarePiece != Piece.None;

                    bool movePreventsCheck = SquareIsInCheckRay(targetSquare);
                    if (movePreventsCheck || !inCheck)
                    {
                        if (includeQuietMoves || isCapture)
                        {
                            moves.Add(new Move(startSquare, targetSquare));
                        }
                    }
                    // If square not empty, can't move any further in this direction
                    // Also, if this move blocked a check, further moves won't block the check
                    if (isCapture || movePreventsCheck)
                    {
                        break;
                    }
                }
            }
        }

        private void GenerateKnightMoves()
        {
            PieceList myKnights = board.GetPieceList(friendlyColourIndex, Piece.Knight);

            for (int i = 0; i < myKnights.GetCount(); i++)
            {
                int startSquare = myKnights[i];

                // Knight cannot move if it is pinned
                if (IsPinned(startSquare))
                {
                    continue;
                }

                for (int knightMoveIndex = 0; knightMoveIndex < KnightMoves[startSquare].Length; knightMoveIndex++)
                {
                    int targetSquare = KnightMoves[startSquare][knightMoveIndex];
                    int targetSquarePiece = board.GetBoard()[targetSquare];
                    bool isCapture = Piece.IsColour(targetSquarePiece, opponentColour);
                    if (includeQuietMoves || isCapture)
                    {
                        // Skip if square contains friendly piece, or if in check and knight is not interposing/capturing checking piece
                        if (Piece.IsColour(targetSquarePiece, friendlyColour) || (inCheck && !SquareIsInCheckRay(targetSquare)))
                        {
                            continue;
                        }
                        moves.Add(new Move(startSquare, targetSquare));
                    }
                }
            }
        }

        private void GeneratePawnMoves()
        {
            PieceList myPawns = board.GetPieceList(friendlyColourIndex, Piece.Pawn);
            int pawnOffset = (friendlyColour == Piece.White) ? 8 : -8;
            int startRank = (board.IsWhiteToMove()) ? 1 : 6;
            int finalRankBeforePromotion = (board.IsWhiteToMove()) ? 6 : 1;

            int enPassantFile = board.EnpassantFile;
            int enPassantSquare = -1;
            if (enPassantFile != -1)
            {
                enPassantSquare = 8 * ((board.IsWhiteToMove()) ? 5 : 2) + enPassantFile;
            }

            for (int i = 0; i < myPawns.GetCount(); i++)
            {
                int startSquare = myPawns[i];
                int rank = BoardRepresentation.RankIndex(startSquare);
                bool oneStepFromPromotion = rank == finalRankBeforePromotion;

                if (includeQuietMoves)
                {

                    int squareOneForward = startSquare + pawnOffset;

                    // Square ahead of pawn is empty: forward moves
                    if (board.GetBoard()[squareOneForward] == Piece.None)
                    {
                        // Pawn not pinned, or is moving along line of pin
                        if (!IsPinned(startSquare) || IsMovingAlongRay(pawnOffset, startSquare, friendlyKingIndex))
                        {
                            // Not in check, or pawn is interposing checking piece
                            if (!inCheck || SquareIsInCheckRay(squareOneForward))
                            {
                                if (oneStepFromPromotion)
                                {
                                    MakePromotionMoves(startSquare, squareOneForward);
                                }
                                else
                                {
                                    moves.Add(new Move(startSquare, squareOneForward));
                                }
                            }

                            // Is on starting square (so can move two forward if not blocked)
                            if (rank == startRank)
                            {
                                int squareTwoForward = squareOneForward + pawnOffset;
                                if (board.GetBoard()[squareTwoForward] == Piece.None)
                                {
                                    // Not in check, or pawn is interposing checking piece
                                    if (!inCheck || SquareIsInCheckRay(squareTwoForward))
                                    {
                                        moves.Add(new Move(startSquare, squareTwoForward, Move.Flag.PawnTwoForward));
                                    }
                                }
                            }
                        }
                    }
                }

                for (int j = 0; j < 2; j++)
                {
                    // Check if square exists diagonal to pawn
                    if (NumSquaresToEdge[startSquare][PawnAttackDirections[friendlyColourIndex][j]] > 0)
                    {
                        // move in direction friendly pawns attack to get square from which enemy pawn would attack
                        int pawnCaptureDir = DirectionOffsets[PawnAttackDirections[friendlyColourIndex][j]];
                        int targetSquare = startSquare + pawnCaptureDir;
                        int targetPiece = board.GetBoard()[targetSquare];

                        // If piece is pinned, and the square it wants to move to is not on same line as the pin, then skip this direction
                        if (IsPinned(startSquare) && !IsMovingAlongRay(pawnCaptureDir, friendlyKingIndex, startSquare))
                        {
                            continue;
                        }

                        // Regular capture
                        if (Piece.IsColour(targetPiece, opponentColour))
                        {
                            // If in check, and piece is not capturing/interposing the checking piece, then skip to next square
                            if (inCheck && !SquareIsInCheckRay(targetSquare))
                            {
                                continue;
                            }
                            if (oneStepFromPromotion)
                            {
                                MakePromotionMoves(startSquare, targetSquare);
                            }
                            else
                            {
                                moves.Add(new Move(startSquare, targetSquare));
                            }
                        }

                        // Capture en-passant
                        if (targetSquare == enPassantSquare)
                        {
                            int epCapturedPawnSquare = targetSquare + ((board.IsWhiteToMove()) ? -8 : 8);
                            if (!InCheckAfterEnPassant(startSquare, targetSquare, epCapturedPawnSquare))
                            {
                                moves.Add(new Move(startSquare, targetSquare, Move.Flag.EnPassantCapture));
                            }
                        }
                    }
                }
            }
        }

        private void MakePromotionMoves(int startIndex, int endIndex)
        {
            moves.Add(new Move(startIndex, endIndex, Move.Flag.PromoteToQueen));
            //if (promotionsToGenerate == PromotionMode.All)
            //{
            //    moves.Add(new Move(fromSquare, toSquare, Move.Flag.PromoteToKnight));
            //    moves.Add(new Move(fromSquare, toSquare, Move.Flag.PromoteToRook));
            //    moves.Add(new Move(fromSquare, toSquare, Move.Flag.PromoteToBishop));
            //}
            //else if (promotionsToGenerate == PromotionMode.QueenAndKnight)
            //{
            //    moves.Add(new Move(fromSquare, toSquare, Move.Flag.PromoteToKnight));
            //}
        }

        private bool ContainsSquare(ulong bitboard, int square)
        {
            return ((bitboard >> square) & 1) != 0;
        }

        private bool IsPinned(int square)
        {
            return pinsExistInPosition && ((pinRayBitmask >> square) & 1) != 0;
        }

        private bool IsMovingAlongRay(int rayDir, int startSquare, int targetSquare)
        {
            int moveDir = DirectionLookup[targetSquare - startSquare + 63];
            return (rayDir == moveDir || -rayDir == moveDir);
        }

        private bool SquareIsInCheckRay(int square)
        {
            return inCheck && ((checkRayBitmask >> square) & 1) != 0;
        }

        private bool SquareIsAttacked(int square)
        {
            return ContainsSquare(opponentAttackMap, square);
        }

        private bool InCheckAfterEnPassant(int startSquare, int targetSquare, int epCapturedPawnSquare)
        {
            // Update board to reflect en-passant capture
            board.GetBoard()[targetSquare] = board.GetBoard()[startSquare];
            board.GetBoard()[startSquare] = Piece.None;
            board.GetBoard()[epCapturedPawnSquare] = Piece.None;

            bool inCheckAfterEpCapture = false;
            if (SquareAttackedAfterEPCapture(epCapturedPawnSquare, startSquare))
            {
                inCheckAfterEpCapture = true;
            }

            // Undo change to board
            board.GetBoard()[targetSquare] = Piece.None;
            board.GetBoard()[startSquare] = Piece.Pawn | friendlyColour;
            board.GetBoard()[epCapturedPawnSquare] = Piece.Pawn | opponentColour;
            return inCheckAfterEpCapture;
        }

        bool SquareAttackedAfterEPCapture(int epCaptureSquare, int capturingPawnStartSquare)
        {
            if (ContainsSquare(opponentAttackMapNoPawns, friendlyKingIndex))
            {
                return true;
            }

            // Loop through the horizontal direction towards ep capture to see if any enemy piece now attacks king
            int dirIndex = (epCaptureSquare < friendlyKingIndex) ? 2 : 3;
            for (int i = 0; i < NumSquaresToEdge[friendlyKingIndex][dirIndex]; i++)
            {
                int squareIndex = friendlyKingIndex + DirectionOffsets[dirIndex] * (i + 1);
                int piece = board.GetBoard()[squareIndex];
                if (piece != Piece.None)
                {
                    // Friendly piece is blocking view of this square from the enemy.
                    if (Piece.IsColour(piece, friendlyColour))
                    {
                        break;
                    }
                    // This square contains an enemy piece
                    else
                    {
                        if (Piece.IsRookOrQueen(piece))
                        {
                            return true;
                        }
                        else
                        {
                            // This piece is not able to move in the current direction, and is therefore blocking any checks along this line
                            break;
                        }
                    }
                }
            }

            // check if enemy pawn is controlling this square (can't use pawn attack bitboard, because pawn has been captured)
            for (int i = 0; i < 2; i++)
            {
                // Check if square exists diagonal to friendly king from which enemy pawn could be attacking it
                if (NumSquaresToEdge[friendlyKingIndex][PawnAttackDirections[friendlyColourIndex][i]] > 0)
                {
                    // move in direction friendly pawns attack to get square from which enemy pawn would attack
                    int piece = board.GetBoard()[friendlyKingIndex + DirectionOffsets[PawnAttackDirections[friendlyColourIndex][i]]];
                    if (piece == (Piece.Pawn | opponentColour)) // is enemy pawn
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public List<Move> GetMoves()
        {
            return moves;
        }

        public bool InCheck
        {
            get { return inCheck; }
        }
    }
}
