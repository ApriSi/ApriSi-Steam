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
using ApriSiSteam.BL.Repositories;
using ApriSiSteam.WPF.UserControls;

namespace ApriSiSteam.WPF.Pages
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        private GamePage gamePage = new GamePage();

        public MainPage()
        {
            InitializeComponent();

            GameFrame.Navigate(gamePage);
        }

        private void ContentControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var gameControl = sender as GameControl;
            gamePage.SetData(gameControl.GameName, gameControl.Image, gameControl.GameID);
        }


        private void OnGamesLoaded(object sender, RoutedEventArgs e)
        {
            var games = SteamAppRepository.ReadOwnedGames();
            foreach (var gameControl in games!.Select(game => new GameControl(game.Name!, game.Image!, game.Appid)))
            {
                GamesItemControl.Items.Add(gameControl);
            }


        }

        private void OnSteamTagsLoaded(object sender, RoutedEventArgs e)
        {
            var tags = SteamAppRepository.GetTags();

            foreach (var checkBox in tags.Select(tag => new CheckBox()
                     {
                         Content = tag,
                         Foreground = Brushes.White
                     }))
            {
                SteamTagsList.Items.Add(checkBox);
            }
        }
    }
}
