using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using ApriSiSteam.Models;
using ApriSiSteam.Repositories;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Steamworks;
using Steamworks.Ugc;

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

            UpdateCategories(false);
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
            GamesInCommonDisplay.Text = string.Empty;

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
                        if(game.AppCategories.Contains("Online Co-op"))
                                GamesInCommonDisplay.AppendText($"Game: {clientGame.Value}\n");

            CheckGamesButton.IsEnabled = true;
            CheckGamesButton.Content = "Check Games";
            CheckGamesButton.Foreground = Brushes.White;
        }
        private async void LoadingData(CancellationToken token)
        {
            LoadingPanel.Visibility = Visibility.Visible;

            try
            {
                while (!token.IsCancellationRequested)
                {
                    LoadingDisplay.Text = "Loading";
                    await Task.Delay(500, token);
                    LoadingDisplay.Text = "Loading.";
                    await Task.Delay(500, token);
                    LoadingDisplay.Text = "Loading..";
                    await Task.Delay(500, token);
                    LoadingDisplay.Text = "Loading...";
                    await Task.Delay(500, token);
                }
            }
            catch (TaskCanceledException)
            {
                LoadingPanel.Visibility = Visibility.Hidden;
            }
        }


        string[] categories = { "co-op", "multi-player", "online co-op" };
        private async void UpdateCategories(bool forceUpdate)
        {
            if (!File.Exists("Games.Json") || forceUpdate)
            {
                //CancellationTokenSource tokenSource = new CancellationTokenSource();
                //Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { LoadingData(tokenSource.Token); }));

                var games = await GetOwnedGamesAsync();

                File.WriteAllText(@"Games.json", JsonConvert.SerializeObject(games));
                //tokenSource.Cancel();
            }

            // Let it snow, let it snow let, let it snow
            /*if (!File.Exists("Games.json") || forceUpdate)
            {
                CancellationTokenSource tokenSource = new CancellationTokenSource();
                Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { LoadingData(tokenSource.Token); }));

                using var client = new HttpClient();
                Dictionary<string, List<AppCategory>> Games = new();
                foreach (var game in OwnedGames)
                {
                    var response = await client.GetFromJsonAsync<Dictionary<int, RootApp>>($"https://store.steampowered.com/api/appdetails?appids={game.Appid}&filters=categories");
                    var gameInfo = response![(int)game.Appid];

                    if (gameInfo.Success is true)
                        foreach (var selectedCategory in categories)
                            foreach (var category in gameInfo.Data!.Categories!)
                                if (category.Description!.ToLower() == selectedCategory && !Games.ContainsKey(game.Appid.ToString()))
                                    Games.Add(game.Appid.ToString(), gameInfo.Data.Categories);
                }

                File.WriteAllText(@"Games.json", JsonConvert.SerializeObject(Games));

                tokenSource.Cancel();
            }*/
        }

        public async Task<IEnumerable<SteamApp>> GetOwnedGamesAsync()
        {
            var ownedGames = new List<SteamApp>();
            foreach (var clientGame in OwnedGames)
            {
                var game = GetSteamApp((int)clientGame.Appid);

                if (game is not null)
                    ownedGames.Add(game);
            }

            return ownedGames;
        }

        public int loadedGames = 0;
        public SteamApp GetSteamApp(int appid)
        {
            try
            {
                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = web.Load($"https://store.steampowered.com/app/{appid}");

                var Categories = doc.DocumentNode.SelectNodes("//a[@class='game_area_details_specs_ctn']");
                var image = doc.DocumentNode.SelectSingleNode("//img[@class='game_header_image_full']");
                var Desc = doc.DocumentNode.SelectSingleNode("//div[@class='game_description_snippet']");

                var titles = new List<string>();
                if(Categories != null)
                {
                    foreach (var item in Categories)
                    {
                        titles.Add(item.InnerText);                               
                    }

                    var steamApp = new SteamApp
                    {
                        Appid = appid,
                        AppCategories = titles,
                        ImgPath = image.Attributes["src"].Value,
                        Description = Desc.InnerText
                    };

                    loadedGames++;
                    Debug.WriteLine(appid + $" - {loadedGames}/{OwnedGames.Count}");
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
            


            /*using var client = new HttpClient();

            try
            {
                var response = await client.GetFromJsonAsync<Dictionary<int, RootApp>>($"https://store.steampowered.com/api/appdetails?appids={appid}&filters=categories").ConfigureAwait(false);
                if (response[appid].Data == null)
                    return null;
                var steamApp = new SteamApp
                {
                    Appid = appid,
                    AppCategories = response[appid].Data.Categories
                };

                return steamApp;
            }
            catch(Exception ex)
            {
                Debug.Write(ex);
                return null;
            }
            Thread.Sleep(750); */
        }

        private void ExitButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MinimizeButton_OnClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
    }
}

public class SteamApp
{
    public int Appid { get; set; }
    public List<string>? AppCategories { get; set; }
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