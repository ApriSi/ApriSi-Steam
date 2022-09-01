using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ApriSiSteam.Models;
using ApriSiSteam.Repositories;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Steamworks;
using Brushes = System.Windows.Media.Brushes;

namespace ApriSiSteam
{
    public partial class MainWindow : Window
    {
        public static readonly List<Game> OwnedGames = new();
        public readonly List<SteamApp> SteamApps = new();

        public static List<Friend>? Friends;
        public static readonly List<Friend> SelectedFriends = new List<Friend>();

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                SteamClient.Init(480);
            }
            catch
            {
                Close();
            }
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            LoadSteamData();
        }


        private void LoadCategories()
        {
            var json = File.ReadAllText(@"Games.json");
            var games = JsonConvert.DeserializeObject<List<SteamApp>>(json);
            var tags = new List<string>();
            foreach (var game in games)
            {
                foreach (var tag in game.AppCategories!)
                {
                    if (tags.Contains(tag)) continue;
                    tags.Add(tag);
                    Dispatcher.Invoke(() => {
                        var checkbox = new CheckBox();
                        checkbox.Content = tag;
                        checkbox.Foreground = Brushes.White;
                        CategoryList.Items.Add(checkbox);
                    });
                }

            }
        }

        private void CheckGamesButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedFriends.Count <= 0)
                return;

            CheckGamesButton.IsEnabled = false;
            CheckGamesButton.Content = "Loading...";
            CheckGamesButton.Foreground = Brushes.Black;
            GameList.Items.Clear();

            if (OwnedGames is null) return;

            // User ID, List o' games (id, name)
            Dictionary<SteamId, Dictionary<int, string>> friendGames = new();
            Dictionary<int, string> clientGames = new();

            foreach (var clientGame in OwnedGames)
                if (!clientGames.ContainsKey((int)clientGame.Appid!))
                    clientGames.Add((int)clientGame.Appid!, clientGame.Name!);

            foreach (var friend in SelectedFriends)
            {
                var friendOwnedGames = OwnedGamesRepository.GetOwnedGames(friend.Id);

                if (friendOwnedGames is null) return;

                friendGames.Add(friend.Id, new Dictionary<int, string>());

                foreach (var game in friendOwnedGames)
                    foreach (var clientGame in OwnedGames)
                        if (game.Name == clientGame.Name && !friendGames[friend.Id].ContainsKey((int)game.Appid!))
                            friendGames[friend.Id].Add((int)game.Appid!, game.Name!);
            }

            foreach (var friend in SelectedFriends)
            {
                var selectedFriendGames = friendGames[friend.Id];
                foreach (var clientGame in OwnedGames)
                    if (!selectedFriendGames.ContainsKey((int)clientGame.Appid!))
                        clientGames.Remove((int)clientGame.Appid);
            }

            var json = File.ReadAllText(@"Games.json");
            var games = JsonConvert.DeserializeObject<List<SteamApp>>(json);

            foreach (var clientGame in clientGames)
                foreach (var game in games!)
                    if (game.Appid == clientGame.Key)
                        if (game.UserDefinedCategories != null && (game.AppCategories!.Contains("Online Co-op") || game.UserDefinedCategories.Contains("Co-op")))
                        {
                            var steamAppControl = new SteamAppControl();
                            steamAppControl.NameDisplay.Text = clientGame.Value;

                            var bitmapImage = new BitmapImage(new Uri(game.ImgPath));
                            steamAppControl.GameBanner.Source = bitmapImage;

                            GameList.Items.Add(steamAppControl);
                        }


            CheckGamesButton.IsEnabled = true;
            CheckGamesButton.Content = "Check Games";
            CheckGamesButton.Foreground = Brushes.White;
        }

        string[] categories = { "co-op", "multi-player", "online co-op" };
        private void UpdateCategories(bool forceUpdate, int startCount, int endCount)
        {
           
            if (!File.Exists("Games.Json") || forceUpdate)
            {
                Dispatcher.Invoke(() =>
                {
                    LoadingPanel.Visibility = Visibility.Visible;
                });
                GetOwnedGamesAsync(startCount, endCount);

                if (endCount == OwnedGames.Count)
                {
                    File.WriteAllText(@"Games.json", JsonConvert.SerializeObject(SteamApps));
                    Dispatcher.Invoke(() =>
                    {
                        LoadingPanel.Visibility = Visibility.Hidden;
                    });
                }
            }


        }

        public async void GetOwnedGamesAsync(int startCount, int endCount)
        {
            for (var i = startCount; i < endCount; i++)
            {
                var game = await GetSteamApp((int)OwnedGames[i].Appid);

                if (game is not null)
                    SteamApps.Add(game);
            }
        }

        public int LoadedGames = 0;
        public Task<SteamApp> GetSteamApp(int appid)
        {
            try
            {
                var web = new HtmlWeb();
                using var client = new HttpClient();

                web.UseCookies = true;

                var uri = new Uri($"https://store.steampowered.com/app/{appid}");

                web.PreRequest += request =>
                {
                    var cookieContainer = new CookieContainer();
                    cookieContainer.Add(new Cookie("birthtime", "312850801") { Domain = uri.Host });
                    request.CookieContainer = cookieContainer;
                    return true;
                };

                var doc = web.Load(uri);

                var categories = doc.DocumentNode.SelectNodes("//a[@class='game_area_details_specs_ctn']");
                var userDefinedCategories = doc.DocumentNode.SelectNodes("//a[@class='app_tag']");

                var image = doc.DocumentNode.SelectSingleNode("//img[@class='game_header_image_full']");
                var description = doc.DocumentNode.SelectSingleNode("//div[@class='game_description_snippet']");

                var category = new List<string>();
                var userDefinedCategory = new List<string>();
                if (categories != null)
                {
                    category.AddRange(categories.Select(item => item.InnerText));

                    userDefinedCategory.AddRange(userDefinedCategories.Select(item => Regex.Replace(item.InnerText, @"\n|\r|\t", string.Empty)));

                    var steamApp = new SteamApp
                    {
                        Appid = appid,
                        AppCategories = category,
                        UserDefinedCategories = userDefinedCategory,
                        ImgPath = image.Attributes["src"].Value,
                    };

                    if (description != null)
                        steamApp.Description = Regex.Replace(description.InnerText, @"\n|\r|\t", string.Empty);

                    LoadedGames++;
                    Debug.WriteLine(appid + $" - {LoadedGames}/{OwnedGames.Count}");
                    Dispatcher.Invoke(() =>
                    {
                        LoadingDisplay.Text = $"{LoadedGames}/{OwnedGames.Count}";
                    });
                    return Task.FromResult(steamApp);
                }
            }
            catch
            {
                return Task.FromResult<SteamApp>(null);
            }
            return Task.FromResult<SteamApp>(null);
        }

        private void ExitButton_OnClick(object sender, RoutedEventArgs e)
        {
            Environment.Exit(Environment.ExitCode);
            Close();
        }

        private void MinimizeButton_OnClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }


        private void LoadProfileInformation()
        {
            UsernameDisplay.Text = SteamClient.Name;
            GameCountDisplay.Text = "Games Owned: " + OwnedGames.Count;

            var web = new HtmlWeb();
            using var client = new HttpClient();
            web.UserAgent = "Mozilla/5.0 (X11; CrOS x86_64 14816.131.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.0.0 Safari/537.36";

            var uri = new Uri($"https://steamdb.info/calculator/{SteamClient.SteamId}/?cc=eu");
            var doc = web.Load(uri);

            var profileImage = doc.DocumentNode.SelectSingleNode("//img[@class='avatar']");
            ProfileImage.Source = new BitmapImage(new Uri(profileImage.Attributes["src"].Value));
        }

        private void FriendlistLoaded(object sender, RoutedEventArgs e)
        {
            foreach (var friend in SteamFriends.GetFriends())
            {
                var friendCheckBox = new CheckBox()
                {
                    Content = friend.Name,
                    DataContext = friend,
                    Foreground = new SolidColorBrush(Colors.White)
                };

                friendCheckBox.Checked += FriendCheckBox_Checked;
                friendCheckBox.Unchecked += FriendCheckBox_Unchecked;

                FriendList.Items.Add(friendCheckBox);
            }
        }

        private void FriendCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;

            if (checkBox is null) return;
            var friend = (Friend)checkBox.DataContext;
            SelectedFriends.Remove(friend);

            DisplaySelectedUsers();
        }

        private void FriendCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var checkBox = (CheckBox)sender;
            if (SelectedFriends.Count > 10)
            {
                checkBox.IsChecked = false;
                return;
            }

            if (checkBox is null) return;
            var friend = (Friend)checkBox.DataContext;
            SelectedFriends.Add(friend);

            DisplaySelectedUsers();
        }

        private void DisplaySelectedUsers()
        {
            FriendNameDisplay.Text = string.Empty;
            foreach (var friend in SelectedFriends)
            {
                FriendNameDisplay.Text += $"Name: {friend.Name}\nId: {friend.Id}\n";
            }
        }

        private void TopOverlay_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => DragMove();

        public void LoadSteamData()
        {
            var clientUserGames = OwnedGamesRepository.GetOwnedGames(SteamClient.SteamId);

            if (clientUserGames is null) return;
            foreach (var game in clientUserGames)
                OwnedGames.Add(game);

            Friends = SteamFriends.GetFriends().ToList();

            var split = SplitListCount(2, OwnedGames.Count);
            for (var i = 0; i < split.Count - 1; i++)
            {
                var tis = split[i];
                var tis2 = split[i + 1];

                var UpdateCategoryThread = new Thread(() => UpdateCategories(false, tis, tis2));
                UpdateCategoryThread.Start();
            }

            LoadProfileInformation();
        }

        public List<int> SplitListCount(int splitCount, int listCount)
        {
            var split = listCount / splitCount;
            var SplitListsCount = new List<int>();
            for (var i = 0; i < 2; i++)
            {
                SplitListsCount.Add(split * i);
            }
            SplitListsCount.Add((int)listCount);

            return SplitListsCount;
        }

        private void CategoryList_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}