using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ApriSiSteam.Models;
using ApriSiSteam.Repositories;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Steamworks;
using Steamworks.Ugc;
using static System.Net.Mime.MediaTypeNames;
using Brushes = System.Windows.Media.Brushes;

namespace ApriSiSteam
{
    /// Ooooh we're half way there, OOOOOOH OOOH SQUIDWARD ON A CHAIR.. TAKE MY HAND AND WE'LL MAKE IT I SWEAR OOOOOOOOH SQUIDWARD ON A CHAIR
    /// SQUIIIIDWAARD OOON A CHAAIIIR
    public partial class MainWindow : Window
    {
        public static List<Game> OwnedGames = new();
        public List<SteamApp> SteamApps = new();

        public static List<Friend>? Friends;
        public static List<Friend> SelectedFriends = new List<Friend>();

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

        private async void WindowLoaded(object sender, RoutedEventArgs e)
        {
            LoadSteamData();
        }

        private async void CheckGamesButton_Click(object sender, RoutedEventArgs e)
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
                if (!clientGames.ContainsKey((int)clientGame.Appid))
                    clientGames.Add((int)clientGame.Appid!, clientGame.Name!);

            foreach (var friend in SelectedFriends)
            {
                var friendOwnedGames = OwnedGamesRepository.GetOwnedGames(friend.Id);
                if (friendOwnedGames is null) return;
                friendGames.Add(friend.Id, new Dictionary<int, string>());
                var games = friendOwnedGames;
                foreach (var game in games)
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
            List<SteamApp> Games = JsonConvert.DeserializeObject<List<SteamApp>>(json);

            foreach (var clientGame in clientGames)
                foreach (var game in Games)
                    if (game.Appid == clientGame.Key)
                        if (game.AppCategories.Contains("Online Co-op") || game.UserDefinedCategories.Contains("Co-op"))
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
        private async void UpdateCategories(bool forceUpdate, int startCount, int endCount)
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
            var ownedGames = new List<SteamApp>();

            for (int i = startCount; i < endCount; i++)
            {
                var game = await GetSteamApp((int)OwnedGames[i].Appid);

                if (game is not null)
                    SteamApps.Add(game);
            }
        }

        public int loadedGames = 0;
        public async Task<SteamApp> GetSteamApp(int appid)
        {
            try
            {
                HtmlWeb web = new HtmlWeb();
                using var client = new HttpClient();

                web.UseCookies = true;

                Uri uri = new Uri($"https://store.steampowered.com/app/{appid}");

                web.PreRequest += request =>
                {
                    CookieContainer cookieContainer = new CookieContainer();
                    cookieContainer.Add(new Cookie("birthtime", "312850801") { Domain = uri.Host });
                    request.CookieContainer = cookieContainer;
                    return true;
                };

                HtmlDocument doc = web.Load(uri);

                var Categories = doc.DocumentNode.SelectNodes("//a[@class='game_area_details_specs_ctn']");
                var UserDefinedCategories = doc.DocumentNode.SelectNodes("//a[@class='app_tag']");

                var image = doc.DocumentNode.SelectSingleNode("//img[@class='game_header_image_full']");
                var Desc = doc.DocumentNode.SelectSingleNode("//div[@class='game_description_snippet']");

                var ageGateImage = doc.DocumentNode.SelectSingleNode("//div[@class='img_ctn']");

                var category = new List<string>();
                var userDefinedCategory = new List<string>();
                if (Categories != null)
                {
                    foreach (var item in Categories)
                    {
                        category.Add(item.InnerText);
                    }

                    foreach (var item in UserDefinedCategories)
                    {
                        userDefinedCategory.Add(Regex.Replace(item.InnerText, @"\n|\r|\t", string.Empty));
                    }

                    var steamApp = new SteamApp
                    {
                        Appid = appid,
                        AppCategories = category,
                        UserDefinedCategories = userDefinedCategory,
                        ImgPath = image.Attributes["src"].Value,
                    };

                    if (Desc != null)
                        steamApp.Description = Regex.Replace(Desc.InnerText, @"\n|\r|\t", string.Empty);

                    loadedGames++;
                    Debug.WriteLine(appid + $" - {loadedGames}/{OwnedGames.Count}");
                    Dispatcher.Invoke(() =>
                    {
                        LoadingDisplay.Text = $"{loadedGames}/{OwnedGames.Count}";
                    });
                    return steamApp;
                }
                else
                {
                    return null;
                }

            }
            catch
            {
                return null;
            }
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

            HtmlWeb web = new HtmlWeb();
            using var client = new HttpClient();
            web.UserAgent = "Mozilla/5.0 (X11; CrOS x86_64 14816.131.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.0.0 Safari/537.36";

            Uri uri = new Uri($"https://steamdb.info/calculator/{SteamClient.SteamId}/?cc=eu");
            HtmlDocument doc = web.Load(uri);

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

        private void SteamAPIButton_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = "https://steamcommunity.com/dev",
                UseShellExecute = true
            });
        }

        public void LoadSteamData()
        {
            var clientUserGames = OwnedGamesRepository.GetOwnedGames(SteamClient.SteamId);

            if (clientUserGames is null) return;
            foreach (var game in clientUserGames)
                OwnedGames.Add(game);

            Friends = SteamFriends.GetFriends().ToList();

            int devidedCount = (int)clientUserGames.Count! / 2;
            var ThreadGamecount = new List<int>();

            for (int i = 0; i < 2; i++)
            {
                ThreadGamecount.Add(devidedCount * i);
            }
            ThreadGamecount.Add((int)clientUserGames.Count);


            for (int i = 0; i < ThreadGamecount.Count - 1; i++)
            {
                var tis = ThreadGamecount[i];
                var tis2 = ThreadGamecount[i + 1];

                var UpdateCategoryThread = new Thread(() => UpdateCategories(false, tis, tis2));
                UpdateCategoryThread.Start();
            }

            LoadProfileInformation();
        }

        private void CategoryList_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}