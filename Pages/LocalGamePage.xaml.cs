using ChessWPF.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace ChessWPF
{
    /// <summary>
    /// Interaction logic for LocalGamePage.xaml
    /// </summary>
    public partial class LocalGamePage : Page
    {
        private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private MediaPlayer mediaPlayer = new MediaPlayer();
        private Uri moveUri = new Uri("./Misc/move.mp3", UriKind.Relative);
        private Uri captureUri = new Uri("./Misc/caputre.mp3", UriKind.Relative);

        private ChessGame Game = new ChessGame();

        // -1 represents a not selected stated
        private int PreviousSelectedSquare = -1;
        private int NextSelectedSquare = -1;

        public LocalGamePage()
        {
            logger.Info("Setting up local game page");
            InitializeComponent();
        }


        /// <summary>
        /// When the chess board control has loaded we first set up the board which creates the coloured squares,
        /// then we load the pieces of the current position to the board.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChessBoardControl_Loaded(object sender, RoutedEventArgs e)
        {
            ChessBoardControl.SetupBoard();
            ChessBoardControl.LoadPosition(Game.ChessBoard.GetBoard());

            ChessBoardControl.ChessBoardGrid.MouseLeftButtonDown += ChessBoardGrid_MouseLeftButtonDown;
            ChessBoardControl.ChessBoardGrid.MouseRightButtonDown += ChessBoardGrid_MouseRightButtonDown;
        }

        private void ChessBoardGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            int index = ChessBoardControl.Grid_HitTest(e);

            if (PreviousSelectedSquare == -1)
            {
                PreviousSelectedSquare = index;
            } else
            {
                NextSelectedSquare = index;

                if (Game.CanMakeMove(PreviousSelectedSquare, NextSelectedSquare))
                {
                    ChessBoardControl.MovePiece(PreviousSelectedSquare, NextSelectedSquare);
                    mediaPlayer.Open(moveUri);
                    mediaPlayer.Play();
                    logger.Info("Can make move");
                    PreviousSelectedSquare = -1;
                    NextSelectedSquare = -1;
                } else
                {
                    PreviousSelectedSquare = -1;
                    NextSelectedSquare = -1;
                }
            }
        }

        /// <summary>
        /// Cancel the moves
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChessBoardGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            PreviousSelectedSquare = -1;
            NextSelectedSquare = -1;
        }
    }
}
