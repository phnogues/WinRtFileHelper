namespace Ph.WinRtFileHelper
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Runtime.InteropServices.WindowsRuntime;
    using System.Threading.Tasks;
    using Windows.Storage;
    using Windows.Storage.Streams;

    public class ZipHelper
    {
        /// <summary>
        /// Create a zip archive.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="destinationFolder">The destination folder.</param>
        /// <param name="zipName">Name of the zip.</param>
        /// <param name="collisionOption">The collision option.</param>
        /// <returns></returns>
        public async static Task ZipFileAsync(StorageFile file, StorageFolder destinationFolder, string zipName = "", CreationCollisionOption collisionOption = CreationCollisionOption.ReplaceExisting)
        {
            using (MemoryStream zipMemoryStream = new MemoryStream())
            {
                using (ZipArchive zipArchive = new ZipArchive(zipMemoryStream, ZipArchiveMode.Create))
                {
                    byte[] buffer = WindowsRuntimeBufferExtensions.ToArray(await FileIO.ReadBufferAsync(file));
                    ZipArchiveEntry entry = zipArchive.CreateEntry(file.Name);
                    using (Stream entryStream = entry.Open())
                    {
                        await entryStream.WriteAsync(buffer, 0, buffer.Length);
                    }
                }

                // Created new file to store compressed files
                var compressedFileName = (string.IsNullOrEmpty(zipName) ? file.Name.Replace(file.FileType, "") : zipName) + ".zip";
                StorageFile zipFile = await destinationFolder.CreateFileAsync(compressedFileName, collisionOption);
                using (IRandomAccessStream zipStream = await zipFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    // Write compressed data from memory to file
                    using (Stream outstream = zipStream.AsStreamForWrite())
                    {
                        byte[] buffer = zipMemoryStream.ToArray();
                        outstream.Write(buffer, 0, buffer.Length);
                        outstream.Flush();
                    }
                }
            }
        }

        /// <summary>
        /// Unzip a file.
        /// </summary>
        /// <param name="destinationFolder">The destination folder.</param>
        /// <param name="zipFile">The zip file.</param>
        /// <returns></returns>
        public async static Task UnZipFileAsync(StorageFile zipFile, StorageFolder destinationFolder)
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
