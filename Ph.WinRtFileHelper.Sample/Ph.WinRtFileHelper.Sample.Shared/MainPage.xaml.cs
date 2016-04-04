using System;
using System.IO;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Ph.WinRtFileHelper.Sample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        private async void btnFileExistsTrue_Click(object sender, RoutedEventArgs e)
        {
            string filePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "DemoFiles", "1.txt");
            bool result = await FileHelper.FileExistsAsync(filePath);
            await new MessageDialog(string.Format("Result: {0}", result)).ShowAsync();
        }

        private async void btnFileExistsFalse_Click(object sender, RoutedEventArgs e)
        {
            string filePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "DemoFiles", "fileNotHere.txt");
            bool result = await FileHelper.FileExistsAsync(filePath);
            await new MessageDialog(string.Format("Result: {0}", result)).ShowAsync();
        }

        private async void btnWrite_Click(object sender, RoutedEventArgs e)
        {
            string filePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "DemoFiles", "1.txt");
            StorageFile file = await StorageFile.GetFileFromPathAsync(filePath);
            await FileHelper.WriteToFileAsync(file, "<file>" + DateTime.Now.ToString() + "</file>");

            await new MessageDialog(string.Format("Write ok")).ShowAsync();
        }

        private async void btnRead_Click(object sender, RoutedEventArgs e)
        {
            string filePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "DemoFiles", "1.txt");
            string content = await FileHelper.GetContentFromFile(filePath);

            await new MessageDialog(string.Format("Result: {0}", content)).ShowAsync();
        }
    }
}
