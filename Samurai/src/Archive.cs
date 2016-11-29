using System;
using System.IO;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.GZip;
using System.IO.Compression;
using System.Collections.Generic;

namespace Samurai
{
    public static class Archive
    {
        static List<string> SupportedExtensions = new List<string> { ".tar.gz", ".tar", ".zip" };

        public static string GetExtension(string fileName)
        {
            foreach (string extension in SupportedExtensions)
            {
                if (fileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                {
                    return extension;
                }
            }
            throw new Exception("Archive format not supported");
        }

        public static void ExtractTar(string tarFileName, string destFolder)
        {
            Stream inStream = File.OpenRead(tarFileName);

            TarArchive tarArchive = TarArchive.CreateInputTarArchive(inStream);
            tarArchive.ExtractContents(destFolder);
            tarArchive.Close();

            inStream.Close();
        }

        public static void ExtractTgz(string gzArchiveName, string destFolder)
        {
            Stream inStream = File.OpenRead(gzArchiveName);
            Stream gzipStream = new GZipInputStream(inStream);

            TarArchive tarArchive = TarArchive.CreateInputTarArchive(gzipStream);
            tarArchive.ExtractContents(destFolder);
            tarArchive.Close();

            gzipStream.Close();
            inStream.Close();
        }

        public static void ExtractZip(string zipArchiveName, string destFolder)
        {
            ZipFile.ExtractToDirectory(zipArchiveName, destFolder);
        }
    }
}