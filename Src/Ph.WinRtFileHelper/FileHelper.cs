namespace Ph.WinRtFileHelper
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml.Serialization;
    using Windows.Storage;
    using Windows.Storage.Search;
    using Windows.Storage.Streams;

    public class FileHelper
    {
        /// <summary>
        /// Check if the file exists.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        public static async Task<bool> FileExistsAsync(string filePath)
        {
            filePath = filePath.Replace("/", @"\");

            bool fileExists;

            try
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(filePath).AsTask().ConfigureAwait(false);
                fileExists = file != null;
            }
            catch (Exception)
            {
                fileExists = false;
            }

            return fileExists;
        }

        /// <summary>
        /// Check if the folder exists.
        /// </summary>
        /// <param name="pathFolder">The folder path.</param>
        /// <returns></returns>
        public static async Task<bool> FolderExistsAsync(string folderPath)
        {
            try
            {
                var folder = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(folderPath);
                return folder != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the file stream. (Don't forget to dispose after using)
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <returns></returns>
        public static async Task<Stream> GetFileStreamAsync(string filePath)
        {
            string pathFile = Path.GetDirectoryName(filePath);
            string fileName = Path.GetFileName(filePath);
            var folder = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(pathFile).AsTask().ConfigureAwait(false);
            var file = await folder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists).AsTask().ConfigureAwait(false);

            IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite).AsTask().ConfigureAwait(false);

            return stream.AsStreamForWrite();
        }

        /// <summary>
        /// Gets the file stream from assets folder.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>A stream</returns>
        public static async Task<Stream> GetFileStreamFromAssetsAsync(string path)
        {
            if (!path.StartsWith("ms-appx:///Assets/"))
            {
                path = "ms-appx:///Assets/" + path.Replace("Assets\\", string.Empty).Replace(@"\", "/");
            }

            var uri = new Uri(path);

            var file = await StorageFile.GetFileFromApplicationUriAsync(uri).AsTask().ConfigureAwait(false);
            return await file.OpenStreamForReadAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Gets content from file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="isAssets">Indicates if the file is a local asset</param>
        /// <returns>A stream</returns>
        public static async Task<string> GetContentFromFile(string filePath, bool isAssets = false)
        {
            string content = string.Empty;

            if (isAssets)
            {
                Stream stream = await GetFileStreamFromAssetsAsync(filePath).ConfigureAwait(false);
                using (StreamReader streamReader = new StreamReader(stream))
                {
                    content = streamReader.ReadToEnd();
                }
            }
            else
            {
                Stream stream = await GetFileStreamAsync(filePath).ConfigureAwait(false);
                using (StreamReader streamReader = new StreamReader(stream))
                {
                    content = streamReader.ReadToEnd();
                }
            }

            return content;
        }

        /// <summary>
        /// Gets the file bytes.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <returns>A bytes array</returns>
        public static async Task<byte[]> GetFileBytesAsync(string path)
        {
            string pathFile = Path.GetDirectoryName(path);
            string fileName = Path.GetFileName(path);

            var folder = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(pathFile).AsTask().ConfigureAwait(false);
            var file = await folder.GetFileAsync(fileName).AsTask().ConfigureAwait(false);

            return await ConvertStorageFileToBase64String(file).ConfigureAwait(false);
        }

        /// <summary>
        /// Converts the storage file to a base64 string.
        /// </summary>
        /// <param name="File">The storage file.</param>
        /// <returns>A bytes array</returns>
        public static async Task<byte[]> ConvertStorageFileToBase64String(StorageFile File)
        {
            var stream = await File.OpenReadAsync().AsTask().ConfigureAwait(false);

            using (var dataReader = new DataReader(stream))
            {
                var bytes = new byte[stream.Size];
                await dataReader.LoadAsync((uint)stream.Size);
                dataReader.ReadBytes(bytes);

                return bytes;
            }
        }

        /// <summary>
        /// Enumerates the files.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The path of each files</returns>
        public async static Task<List<string>> EnumerateFiles(string parentFolderPath)
        {
            List<string> folders = new List<string>();

            var mainFolder = await StorageFolder.GetFolderFromPathAsync(parentFolderPath).AsTask().ConfigureAwait(false);
            var subFolders = await mainFolder.GetFilesAsync().AsTask().ConfigureAwait(false);

            folders = subFolders.Select(f => f.Path).ToList();

            return folders;
        }

        /// <summary>
        /// Gets the sub folders path.
        /// </summary>
        /// <param name="parentFolderPath">The parent folder path.</param>
        /// <returns></returns>
        public async static Task<List<string>> GetSubFoldersPath(string parentFolderPath)
        {
            List<string> folders = new List<string>();

            var mainFolder = await StorageFolder.GetFolderFromPathAsync(parentFolderPath).AsTask().ConfigureAwait(false);
            var subFolders = await mainFolder.GetFoldersAsync().AsTask().ConfigureAwait(false);

            folders = subFolders.Select(f => f.Path).ToList();

            return folders;
        }

        /// <summary>
        /// Gets the sub folders names.
        /// </summary>
        /// <param name="parentFolderPath">The parent folder path.</param>
        /// <returns></returns>
        public async static Task<List<string>> GetSubFoldersNames(string parentFolderPath)
        {
            List<string> folders = new List<string>();

            var mainFolder = await StorageFolder.GetFolderFromPathAsync(parentFolderPath).AsTask().ConfigureAwait(false);
            var subFolders = await mainFolder.GetFoldersAsync().AsTask().ConfigureAwait(false);

            folders = subFolders.Select(f => f.Name).ToList();

            return folders;
        }

        /// <summary>
        /// Deserialises the XML.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xmlFile">The XML file path.</param>
        /// <param name="isAssets">Indicates if the file is a local asset</param>
        /// <returns></returns>
        public static T DeserialiseXml<T>(string xmlFile, bool isAssets = false)
        {
            T result;
            XmlSerializer xs = new XmlSerializer(typeof(T));
            Stream stream = null;

            if (isAssets)
            {
                stream = GetFileStreamFromAssetsAsync(xmlFile).Result;
            }
            else
            {
                stream = GetFileStreamAsync(xmlFile).Result;
            }

            result = (T)xs.Deserialize(stream);
            stream.Dispose();

            return result;
        }

        /// <summary>
        /// Writes to file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        public static async Task WriteToFileAsync(StorageFile file, string content)
        {
            using (Stream stream = await file.OpenStreamForWriteAsync())
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(content);
                }
            }
        }
    }
}
