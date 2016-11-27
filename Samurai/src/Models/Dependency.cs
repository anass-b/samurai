using LibGit2Sharp;
using System;
using System.Diagnostics;
using System.IO;

namespace Samurai.Models
{
    public class Dependency : Buildable
    {
        public string Name { get; set; }

        string _version;
        public string Version
        {
            get
            {
                return _version;
            }

            set
            {
                if (value.Length == 0)
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
        public string Patch { get; set; }
        public Source Source { get; set; }

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

        string _localPath;
        public string LocalPath
        {
            get
            {
                if (_localPath == null)
                {
                    _localPath = Path.Combine(Locations.VendorFolderPath, Name);
                }
                return _localPath;
            }
        }

        public void Fetch()
        {
            Download();
            Copy();
        }

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
                throw new NotImplementedException();
            }
            else if (Source.Type == Source.FileTypeName)
            {
                throw new NotImplementedException();
            }
        }

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
    }
}
