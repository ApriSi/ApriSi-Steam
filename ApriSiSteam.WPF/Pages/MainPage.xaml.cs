using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private List<string> tagStrings;

        public MainPage()
        {
            InitializeComponent();
            GamesCountTextBlock.Text = "Games: " + SteamAppRepository.ReadOwnedGames()!.Count; 
            GameFrame.Navigate(gamePage);
        }

        private void ContentControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var gameControl = sender as GameControl;
            gamePage.SetData(gameControl.GameName, gameControl.GameImage.Source, gameControl.GameID);
        }


        private void OnGamesLoaded(object sender, RoutedEventArgs e)
        {
            var games = SteamAppRepository.ReadOwnedGames();
            foreach (var gameControl in games)
            {
                var game = new GameControl(gameControl.name!, $"https://cdn.cloudflare.steamstatic.com/steam/apps/{gameControl.appid}/header.jpg", gameControl.appid.ToString(), gameControl.tags!);
                GamesItemControl.Items.Add(game);
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
                checkBox.Checked += CheckBoxOnChecked;
                checkBox.Unchecked += CheckBoxOnUnchecked;
                SteamTagsList.Items.Add(checkBox);
            }
        }

        private void CheckBoxOnUnchecked(object sender, RoutedEventArgs e)
        {
            var checkerBox = sender as CheckBox;
            tagStrings.Remove(checkerBox!.Content.ToString()!);
            var searchThread = new Thread(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    SortGamesControls(GamesItemControl);
                });
            });
            searchThread.Start();
        }

        private void CheckBoxOnChecked(object sender, RoutedEventArgs e)
        {
            if(sender is CheckBox checkBox)
                tagStrings.Add((string)checkBox.Content);

            var searchThread = new Thread(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    SortGamesControls(GamesItemControl);
                });
            });
            searchThread.Start();
        }

        public void SortGamesControls(ItemsControl itemsControl)
        {
            foreach (var item in itemsControl.Items)
            {

                if (item is GameControl control)
                {
                    if (!control!.GameNameText.Text.ToLower().Contains(GamesSearchBox.Text.ToLower())) {
                        control.Visibility = Visibility.Hidden;
                        control.Height = 0;
                    }else
                    {
                        control.Visibility = Visibility.Visible;
                        control.Height = 20;
                    }

                }
            }
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
                Dispatcher.Invoke(() =>
                {
                    SortGamesControls(GamesItemControl);
                });
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
