namespace Ph.WinRtFileHelper
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Threading.Tasks;
    using Windows.Storage;

    public class ZipHelper
    {
        /// <summary>
        /// Unzip a file.
        /// </summary>
        /// <param name="destinationFolder">The destination folder.</param>
        /// <param name="zipFile">The zip file.</param>
        /// <returns></returns>
        public async static Task UnZipFileAsync(StorageFolder destinationFolder, StorageFile zipFile)
        {
            using (var zipStream = await zipFile.OpenStreamForReadAsync())
            {
                using (MemoryStream zipMemoryStream = new MemoryStream((int)zipStream.Length))
                {
                    await zipStream.CopyToAsync(zipMemoryStream);

                    using (var archive = new ZipArchive(zipMemoryStream, ZipArchiveMode.Read))
                    {
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {

                            if (string.IsNullOrEmpty(entry.Name))
                            {
                                // Folder
                                await CreateRecursiveFolder(destinationFolder, entry);
                            }
                            else
                            {
                                // File
                                await ExtractFile(destinationFolder, entry);
                            }
                        }
                    }
                }
            }
        }

        private async static Task CreateRecursiveFolder(StorageFolder folder, ZipArchiveEntry entry)
        {
            var steps = entry.FullName.Split('/').ToList();

            steps.RemoveAt(steps.Count() - 1);

            foreach (var i in steps)
            {
                await folder.CreateFolderAsync(i, CreationCollisionOption.OpenIfExists);

                folder = await folder.GetFolderAsync(i);
            }
        }

        private async static Task ExtractFile(StorageFolder folder, ZipArchiveEntry entry)
        {
            var steps = entry.FullName.Split('/').ToList();

            steps.RemoveAt(steps.Count() - 1);

            foreach (var i in steps)
            {
                folder = await folder.GetFolderAsync(i);
            }

            using (Stream fileData = entry.Open())
            {
                StorageFile outputFile = await folder.CreateFileAsync(entry.Name, CreationCollisionOption.ReplaceExisting);

                using (Stream outputFileStream = await outputFile.OpenStreamForWriteAsync())
                {
                    await fileData.CopyToAsync(outputFileStream);
                    await outputFileStream.FlushAsync();
                }
            }
        }
    }
}
