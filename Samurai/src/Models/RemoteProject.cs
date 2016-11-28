using LibGit2Sharp;
using SharpCompress.Archives;
using SharpCompress.Readers;
using System;
using System.IO;
using System.Net;

namespace Samurai.Models
{
    public class RemoteProject: Project
    {
        public Source Source { get; set; }

        public void Fetch()
        {
            Download();
            Copy();
        }

        /// <summary>
        /// Download the resource into <see cref="GlobalPath"/>
        /// </summary>
        public void Download()
        {
            // We don't overwrite existing directories
            if (Directory.Exists(GlobalPath)) return;

            Common.PrintImportantStep($"Downloading {Name}");

            if (Source.Type == Source.GitTypeName)
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
                    string commitPointer = $"refs/tags/{Version}";
                    var commit = repo.Lookup<Commit>(commitPointer);
                    repo.Checkout(commitPointer);
                }
                Console.WriteLine();
            }
            else if (Source.Type == Source.ArchiveTypeName)
            {
                string filename = Path.GetFileNameWithoutExtension(new Uri(Source.Url).AbsolutePath);
                string archiveName = Guid.NewGuid().ToString() + "." + Path.GetExtension(new Uri(Source.Url).AbsolutePath);
                string downloadPath = Path.Combine(Locations.DotFolderPath, archiveName);

                var webClient = new WebClient();
                webClient.DownloadFile(Source.Url, downloadPath);

                ExtractionOptions options = new ExtractionOptions();
                options.ExtractFullPath = true;
                ArchiveFactory.WriteToDirectory(downloadPath, Locations.DotFolderPath, options);

                string extractedDirName = Path.Combine(Locations.DotFolderPath, filename);
                Directory.Move(extractedDirName, GlobalPath);

                File.Delete(downloadPath);
            }
            else if (Source.Type == Source.FileTypeName)
            {
                Directory.CreateDirectory(GlobalPath);

                string filename = Path.GetFileName(new Uri(Source.Url).AbsolutePath);
                string downloadPath = Path.Combine(GlobalPath, filename);

                var webClient = new WebClient();
                webClient.DownloadFile(Source.Url, downloadPath);
            }
        }

        /// <summary>
        /// Copies the resource from <see cref="GlobalPath"/> to <see cref="Project.LocalPath"/>
        /// </summary>
        void Copy()
        {
            // We don't overwrite existing directories
            if (Directory.Exists(LocalPath)) return;

            Common.PrintImportantStep($"Copying {Name}");

            try
            {
                Common.DirectoryCopy(GlobalPath, LocalPath, true);
            }
            catch (Exception e)
            {
                Common.PrintException(e);
            }
        }

        public void ApplyPatch()
        {
            if (Patch == null || Patch.Length == 0) return;

            // Patch absolute path
            string patchPath = Path.Combine(Environment.CurrentDirectory, Patch);
            // Apply patch using git cli, LibGit2Sharp doesn't support applying patches yet
            Common.RunCommand("git", $"apply {patchPath}", LocalPath);
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

        string _globalPath;
        public string GlobalPath
        {
            get
            {
                if (_globalPath == null)
                {
                    if (Version != null && Version.Length > 0)
                    {
                        _globalPath = Path.Combine(Locations.DotFolderPath, $"{Name}@{Version}");
                    }
                    else
                    {
                        _globalPath = Path.Combine(Locations.DotFolderPath, Name);
                    }
                }
                return _globalPath;
            }
        }
    }
}
