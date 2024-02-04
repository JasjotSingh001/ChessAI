using ChessWPF.AI;
using ChessWPF.Game;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static ChessWPF.Game.Move;

namespace ChessWPF
{
    /// <summary>
    /// Interaction logic for AIGamePage.xaml
    /// </summary>
    public partial class AIGamePage : Page
    {
        private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private MediaPlayer mediaPlayer = new MediaPlayer();
        private Uri moveUri = new Uri("./Misc/move.mp3", UriKind.Relative);
        private Uri captureUri = new Uri("./Misc/caputre.mp3", UriKind.Relative);

        private AIGameHandler aiGameHandler = new AIGameHandler();
        private bool playingAsWhite = true;

        // -1 represents a not selected state
        private int PreviousSelectedSquare = -1;
        private int NextSelectedSquare = -1;

        private BackgroundWorker backgroundWorker = new BackgroundWorker();

        public AIGamePage()
        {
            logger.Info("Setting up AI Game Page");
            InitializeComponent();
        }

        // When the chess board control has loaded we first set up the board which creates the coloured squares,
        // then we load the pieces of the current position to the board.
        private void ChessBoardControl_Loaded(object sender, RoutedEventArgs e)
        {
            ChessBoardControl.SetupBoard();
            ChessBoardControl.LoadPosition(aiGameHandler.ChessBoard.GetBoard());

            ChessBoardControl.ChessBoardGrid.MouseLeftButtonDown += ChessBoardGrid_MouseLeftButtonDown;
            ChessBoardControl.ChessBoardGrid.MouseRightButtonDown += ChessBoardGrid_MouseRightButtonDown;
        }

        private void ChessBoardGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            int index = ChessBoardControl.Grid_HitTest(e);

            if (playingAsWhite && !aiGameHandler.WhiteToMove) 
            {
                logger.Info("AI's turn to move");
                return; 
            }

            if (aiGameHandler.ChessBoard.GetPiece(index) == Piece.None && PreviousSelectedSquare == -1) return;

            if (PreviousSelectedSquare == -1)
            {
                PreviousSelectedSquare = index;
            }
            else
            {
                NextSelectedSquare = index;

                if (aiGameHandler.CanPlayerMakeMove(PreviousSelectedSquare, NextSelectedSquare))
                {
                    int flag = aiGameHandler.MakePlayerMove(PreviousSelectedSquare, NextSelectedSquare);

                    ChessBoardControl.MovePiece(PreviousSelectedSquare, NextSelectedSquare, flag);
                    mediaPlayer.Open(moveUri);
                    mediaPlayer.Play();

                    PreviousSelectedSquare = -1;
                    NextSelectedSquare = -1;

                    if (!aiGameHandler.IsGameOver)
                    {
                        aiGameHandler.MoveGenerationTest();
                        //Move move = aiGameHandler.ChooseMove();
                        //DisplayAIMove(move);
                    } else
                    {

                    }
                }
                else
                {
                    PreviousSelectedSquare = -1;
                    NextSelectedSquare = -1;
                }
            }
        }

        private void ChessBoardGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            PreviousSelectedSquare = -1;
            NextSelectedSquare = -1;
        }

        private void DisplayAIMove(Move move)
        {
            ChessBoardControl.MovePiece(move.StartSquare, move.EndSquare, move.MoveFlag);
            mediaPlayer.Open(moveUri);
            mediaPlayer.Play();
        }
    }
}
