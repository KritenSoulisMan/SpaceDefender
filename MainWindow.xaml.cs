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

namespace SpaceDefender
{

    public partial class MainWindow : Window
    {
        private GameViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new GameViewModel(GameCanvas, PlayerShip);
            DataContext = _viewModel;

            KeyDown += MainWindow_KeyDown;
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                _viewModel.Shoot();
            }
            else
            {
                _viewModel.MovePlayer(e.Key);
            }
        }
    }

}
