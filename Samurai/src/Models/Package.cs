using Newtonsoft.Json.Linq;
using System;
using System.IO;
using Newtonsoft.Json;

namespace Samurai.Models
{
    public class Package
    {
        /// <summary>
        /// <see cref="Config"/> class will assign a delegate in this property
        /// so we can get the value of <see cref="Config.InstallDir"/>
        /// </summary>
        /// <value>The get install dir.</value>
        public GetInstallDirDelegate GetInstallDir { get; set; }

        /// <summary>
        /// Directory where the package will be installed,
        /// excluding the package name
        /// </summary>
        /// <value>The install dir.</value>
        public string InstallDir { get; set; }

        /// <summary>
        /// Package name, serves as the folder name in the global ~/.samuarai
        /// or local vendor/
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Path of a .patch file, this should be relative to the
        /// <see cref="Environment.CurrentDirectory"/>
        /// </summary>
        /// <value>The patch.</value>
        public string Patch { get; set; }

        /// <summary>
        /// Cmake config
        /// </summary>
        /// <value>The CM ake.</value>
        public CMake CMake { get; set; }

        /// <summary>
        /// Build config
        /// </summary>
        /// <value>The build.</value>
        public Build Build { get; set; }

        /// <summary>
        /// Tells if this package is the <see cref="Config.Self"/> section
        /// </summary>
        /// <value><c>true</c> if is self; otherwise, <c>false</c>.</value>
        [JsonIgnore]
        public bool IsSelf { get; set; }

        /// <summary>
        /// Gets CMake vars that can be used with the current OS
        /// </summary>
        /// <returns>Vars JSON object</returns>
        JObject GetCurrentOsCMakeVars()
        {
            string os = OS.GetCurrent();
            foreach (var varSet in CMake.OsSpecificVars)
            {
                if (varSet.Os == os) return varSet.Vars;
            }
            throw new Exception("Non supported OS");
        }

        /// <summary>
        /// Converts vars JSON object to CMake args
        /// </summary>
        /// <returns>The ake variables to arguments.</returns>
        /// <param name="vars">CMake args</param>
        string CMakeVarsToArgs(JObject vars)
        {
            var args = "";
            foreach (var prop in vars.Properties())
            {
                args += $" -D{prop.Name}=\"{prop.Value}\"";
            }
            return args;
        }

        /// <summary>
        /// Gets a CMake generator for current OS.
        /// </summary>
        /// <returns>OS ID</returns>
        string GetGeneratorForCurrentOs()
        {
            string os = OS.GetCurrent();
            foreach (var generator in CMake.Generators)
            {
                if (generator.Os == os) return generator.Name;
            }
            throw new Exception("Non supported OS");
        }

        /// <summary>
        /// Run CMake on this package
        /// </summary>
        public void RunCMake()
        {
            if (CMake == null) return;

            if (CMake.ExcludeOS != null)
            {
                string currentOs = OS.GetCurrent();
                foreach (var os in CMake.ExcludeOS)
                {
                    if (currentOs == os) return;
                }
            }

            Logs.PrintImportantStep($"Running cmake {Name}");

            if (Path.IsPathRooted(CMake.SrcDir))
            {
                throw new Exception("script.workingDir cannot be a full path "
                                    + $"it should be set relatively to {PackagePath}");
            }

            string args = $"{CMake.SrcDir} ";
            if (CMake.Vars != null)
            {
                args += CMakeVarsToArgs(CMake.Vars);
            }
            if (CMake.OsSpecificVars != null)
            {
                args += CMakeVarsToArgs(GetCurrentOsCMakeVars());
            }
            if (CMake.Args != null)
            {
                foreach (var arg in CMake.Args)
                {
                    args += $" {arg}";
                }
            }

            // CMake generator
            if (!string.IsNullOrWhiteSpace(CMake.Generator))
            {
                args += $" -G\"{CMake.Generator}\"";
            }
            else if (CMake.Generators != null)
            {
                string generator = GetGeneratorForCurrentOs();
                args += $" -G\"{generator}\"";
            }

            args = args.Trim();

            string workingDir = null;
            if (Path.IsPathRooted(CMake.WorkingDir))
            {
                workingDir = CMake.WorkingDir;
            }
            else
            {
                workingDir = Path.Combine(PackagePath, CMake.WorkingDir);
            }
            if (!Directory.Exists(workingDir)) Directory.CreateDirectory(workingDir);

            Shell.RunProgramWithArgs("cmake", args, workingDir);
        }

        /// <summary>
        /// Run build config for current OS
        /// </summary>
        /// <param name="os">Os.</param>
        void RunBuildScriptForOs(string os)
        {
            if (Build.Scripts != null)
            {
                foreach (var script in Build.Scripts)
                {
                    if (!string.IsNullOrWhiteSpace(script.Os) && script.Os != os) continue;
                    if (string.IsNullOrWhiteSpace(script.WorkingDir))
                    {
                        throw new Exception("build.script.workingDir cannot be null or empty");
                    }
                    if (string.IsNullOrWhiteSpace(script.Name))
                    {
                        throw new Exception("build.script.name cannot be null or empty");
                    }

                    Logs.PrintImportantStep($"Building {Name}");

                    string argsStr = "";
                    if (script.Args != null)
                    {
                        foreach (var arg in script.Args)
                        {
                            argsStr += $" {arg}";
                        }
                    }

                    if (Path.IsPathRooted(script.WorkingDir))
                    {
                        throw new Exception("script.workingDir cannot be a full path "
                                            + $"it should be set relatively to {PackagePath}");
                    }
                    string workingDir = Path.Combine(PackagePath, script.WorkingDir);
                    string scriptPath = Path.Combine(Environment.CurrentDirectory, script.Name);
                    Shell.RunProgramWithArgs(scriptPath, argsStr.Trim(), workingDir);
                }
            }

            if (Build.Commands != null)
            {
                foreach (var command in Build.Commands)
                {
                    // Check requirements
                    if (!string.IsNullOrWhiteSpace(command.Os) && command.Os != os) continue;
                    if (string.IsNullOrWhiteSpace(command.WorkingDir))
                    {
                        throw new Exception("build.command.workingDir cannot be null or empty");
                    }
                    if (string.IsNullOrWhiteSpace(command.Name))
                    {
                        throw new Exception("build.command.name cannot be null or empty");
                    }

                    Logs.PrintImportantStep($"Building {Name}");

                    string argsStr = "";
                    if (command.Args != null)
                    {
                        foreach (var arg in command.Args)
                        {
                            argsStr += $" {arg}";
                        }
                    }

                    if (Path.IsPathRooted(command.WorkingDir))
                    {
                        throw new Exception("command.workingDir cannot be a full path "
                                            + $"it should be set relatively to {PackagePath}");
                    }
                    string workingDir = Path.Combine(PackagePath, command.WorkingDir);
                    Shell.RunProgramWithArgs(command.Name, argsStr.Trim(), workingDir);
                }
            }
        }

        /// <summary>
        /// Run build config
        /// </summary>
        public void RunBuild()
        {
            if (Build == null) return;

            RunBuildScriptForOs(OS.GetCurrent());
        }

        

        /// <summary>
        /// Replaces directory separators that are not compatible with current OS
        /// with the appropriate one, which is taken from <see cref="Path.DirectorySeparatorChar"/>
        /// </summary>
        public virtual void FixDirSeparatorInPaths()
        {
            string os = OS.GetCurrent();

            if (InstallDir != null)
            {
                InstallDir = InstallDir.ReplaceWrongDirSepChar();
            }

            if (CMake != null)
            {
                CMake.SrcDir = CMake.SrcDir.ReplaceWrongDirSepChar();
                CMake.WorkingDir = CMake.WorkingDir.ReplaceWrongDirSepChar();
            }
            if (Build != null)
            {
                if (Build.Scripts != null)
                {
                    for (var i = 0; i < Build.Scripts.Count; i++)
                    {
                        Build.Scripts[i].WorkingDir = Build.Scripts[i].WorkingDir.ReplaceWrongDirSepChar();

                        // We fix paths for values that concerns the current OS only
                        if (Build.Scripts[i].Os == os)
                        {
                            Build.Scripts[i].Name = Build.Scripts[i].Name.ReplaceWrongDirSepChar();
                        }
                    }
                }
            }
            if (Patch != null)
            {
                Patch = Patch.ReplaceWrongDirSepChar();
            }
        }

        /// <summary>
        /// Gets the full path of <see cref="InstallDir"/>. It considered the
        /// base path of every relative path in the package.
        /// </summary>
        /// <returns>
        /// If <see cref="InstallDir"/> is relative, returns the equivalent full path.
        /// If <see cref="InstallDir"/> is null, empty, returns the absolute path
        /// of the vendor folder
        /// </returns>
        string _basePath;
        public string BasePath
        {
            get
            {
                if (_basePath == null)
                {
                    if (IsSelf)
                    {
                        return Environment.CurrentDirectory;
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(InstallDir))
                        {
                            if (!Path.IsPathRooted(InstallDir))
                            {
                                _basePath = Path.GetFullPath(InstallDir);
                            }
                            else
                            {
                                _basePath = InstallDir;
                            }
                        }
                        else
                        {
                            if (GetInstallDir == null)
                            {
                                throw new Exception($"{GetInstallDir.GetType().Name} delegate is null. "
                                    + $"Call method PostParsingInit() in {typeof(Config).Name} class");
                            }
                            _basePath = GetInstallDir();
                        }
                    }
                }
                return _basePath;
            }
        }

        /// <summary>
        /// Full path of the package directory: <see cref="BasePath"/> + <see cref="Name"/>
        /// </summary>
        string _packagePath;
        public string PackagePath
        {
            get
            {
                if (_packagePath == null)
                {
                    if (IsSelf)
                    {
                        _packagePath = Environment.CurrentDirectory;
                    }
                    else
                    {
                        _packagePath = Path.Combine(BasePath, Name);
                    }
                }
                return _packagePath;
            }
        }
    }
}
