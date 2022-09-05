using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
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

        public MainWindow()
        {
            InitializeComponent();
            Steam.RunSteam();

            var gameLoadThread = new Thread(() => SteamAppRepository.CreateOwnedGamesJson(Steam.GetClientSteamId()));
            gameLoadThread.Start();
        }

        private void OnVersionDisplayLoaded(object sender, RoutedEventArgs e) =>
            VersionText.Text = AppInformation.VERSION;

        private void Top_OnMouseDown(object sender, MouseButtonEventArgs e) => DragMove();

        private void TopLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
                WindowState = WindowState != WindowState.Normal ? WindowState.Normal : WindowState.Maximized;
        }

        private void ExitButtonClick(object sender, RoutedEventArgs e) => Environment.Exit(Environment.ExitCode);
        private void MinimizeButtonClick(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void MaximizeButtonClick(object sender, RoutedEventArgs e) => WindowState =
            WindowState != WindowState.Normal ? WindowState.Normal : WindowState.Maximized;

        private void UserInformationLoaded(object sender, RoutedEventArgs e)
        {
            var steamClient = SteamClientRepository.GetSteamClientInformation();
            UserNameTextBlock.Text = Steam.GetClientName();
            UserLevelTextBlock.Text = "Level: " + steamClient.Level;
            UserImage.Source = new BitmapImage(new Uri(steamClient.Avatar!));

            if (steamClient.AvatarFrame is not null)
                UserImageFrame.Source = new BitmapImage(new Uri(steamClient.AvatarFrame!));
        }
    }
}
