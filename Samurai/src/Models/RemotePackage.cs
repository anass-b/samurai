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
            Copy();
        }

        /// <summary>
        /// Downloads this package into <see cref="GlobalPath"/>
        /// </summary>
        public void Download()
        {
            // We don't overwrite existing directories
            if (Directory.Exists(GlobalPath)) return;

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
                    Repository.Clone(Source.Url, GlobalPath, options);
                    Repository repo = new Repository(GlobalPath);
                    if (Version != null)
                    {
                        string commitPointer = $"refs/tags/{Version}"; repo.Lookup<Commit>(commitPointer);
                        repo.Checkout(commitPointer);
                    }
                    Console.WriteLine();
                }
                else
                {
                    Shell.RunProgramWithArgs("git", $"clone {Source.Url} {GlobalPath}");
                    if (Version != null)
                    {
                        Shell.RunProgramWithArgs("git", $"checkout tags/{Version}", GlobalPath);
                    }
                }
            }
            else if (Source.Type == Source.ArchiveTypeName)
            {
                string filename = Path.GetFileNameWithoutExtension(new Uri(Source.Url).AbsolutePath);
                string archiveName = Guid.NewGuid() + Path.GetExtension(new Uri(Source.Url).AbsolutePath);
                string downloadPath = Path.Combine(Locations.DotFolderPath, archiveName);
                string extractedDirPath = Path.Combine(Locations.DotFolderPath, filename);

                try
                {
                    var webClient = new WebClient();
                    webClient.DownloadFile(Source.Url, downloadPath);

                    ExtractionOptions options = new ExtractionOptions();
                    options.ExtractFullPath = true;
                    ArchiveFactory.WriteToDirectory(downloadPath, Locations.DotFolderPath, options);

                    Directory.Move(extractedDirPath, GlobalPath);
                }
                finally
                {
                    // Cleanup
                    if (Directory.Exists(extractedDirPath))
                    {
                        Directory.Delete(extractedDirPath, true);
                    }
                    if (File.Exists(downloadPath))
                    {
                        File.Delete(downloadPath);
                    }
                }
            }
            else if (Source.Type == Source.FileTypeName)
            {
                Directory.CreateDirectory(GlobalPath);

                try
                {
                    string filename = Path.GetFileName(new Uri(Source.Url).AbsolutePath);
                    string downloadPath = Path.Combine(GlobalPath, filename);

                    var webClient = new WebClient();
                    webClient.DownloadFile(Source.Url, downloadPath);
                }
                catch
                {
                    // Cleanup
                    if (Directory.Exists(GlobalPath))
                    {
                        Directory.Delete(GlobalPath);
                    }
                }
            }
        }

        /// <summary>
        /// Copies this package from <see cref="GlobalPath"/> to <see cref="Package.LocalPath"/>
        /// </summary>
        void Copy()
        {
            // We don't overwrite existing directories
            if (Directory.Exists(LocalPath)) return;

            Logs.PrintImportantStep($"Copying {Name}");

            try
            {
                FileSystem.DirectoryCopy(GlobalPath, LocalPath, true);
            }
            catch (Exception e)
            {
                Logs.PrintException(e);
            }
        }

        /// <summary>
        /// Applies a .patch on this package using git cli
        /// </summary>
        public void ApplyPatch()
        {
            if (string.IsNullOrEmpty(Patch)) return;

            Logs.PrintImportantStep($"Patching {Name}");

            // Patch absolute path
            string patchPath = Path.Combine(Environment.CurrentDirectory, Patch);
            // Apply patch using git cli, LibGit2Sharp doesn't support applying patches yet
            Shell.RunProgramWithArgs("git", $"apply {patchPath}", LocalPath);
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

        /// <summary>
        /// The package of the package inside ~/.samurai folder
        /// </summary>
        string _globalPath;
        public string GlobalPath
        {
            get
            {
                if (_globalPath == null)
                {
                    if (string.IsNullOrEmpty(Version))
                    {
                        _globalPath = Path.Combine(Locations.DotFolderPath, Name);
                    }
                    else
                    {
                        _globalPath = Path.Combine(Locations.DotFolderPath, $"{Name}@{Version}");
                    }
                }
                return _globalPath;
            }
        }
    }
}
