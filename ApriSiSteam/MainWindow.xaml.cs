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
            var clientUserGames = await OwnedGamesRepository.GetOwnedGamesAsync(SteamClient.SteamId);

            if (clientUserGames.Games is null) return;
            foreach (var game in clientUserGames.Games.ToList())
                OwnedGames.Add(game);

            Friends = SteamFriends.GetFriends().ToList();

            var UpdateCategoryThread = new Thread(() => UpdateCategories(false));
            UpdateCategoryThread.Start();

            //await UpdateCategories(false);
            LoadProfileInformation();
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
                var friendOwnedGames = await OwnedGamesRepository.GetOwnedGamesAsync(friend.Id);
                if (friendOwnedGames.Games is null) return;
                friendGames.Add(friend.Id, new Dictionary<int, string>());
                var games = friendOwnedGames.Games.ToList();
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
                        if(game.AppCategories.Contains("Online Co-op") || game.UserDefinedCategories.Contains("Co-op"))
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
        private async void UpdateCategories(bool forceUpdate)
        {
            Dispatcher.Invoke(() =>
            {
                LoadingPanel.Visibility = Visibility.Visible;
            });
            if (!File.Exists("Games.Json") || forceUpdate)
            {
                var games = await GetOwnedGamesAsync();

                File.WriteAllText(@"Games.json", JsonConvert.SerializeObject(games));
            }

            Dispatcher.Invoke(() =>
            {
                LoadingPanel.Visibility = Visibility.Hidden;
            });
        }

        public async Task<IEnumerable<SteamApp>> GetOwnedGamesAsync()
        {
            var ownedGames = new List<SteamApp>();
            
            foreach (var clientGame in OwnedGames)
            {
                var game = await GetSteamApp((int)clientGame.Appid);

                if (game is not null)
                    ownedGames.Add(game);
            }

            return ownedGames;
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
            Close();
        }

        private void MinimizeButton_OnClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }


        private async void LoadProfileInformation()
        {
            UsernameDisplay.Text = SteamClient.Name;
            GameCountDisplay.Text = "Games Owned: " + OwnedGames.Count;

            var userSummaries = await OwnedGamesRepository.GetUserSummaries(SteamClient.SteamId);
            ProfileImage.Source = new BitmapImage(new Uri(userSummaries.Avatarfull));
        }
    }
}

public class SteamApp
{
    public int Appid { get; set; }
    public List<string>? AppCategories { get; set; }
    public List<string>? UserDefinedCategories { get; set; }
    public string ImgPath { get; set; }
    public string Description { get; set; }
}

public class RootApp
{
    public bool? Success { get; set; }
    public AppDetails? Data { get; set; }
}

public class AppDetails
{
    public List<AppCategory>? Categories { get; set; }
}

public class AppCategory
{
    public int? Id { get; set; }
    public string? Description { get; set; }
}