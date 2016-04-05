namespace Ph.WinRtFileHelper.Sample
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Windows.Storage;
    using Windows.UI.Popups;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Navigation;

    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        // File exixts = true
        private async void btnFileExistsTrue_Click(object sender, RoutedEventArgs e)
        {
            bool result = await FileHelper.FileExistsAsync(this.FileDemoOneTxtPath);

            await new MessageDialog(string.Format("Result: {0}", result)).ShowAsync();
        }

        // File exists = false
        private async void btnFileExistsFalse_Click(object sender, RoutedEventArgs e)
        {
            string filePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "DemoFiles", "fileNotHere.txt");
            bool result = await FileHelper.FileExistsAsync(filePath);

            await new MessageDialog(string.Format("Result: {0}", result)).ShowAsync();
        }

        // Write content to the file
        private async void btnWrite_Click(object sender, RoutedEventArgs e)
        {
            StorageFile file = await StorageFile.GetFileFromPathAsync(this.FileDemoOneTxtPath);
            await FileHelper.WriteToFileAsync(file, "<file>" + DateTime.Now.ToString() + "</file>");

            await new MessageDialog(string.Format("Write ok")).ShowAsync();
        }

        // Read content from the file
        private async void btnRead_Click(object sender, RoutedEventArgs e)
        {
            string content = await FileHelper.GetContentFromFileAsync(this.FileDemoOneTxtPath);

            await new MessageDialog(string.Format("Result: {0}", content)).ShowAsync();
        }

        // Zip the file one
        private async void btnZip_Click(object sender, RoutedEventArgs e)
        {
            StorageFile fileToZip = await GetFileDemoOneTxt();
            StorageFolder outputFolder = await GetFolderDemoZip();

            await ZipHelper.ZipFileAsync(fileToZip, outputFolder);

            await new MessageDialog(string.Format("File zipped to {0}", outputFolder.Path)).ShowAsync();
        }

        // Unzip archive
        private async void btnUnzip_Click(object sender, RoutedEventArgs e)
        {
            string filePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "DemoFiles", "Zip", "1.zip");

            if (await FileHelper.FileExistsAsync(filePath))
            {
                StorageFile fileToUnzip = await StorageFile.GetFileFromPathAsync(filePath);
                StorageFolder zipFolder = await GetFolderDemoZip();

                await ZipHelper.UnZipFileAsync(fileToUnzip, zipFolder);

                await new MessageDialog(string.Format("File unzipped to {0}", zipFolder.Path)).ShowAsync();
            }
            else
            {
                await new MessageDialog("! Please zip the file before !").ShowAsync();
            }
        }

        // Enumerate Files
        private async void btnEnumerateFiles_Click(object sender, RoutedEventArgs e)
        {
            IReadOnlyList<StorageFile> files = await FileHelper.EnumerateFilesAsync(await GetFolderDemo());

            await new MessageDialog(string.Format("Files: \r\n {0}", string.Join("\r\n", files.Select(f => "- " + f.Name)))).ShowAsync();
        }

        // Enumerate folders
        private async void btnSubfolders_Click(object sender, RoutedEventArgs e)
        {
            IReadOnlyList<StorageFolder> folders = await FileHelper.GetSubFoldersAsync(await GetFolderDemo());

            await new MessageDialog(string.Format("Folders: \r\n {0}", string.Join("\r\n", folders.Select(f => "- " + f.Name)))).ShowAsync();
        }

        private async void btnInit_Click(object sender, RoutedEventArgs e)
        {
            App.InitDemoFiles(true);
            await new MessageDialog("Demo files successfully cleaned").ShowAsync();
        }

        private string FileDemoOneTxtPath
        {
            get
            {
                return Path.Combine(ApplicationData.Current.LocalFolder.Path, "DemoFiles", "1.txt");
            }
        }

        private async Task<StorageFolder> GetFolderDemo()
        {
            string folderPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "DemoFiles");
            return await StorageFolder.GetFolderFromPathAsync(folderPath);
        }

        private async Task<StorageFolder> GetFolderDemoZip()
        {
            string folderPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "DemoFiles", "Zip");
            return await StorageFolder.GetFolderFromPathAsync(folderPath);
        }

        private async Task<StorageFile> GetFileDemoOneTxt()
        {
            return await StorageFile.GetFileFromPathAsync(FileDemoOneTxtPath);
        }
    }
}
