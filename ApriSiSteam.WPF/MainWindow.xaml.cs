using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
        public MainWindow()
        {
            InitializeComponent();
            Steam.RunSteam();

            var steamClient = SteamClientRepository.GetSteamClientInformation();

                UserNameTextBlock.Text = Steam.GetClientName();
                UserLevelTextBlock.Text = "Level: " + steamClient.Level;
                UserImage.Source = new BitmapImage(new Uri(steamClient.Avatar!));

            if (steamClient.AvatarFrame is not null)
                Dispatcher.Invoke(() => UserImageFrame.Source = new BitmapImage(new Uri(steamClient.AvatarFrame!)));
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

        private async void UserInformationLoaded(object sender, RoutedEventArgs e)
        {
            var friendsThread = new Thread(() =>
            {
                var steamClient = SteamClientRepository.GetSteamClientInformation();
                Dispatcher.Invoke(() =>
                {
                    UserNameTextBlock.Text = Steam.GetClientName();
                    UserLevelTextBlock.Text = "Level: " + steamClient.Level;
                    UserImage.Source = new BitmapImage(new Uri(steamClient.Avatar!));
                });

                if (steamClient.AvatarFrame is not null)
                  Dispatcher.Invoke(() =>  UserImageFrame.Source = new BitmapImage(new Uri(steamClient.AvatarFrame!)));

                foreach (var friend in SteamFriendRepository.GetFriends())
                {
                    Dispatcher.Invoke(() => { 
                        var friendControl = new FriendControl(friend.Name!, friend.Avatar!, friend.SteamId!);
                        FriendList.Items.Add(friendControl);
                    });
                }

                while (!File.Exists("FriendGames.json"))
                {
                    Thread.Sleep(20);
                }

                foreach (var item in FriendList.Items)
                {
                    var control = item as FriendControl;
                    Dispatcher.Invoke(() => control!.SetData());
                }
            });

            friendsThread.Start();
        }

        public void SetPage(Page page)
        {
            
            PageFrame.Navigate(page);
        }

        private void FriendControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var friendControl = sender as FriendControl;

            if (FriendImageList.Items.Cast<Image>().Any(friend => friend.DataContext.ToString() == friendControl!.SteamId))
                return;

            var friendImage = new Image()
            {
                Source = friendControl!.ImageDisplay.Source,
                DataContext = friendControl.SteamId
            };

            friendImage.MouseDown += RemoveFriendMouseDown;

            FriendImageList.Items.Add(friendImage);

            var mainPage = PageFrame.Content as MainPage;
            mainPage!.SortGamesControls();
        }
        private void RemoveFriendMouseDown(object sender, MouseButtonEventArgs e)
        {
            FriendImageList.Items.Remove(sender);
            var mainPage = PageFrame.Content as MainPage;
            mainPage!.SortGamesControls();
        }
    }
}
