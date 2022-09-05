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
using ApriSiSteam.BL;

namespace ApriSiSteam.WPF.UserControls
{
    /// <summary>
    /// Interaction logic for GameControl.xaml
    /// </summary>
    public partial class GameControl : UserControl
    {
        public string GameName;
        public BitmapImage Image;
        public string GameID;
        public GameControl(string gameName, string imageUrl, string gameId)
        {
            GameName = gameName;
            Image = new BitmapImage(new Uri(imageUrl));
            GameID = gameId;

            InitializeComponent();

            SetData();
        }

        private void SetData()
        {
            if (Steam.IsGameInstalled(GameID))
                GamePanel.Background = (Brush)new BrushConverter().ConvertFromString("#01A299")!;

            GameImage.Source = Image;
            GameNameText.Text = GameName;
        }
    }
}
