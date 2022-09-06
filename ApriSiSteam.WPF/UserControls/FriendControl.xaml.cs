using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ApriSiSteam.BL.Models;

namespace ApriSiSteam.WPF.UserControls
{
    /// <summary>
    /// Interaction logic for FriendControl.xaml
    /// </summary>
    public partial class FriendControl : UserControl
    {
        public string SteamId;
        public FriendControl(string name, string avatar, string steamId)
        {
            InitializeComponent();
            
            SteamId = steamId;
            NameDisplay.Text = name;
            ImageDisplay.Source = new BitmapImage(new Uri(avatar));

        }
    }
}
