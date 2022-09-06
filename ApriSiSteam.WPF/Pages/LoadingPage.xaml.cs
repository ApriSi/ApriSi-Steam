using System;
using System.CodeDom.Compiler;
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
using ApriSiSteam.BL;
using ApriSiSteam.BL.Models;
using ApriSiSteam.BL.Repositories;

namespace ApriSiSteam.WPF.Pages
{
    /// <summary>
    /// Interaction logic for LoadingPage.xaml
    /// </summary>
    public partial class LoadingPage : Page
    {
        public LoadingPage()
        {
            InitializeComponent();
        }

        public async void Loading()
        {
            SteamAppRepository.CreateOwnedGamesJson(Steam.GetClientSteamId());
            while (!File.Exists("ClientGames.json"))
            {
                Dispatcher.Invoke(() =>
                {
                    LoadingDisplay.Text = $"{SteamAppRepository.CurrentLoadedGames}/{SteamAppRepository.CurrentLoadedGames}";
                });
                Thread.Sleep(100);
            }

            Dispatcher.Invoke(() =>
            {
                var window = Window.GetWindow(App.Current.MainWindow) as MainWindow;
                window!.SetPage(new MainPage());
            });

            await Task.CompletedTask;
        }

        private void LoadingPageLoaded(object sender, RoutedEventArgs e)
        {
            var loadingThread = new Thread(Loading);
            loadingThread.Start();
        }
    }
}
