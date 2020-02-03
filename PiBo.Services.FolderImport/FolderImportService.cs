using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace PiBo.Services.FolderImport
{
    public class FolderImportService
    {
        private FileSystemWatcher _watcher;
        private ILogger _logger;

        public FolderImportService(ILogger<FolderImportService> logger)
        {
            _logger = logger;
        }

        public void Start()
        {
            _watcher = new FileSystemWatcher("D:\\TestPiBo");
            _watcher.Created += _watcher_Created;
            _watcher.EnableRaisingEvents = true;
        }

        private void _watcher_Created(object sender, FileSystemEventArgs e)
        {
            _logger.LogInformation($"{e.Name}: {e.ChangeType}");

            WaitForFile(e.FullPath);

            var directories = ImageMetadataReader.ReadMetadata(e.FullPath);

            foreach (var directory in directories)
            {
                foreach (var tag in directory.Tags)
                {
                    _logger.LogDebug($"{e.Name}:{directory.Name} - {tag.Name} = {tag.Description}");
                }
            }

            var imageInfo = new ImageInfo();

            var exifIfd0Directory = directories.OfType<ExifIfd0Directory>().FirstOrDefault();
            if(exifIfd0Directory != null)
            {
                imageInfo.CaptureDate = exifIfd0Directory.GetDateTime(ExifDirectoryBase.TagDateTime);
                imageInfo.Camera = exifIfd0Directory.GetString(ExifDirectoryBase.TagModel);
                imageInfo.Width = exifIfd0Directory.TryGetInt32(ExifSubIfdDirectory.TagImageWidth, out var Width) ? Width : 0;
                imageInfo.Height = exifIfd0Directory.TryGetInt32(ExifSubIfdDirectory.TagImageHeight, out var Height) ? Height : 0;
            }

            var exifSubIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
            if (exifSubIfdDirectory != null)
            {
                imageInfo.ExposureTime = exifSubIfdDirectory.TryGetDouble(ExifSubIfdDirectory.TagExposureTime, out var exposureTimer) ?  exposureTimer : Double.NaN;
                imageInfo.FNumber = exifSubIfdDirectory.TryGetDouble(ExifSubIfdDirectory.TagFNumber, out var fnumber) ? fnumber : Double.NaN;
                imageInfo.FocalLength35 = exifSubIfdDirectory.TryGetDouble(ExifSubIfdDirectory.TagFNumber, out var focalLength35) ? focalLength35 : Double.NaN;
                imageInfo.FocalLength = exifSubIfdDirectory.TryGetDouble(ExifSubIfdDirectory.TagFNumber, out var focalLength) ? focalLength : Double.NaN;
                imageInfo.LensModel = exifSubIfdDirectory.GetString(ExifSubIfdDirectory.TagLensModel);
            }

            var gpsInfo = directories.OfType<GpsDirectory>().FirstOrDefault();
            if (gpsInfo != null)
            {
                if (
                    gpsInfo.TryGetDouble(GpsDirectory.TagLatitude, out var latitude) &&
                    gpsInfo.TryGetDouble(GpsDirectory.TagLongitude, out var longitude))
                {
                    imageInfo.GpsInfo = new Position(longitude, latitude);
                }
            }

            _logger.LogInformation($"{e.Name}: Done");
        }

        public void Stop()
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Dispose();
        }

        bool WaitForFile(string fullPath)
        {
            int numTries = 0;
            while (true)
            {
                ++numTries;
                try
                {
                    // Attempt to open the file exclusively.
                    using (FileStream fs = new FileStream(fullPath,
                        FileMode.Open, FileAccess.ReadWrite,
                        FileShare.None, 100))
                    {
                        fs.ReadByte();

                        // If we got this far the file is ready
                        break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                       "WaitForFile {0} failed to get an exclusive lock: {1}",
                        fullPath, ex.ToString());

                    if (numTries > 10)
                    {
                        _logger.LogWarning(
                            "WaitForFile {0} giving up after 10 tries",
                            fullPath);
                        return false;
                    }

                    // Wait for the lock to be released
                    System.Threading.Thread.Sleep(500);
                }
            }

            _logger.LogTrace("WaitForFile {0} returning true after {1} tries",
                fullPath, numTries);
            return true;
        }
    }
}
