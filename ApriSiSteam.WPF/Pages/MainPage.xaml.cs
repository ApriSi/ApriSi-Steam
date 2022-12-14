using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;
using ApriSiSteam.BL;
using ApriSiSteam.BL.Repositories;
using ApriSiSteam.WPF.UserControls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ApriSiSteam.WPF.Pages
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        private GamePage gamePage = new GamePage();
        private List<string?> tagStrings = new();

        public MainPage()
        {
            InitializeComponent();
            GamesCountTextBlock.Text = "Games: " + SteamAppRepository.ReadOwnedGames()!.Count; 
            GameFrame.Navigate(gamePage);
        }

        private void ContentControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var gameControl = sender as GameControl;
            gamePage.SetData(gameControl!.GameName, gameControl.GameImage.Source, gameControl.GameID);
        }


        private void OnGamesLoaded(object sender, RoutedEventArgs e)
        {
            var loadGames = new Thread(() =>
            {
                var games = SteamAppRepository.ReadOwnedGames();
                var sortedGames = games!.OrderBy(o => o.name).ToList();

                foreach (var gameControl in sortedGames)
                {
                    Dispatcher.Invoke(() =>
                    {
                        var game = new GameControl(gameControl.name!, $"https://cdn.cloudflare.steamstatic.com/steam/apps/{gameControl.appid}/header.jpg", gameControl.appid.ToString(), gameControl.tags!);
                        GamesItemControl.Items.Add(game);
                    });
                }

            });
            loadGames.Start();
        }

        private void OnSteamTagsLoaded(object sender, RoutedEventArgs e)
        {
            var tags = SteamAppRepository.GetTags();

            foreach (var checkBox in tags.Select(tag => new CheckBox()
                     {
                         Content = tag,
                         DataContext = tag,
                         Foreground = Brushes.White
                     }))
            {
                checkBox.Checked += CheckBoxOnChecked;
                checkBox.Unchecked += CheckBoxOnUnchecked;
                SteamTagsList.Items.Add(checkBox);
            }
        }

        private void CheckBoxOnUnchecked(object sender, RoutedEventArgs e)
        {
            var checkerBox = sender as CheckBox;
            tagStrings.Remove(checkerBox!.DataContext.ToString());
            var searchThread = new Thread(() =>
            {
                SortGamesControls();
            });
            searchThread.Start();
            Debug.WriteLine(tagStrings.Count);
        }

        private void CheckBoxOnChecked(object sender, RoutedEventArgs e)
        {
            if(sender is CheckBox checkBox)
                tagStrings.Add(checkBox.DataContext.ToString());

            var searchThread = new Thread(() =>
            {
                SortGamesControls();
            });
            searchThread.Start();
            Debug.WriteLine(tagStrings.Count);
        }

        private static List<string> GetSortedFriendGames()
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow!.FriendImageList.Items.Count < 0) return new List<string>();
            
            var friendsAppIds = new List<string>();
            foreach (var selectedFriend in mainWindow!.FriendImageList.Items)
            {
                var friend = selectedFriend as Image;

                var games = SteamFriendRepository.GetFriendGames(friend!.DataContext.ToString());
                var currentList = new List<string>();

                foreach (var appId in games)
                {
                    if (friendsAppIds.Contains(appId) || friendsAppIds.Count <= 0)
                        currentList.Add(appId);
                }

                friendsAppIds = currentList; 
            }

            return friendsAppIds;
        }

        public void SortGamesControls()
        {
            Dispatcher.Invoke(() =>
            {
                var friendAppIds = GetSortedFriendGames();
                foreach (var item in GamesItemControl.Items)
                {
                    var hasCategories = true;
                    if (item is GameControl control && tagStrings.Count > 0)
                    {
                        var gameTags = new List<string>();
                        if (control.Category.ToString() != "[]")
                        {
                            foreach (var tag in (control.Category as JObject)!)
                                gameTags.Add(tag.Key);

                            hasCategories = tagStrings.Except(gameTags).ToList().Count <= 0;
                        }
                        else
                        {
                            hasCategories = false;
                        }
                    }

                    if (item is not GameControl gameControl) continue;
                    if (!gameControl!.GameNameText.Text.ToLower().Contains(GamesSearchBox.Text.ToLower()) || !hasCategories || (!friendAppIds.Contains(gameControl.GameID) && friendAppIds.Count > 0))
                    {
                        Dispatcher.Invoke(() =>
                        {
                            gameControl.Visibility = Visibility.Hidden;
                            gameControl.Height = 0;
                        });
                    }
                    else
                    {
                        Dispatcher.Invoke(() =>
                        {
                            gameControl.Visibility = Visibility.Visible;
                            gameControl.Height = 20;
                        });
                    }
                }       
            });
        }

        private void RefreshGamesButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in GamesItemControl.Items)
            {
                var gameItem = item as GameControl;
                gameItem!.SetData();
            }
        }

        private void GamesSearchBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            SearchboxSearchText.Visibility = Visibility.Hidden;
            if (GamesSearchBox.Text == "")
                SearchboxSearchText.Visibility = Visibility.Visible;

            var searchThread = new Thread(() =>
            {
                Dispatcher.Invoke(SortGamesControls);
            });
            searchThread.Start();
        }

        private void TagsSearchBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TagBoxSearchText.Visibility = Visibility.Hidden;
            if (TagsSearchBox.Text == "")
                TagBoxSearchText.Visibility = Visibility.Visible;

            var searchThread = new Thread(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    foreach (var item in SteamTagsList.Items)
                    {
                        if (item is CheckBox checkbox)
                            if (!checkbox.Content.ToString()!.ToLower().Contains(TagsSearchBox.Text.ToLower()))
                            {
                                checkbox.Visibility = Visibility.Hidden;
                                checkbox.Height = 0;
                            }
                            else
                            {
                                checkbox.Visibility = Visibility.Visible;
                                checkbox.Height = 20;
                            }
                    }
                });
            });
            searchThread.Start();
        }
    }
}
