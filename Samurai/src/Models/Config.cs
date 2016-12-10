using System.Collections.Generic;
using System.IO;

namespace Samurai.Models
{
    public delegate string GetInstallDirDelegate();

    public class Config
    {
        public string InstallDir { get; set; }
        public List<RemotePackage> Dependencies { get; set; }
        public Package Self { get; set; }

        public void PostParsingInit()
        {
            FixDirSeparatorInPaths();

            // Assign config to all deps so they can
            foreach (var dep in Dependencies)
            {
                dep.GetInstallDir = GetInstallDir;
            }

            if (Self != null)
            {
                Self.IsSelf = true;
                Self.GetInstallDir = GetInstallDir;
            }
        }

        string GetInstallDir()
        {
            return BasePath;
        }

        void FixDirSeparatorInPaths()
        {
            if (InstallDir != null)
            {
                InstallDir = InstallDir.ReplaceWrongDirSepChar();
            }

            if (Dependencies != null)
            {
                foreach (var dep in Dependencies)
                {
                    dep.FixDirSeparatorInPaths();
                }
            }

            if (Self != null)
            {
                Self.FixDirSeparatorInPaths();
            }
        }

        /// <summary>
        /// Directory where all packages will be installed
        /// </summary>
        string _basePath;
        public string BasePath
        {
            get
            {
                if (_basePath == null)
                {
                    if (string.IsNullOrWhiteSpace(InstallDir))
                    {
                        _basePath = Path.GetFullPath("vendor");
                    }
                    else if (Path.IsPathRooted(InstallDir))
                    {
                        _basePath = InstallDir;
                    }
                    else
                    {
                        _basePath = Path.GetFullPath(InstallDir);
                    }
                }
                return _basePath;
            }
        }
    }
}
