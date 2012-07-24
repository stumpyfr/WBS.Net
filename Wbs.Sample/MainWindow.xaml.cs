using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace Wbs.Sample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string consumerKey = "8232ce439c8236c240156d7955423239cc44621ed3668c03fd17b1f30";
        private const string consumerSecret = "2f7401d0c5707fb3cd951bc07c7c4d9c1657d8ce284fc47e669d633b0552479";
        private const string token = "c99115c176cdb3154f919c24d3cc7bd694c36301521e9c7ddd8e421c329454";
        private const string tokenSecret = "e6c9e10997c7888c5e6c485b6e6079b88ffeedf8d95cbff7194c0055c46";
        private const int userId = 815991;

        WBS.Net.WBS wbs = new WBS.Net.WBS(
            consumerKey,
            consumerSecret,
            token, // optionnal
            tokenSecret, // optionnal
            userId); // optionnal 
        // if you dont see these optionnal parameter, you will need to manage the oauth validation (see below)

        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // already logued so we can directly get Measures
            var list = await wbs.GetMeasures(DateTime.Now.AddYears(-1), DateTime.Now);

            // if we dont have token, tokenSecret and userId, we need to get a oauth token
            //var url = await wbs.Connect();
            //this.WebBrowser.LoadCompleted += WebBrowser_LoadCompleted;
            //this.WebBrowser.Source = new Uri(url);
        }

        void WebBrowser_LoadCompleted(object sender, NavigationEventArgs e)
        {
            if (e.Uri.OriginalString.Contains("oauth_verifier"))
            {
                var data = e.Uri.OriginalString.Split(new char[] { '=', '&' });
                var verifier = data[5];
                var userId = data[1];

                wbs.Valid(verifier, int.Parse(userId));
            }
        }
    }
}
