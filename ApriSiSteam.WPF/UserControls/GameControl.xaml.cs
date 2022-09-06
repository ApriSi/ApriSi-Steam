using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public string Image;
        public string GameID;
        public GameControl(string gameName, string imageUrl, string gameId)
        {
            GameName = gameName;
            Image = imageUrl;
            GameID = gameId;

            InitializeComponent();

            SetData();
        }

        public void SetData()
        {
            if (Steam.IsGameInstalled(GameID))
                GamePanel.Background = (Brush)new BrushConverter().ConvertFromString("#01A299")!;

            GameImage.Source = new BitmapImage(new Uri(Image));
            GameNameText.Text = GameName;
        }
    }
}
