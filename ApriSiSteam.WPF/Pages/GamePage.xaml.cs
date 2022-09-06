using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ApriSiSteam.WPF.Pages
{
    /// <summary>
    /// Interaction logic for GamePage.xaml,
    /// Interaction Interaction Interaction... It's all in my head, It's all in my head, it's all in my head
    /// </summary>
    public partial class GamePage : Page
    {
        private string gameID;
        public GamePage()
        {
            InitializeComponent();
        }

        public void SetData(string name, ImageSource image, string id)
        {
            GameTitleTextBlock.Text = name;
            GameImage.Source = image;
            gameID = id;
        }

        private void GamePlayButtonOnClick(object sender, RoutedEventArgs e)
        {
            var runGameProcess = new Process();
            runGameProcess.StartInfo.FileName = $"steam://run/{gameID}";
            runGameProcess.StartInfo.UseShellExecute = true;
            runGameProcess.Start();
        }
    }
}
