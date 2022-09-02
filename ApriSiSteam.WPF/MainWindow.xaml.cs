using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ApriSiSteam.BL;
using ApriSiSteam.BL.Models;
using ApriSiSteam.BL.Repositories;

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
            SteamAppRepository.CreateOwnedGamesJSON(Steam.GetClientSteamId());

            var ownedGames = SteamAppRepository.ReadOwnedGames();

            if (ownedGames == null) return;
            foreach (var game in ownedGames)
            {
                Debug.WriteLine(game.Appid);
                Debug.WriteLine(game.Name);
                Debug.WriteLine(game.Image);
                Debug.WriteLine("\nCATEGORY");
                foreach (var category in game.Categories!)
                {
                    Debug.WriteLine(category);
                }

                Debug.WriteLine("\n\n");
            }

            Debug.WriteLine("Main Window Loaded");
        }

        private void OnMinimizeClicked(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void OnExitClicked(object sender, RoutedEventArgs e) => Environment.Exit(Environment.ExitCode);
        private void OnTopPanelMouseDown(object sender, MouseButtonEventArgs e) => DragMove();

        private void CheckGamesClicked(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Loading Games");
        }

        private void OnVersionDisplayLoaded(object sender, RoutedEventArgs e) => VersionLabel.Text = AppInformation.VERSION;

        private void OnProfileInformationLoaded(object sender, RoutedEventArgs e)
        {
            var steamClient = SteamClientRepository.GetSteamClientInformation();
            
            NameDisplay.Text = Steam.GetClientName();
            LevelDisplay.Text = "Level: " + steamClient.Level;
            AvatarImage.Source = new BitmapImage(new Uri(steamClient.Avatar!));

            // When OwnedGames is available
            //GameCountDisplay.Text = "Games: " + steamClient.GameCount;
        }

        private void OnCategoryListLoaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Category List Loaded");
        }

        private void OnFriendListLoaded(object sender, RoutedEventArgs e)
        {
            var friendList = Steam.GetFriends();

            foreach (var friend in friendList)
            {
                var checkBox = new CheckBox()
                {
                    Content = friend.Name,
                    DataContext = friend,
                    Foreground = Brushes.White
                };

                checkBox.Checked += OnFriendCheckboxChecked;
                checkBox.Unchecked += OnFriendCheckboxUnchecked;

                FriendList.Items.Add(checkBox);
            }
        }

        private void OnFriendCheckboxUnchecked(object sender, RoutedEventArgs e)
        {
            var checkBox = (CheckBox)sender;
            var friend = (SteamFriend)checkBox.DataContext;

            Debug.WriteLine("Removed {0} from SelectedFriends", friend.Name);
            SelectedFriends.Remove(friend);

            DisplaySelectedUsers();
        }

        private void OnFriendCheckboxChecked(object sender, RoutedEventArgs e)
        {
            var checkBox = (CheckBox)sender;
            var friend = (SteamFriend)checkBox.DataContext;

            Debug.WriteLine("Added {0} to SelectedFriends", friend.Name);
            SelectedFriends.Add(friend);

            DisplaySelectedUsers();
        }

        private void DisplaySelectedUsers()
        {
            FriendNameDisplay.Text = string.Empty;
            foreach (var friend in SelectedFriends)
            {
                FriendNameDisplay.Text += $"Name: {friend.Name}\nId: {friend.SteamId}\n";
            }
        }
    }
}
