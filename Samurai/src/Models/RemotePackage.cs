using LibGit2Sharp;
using SharpCompress.Archives;
using SharpCompress.Readers;
using System;
using System.IO;
using System.Net;

namespace Samurai.Models
{
    /// <summary>
    /// A package that has to be downloaded and serves as a dependency.
    /// It can be a git repository, an archive or a file that can be
    /// downloaded using a URL
    /// </summary>
    public class RemotePackage: Package
    {
        public Source Source { get; set; }

        public void Fetch()
        {
            Download();
        }

        /// <summary>
        /// Downloads this package into <see cref="Package.PackagePath"/>
        /// </summary>
        public void Download()
        {
            // We don't overwrite existing directories
            if (Directory.Exists(PackagePath)) return;

            Logs.PrintImportantStep($"Downloading {Name}");

            if (Source.Type == Source.GitTypeName)
            {
                string os = OS.GetCurrent();

                // LibGit2Sharp seems to have issues running on macOS Sierra
                // For Linux/Unix/macOS we use git cli
                // For Windows we use LibGit2Sharp
                if (os == OS.Windows)
                {
                    CloneOptions options = new CloneOptions();
                    options.RecurseSubmodules = true;
                    options.OnTransferProgress = (TransferProgress progress) =>
                    {
                        Console.SetCursorPosition(0, Console.CursorTop);
                        Console.Write($"Transfer progress: {progress.ReceivedObjects}/{progress.TotalObjects}");
                        return true;
                    };
                    Repository.Clone(Source.Url, PackagePath, options);
                    var repo = new Repository(PackagePath);
                    if (Version != null)
                    {
                        string commitPointer = $"refs/tags/{Version}"; repo.Lookup<Commit>(commitPointer);
                        repo.Checkout(commitPointer);
                    }
                    Console.WriteLine();
                }
                else
                {
                    Shell.RunProgramWithArgs("git", $"clone {Source.Url} {PackagePath}");
                    if (Version != null)
                    {
                        Shell.RunProgramWithArgs("git", $"checkout tags/{Version}", workingDirectory: PackagePath);
                    }
                }
            }
            else if (Source.Type == Source.ArchiveTypeName)
            {
                string filename = Path.GetFileName(new Uri(Source.Url).AbsolutePath);
                string downloadPath = Path.Combine(BasePath, filename);

                try
                {
                    string extension = Archive.GetExtension(new Uri(Source.Url).AbsolutePath).ToLower();

                    if (!File.Exists(downloadPath))
                    {
                        var webClient = new WebClient();
                        webClient.DownloadFile(Source.Url, downloadPath);
                    }

                    string destination = Source.ArchiveHasRootDir ? BasePath : PackagePath;
                    if (extension == ".tar.gz")
                    {
                        Archive.ExtractTgz(downloadPath, destination);
                    }
                    else if (extension == ".tar")
                    {
                        Archive.ExtractTar(downloadPath, destination);
                    }
                    else if (extension == ".zip")
                    {
                        Archive.ExtractZip(downloadPath, destination);
                    }
                }
                finally
                {
                    // Cleanup
                    if (File.Exists(downloadPath))
                    {
                        File.Delete(downloadPath);
                    }
                }
            }
            else if (Source.Type == Source.FileTypeName)
            {
                Directory.CreateDirectory(PackagePath);

                try
                {
                    string filename = Path.GetFileName(new Uri(Source.Url).AbsolutePath);
                    string downloadPath = Path.Combine(PackagePath, filename);

                    var webClient = new WebClient();
                    webClient.DownloadFile(Source.Url, downloadPath);
                }
                catch
                {
                    // Cleanup
                    if (Directory.Exists(PackagePath))
                    {
                        Directory.Delete(PackagePath);
                    }
                }
            }
        }

        /// <summary>
        /// Applies a .patch on this package using git cli
        /// </summary>
        public void ApplyPatch()
        {
            if (string.IsNullOrWhiteSpace(Patch)) return;

            Logs.PrintImportantStep($"Patching {Name}");

            // Patch absolute path
            string patchPath = Path.Combine(Environment.CurrentDirectory, Patch);
            // Apply patch using git cli, LibGit2Sharp doesn't support applying patches yet
            Shell.RunProgramWithArgs("git", $"apply {patchPath}", workingDirectory: PackagePath);
        }

        public override void FixDirSeparatorInPaths()
        {
            base.FixDirSeparatorInPaths();
            if (Patch != null)
            {
                Patch = ReplaceWrongDirSepChar(Patch, GetWrongDirSepChar());
            }
        }

        public override void AssignVars(string varsStr)
        {
            base.AssignVars(varsStr);

            if (varsStr == null || varsStr.Length == 0) return;

            string[] tuples = varsStr.Split(';');
            if (tuples.Length == 0) return;

            Name = ReplaceVars(Name, tuples);
            Version = ReplaceVars(Version, tuples);

            Source.Url = ReplaceVars(Source.Url, tuples);
        }

        /// <summary>
        /// Package version
        /// </summary>
        string _version;
        public string Version
        {
            get { return _version; }
            set
            {
                if (value != null && value.Length == 0)
                {
                    // We don't accept zero length string, it will be considered as null
                    _version = null;
                }
                else
                {
                    _version = value;
                }
            }
        }
    }
}
