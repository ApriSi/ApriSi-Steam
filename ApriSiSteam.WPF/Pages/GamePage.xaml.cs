using System.Diagnostics;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ApriSiSteam.BL;

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

            var data = Scraper.Scrape($"https://store.steampowered.com/app/{gameID}", true);
            var description = data.SelectSingleNode("//div[@class='game_description_snippet']");
            if (description != null)
                GameDescriptionTextBlock.Text = HttpUtility.HtmlDecode(description.InnerHtml.Remove(0, 10));
            else
                GameDescriptionTextBlock.Text = "No Description";
            

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
