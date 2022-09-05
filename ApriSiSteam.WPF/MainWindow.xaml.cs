using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ApriSiSteam.BL;
using ApriSiSteam.BL.Models;
using ApriSiSteam.BL.Repositories;
using ApriSiSteam.WPF.Pages;
using ApriSiSteam.WPF.UserControls;

namespace ApriSiSteam.WPF
{
    /// <summary>
    ///  And I would walk 500 miles And I would roll 500 more Just to be the man who rolled a thousand miles
    /// </summary>
    public partial class MainWindow : Window
    {
        private static List<SteamFriend> SelectedFriends = new();
        private GamePage gamePage = new GamePage();

        public MainWindow()
        {
            // steam://run/480
            InitializeComponent();
            Steam.RunSteam();
            SteamAppRepository.CreateOwnedGamesJson(Steam.GetClientSteamId());

            GameFrame.Navigate(gamePage);
        }

        private void OnVersionDisplayLoaded(object sender, RoutedEventArgs e) => VersionText.Text = AppInformation.VERSION;

        private void Top_OnMouseDown(object sender, MouseButtonEventArgs e) => DragMove();

        private void TopLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ClickCount >= 2)
                WindowState = WindowState != WindowState.Normal ? WindowState.Normal : WindowState.Maximized;
        }

        private void ExitButtonClick(object sender, RoutedEventArgs e) => Environment.Exit(Environment.ExitCode);
        private void MinimizeButtonClick(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void MaximizeButtonClick(object sender, RoutedEventArgs e) => WindowState = WindowState != WindowState.Normal ? WindowState.Normal : WindowState.Maximized;
        

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

        private void UserInformationLoaded(object sender, RoutedEventArgs e)
        {
            var steamClient = SteamClientRepository.GetSteamClientInformation();
            UserNameTextBlock.Text = Steam.GetClientName();
            UserLevelTextBlock.Text = "Level: " + steamClient.Level;
            GamesCountTextBlock.Text = "Games: " + SteamAppRepository.ReadOwnedGames()!.Count;
            UserImage.Source = new BitmapImage(new Uri(steamClient.Avatar!));

            if(steamClient.AvatarFrame is not null)
                UserImageFrame.Source = new BitmapImage(new Uri(steamClient.AvatarFrame!));
        }
    }
}
