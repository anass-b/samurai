using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace Samurai.Models
{
    public class Package
    {
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
        /// Gets a CMake generator for current OS.
        /// </summary>
        /// <returns>OS ID</returns>
        string GetGeneratorForCurrentOs()
        {
            string os = Common.GetOs();
            foreach (var generator in CMake.Generators)
            {
                if (generator.Os == os) return generator.Name;
            }
            throw new Exception("Non supported OS");
        }

        /// <summary>
        /// Gets CMake vars that can be used with the current OS
        /// </summary>
        /// <returns>Vars JSON object</returns>
        JObject GetCurrentOsCMakeVars()
        {
            string os = Common.GetOs();
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
        /// Run CMake on this package
        /// </summary>
        public void RunCMake()
        {
            if (CMake == null) return;
            
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
            if (CMake.Generators != null)
            {
                args += $" -G\"{GetGeneratorForCurrentOs()}\"";
            }
            args = args.Trim();

            string workingDir = Path.Combine(LocalPath, CMake.WorkingDir);

            if (!Directory.Exists(workingDir)) Directory.CreateDirectory(workingDir);

            Common.RunCommand("cmake", args, workingDir);
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
                    if (script.Os == os)
                    {
                        string argsStr = "";
                        if (script.Args != null)
                        {
                            foreach (var arg in script.Args)
                            {
                                argsStr += $" {arg}";
                            }
                        }
                        string workingDir = Path.Combine(LocalPath, Build.WorkingDir);
                        string scriptPath = Path.Combine(Environment.CurrentDirectory, script.Name);
                        Common.RunCommand(scriptPath, argsStr.Trim(), workingDir);
                        return;
                    }
                }
            }

            if (Build.Commands != null)
            {
                foreach (var command in Build.Commands)
                {
                    if (command.Os == os)
                    {
                        string argsStr = "";
                        if (command.Args != null)
                        {
                            foreach (var arg in command.Args)
                            {
                                argsStr += $" {arg}";
                            }
                        }
                        string workingDir = Path.Combine(LocalPath, Build.WorkingDir);
                        Common.RunCommand(command.Name, argsStr.Trim(), workingDir);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Run build config
        /// </summary>
        public void RunBuild()
        {
            if (Build == null) return;

            RunBuildScriptForOs(Common.GetOs());
        }

        /// <summary>
        /// Get the path separator that should not be used with the current OS
        /// Example: if we are on Windows this method returns '/',
        /// on Unix/Linux/macOS it returns '\\'
        /// </summary>
        /// <returns>Char containing the path separator that should
        /// not be used with the current OS</returns>
        protected char GetWrongDirSepChar()
        {
            string os = Common.GetOs();
            if (os == Common.OsIdWin)
            {
                return '/';
            }
            else if (os == Common.OsIdUnix || os == Common.OsIdMacOS)
            {
                return '\\';
            }
            else
            {
                throw new Exception("Non supported OS");
            }
        }

        /// <summary>
        /// Replaces occurences of <paramref name="wrongChar"/> with
        /// <see cref="Path.DirectorySeparatorChar"/>
        /// </summary>
        /// <returns>The wrong dir sep char.</returns>
        /// <param name="path">Path.</param>
        /// <param name="wrongChar">Wrong char.</param>
        protected string ReplaceWrongDirSepChar(string path, char wrongChar)
        {
            return path = path.Replace(wrongChar, Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Replaces directory separators that are not compatible with current OS
        /// with the appropriate one, which is taken from <see cref="Path.DirectorySeparatorChar"/>
        /// </summary>
        public virtual void FixDirSeparatorInPaths()
        {
            string os = Common.GetOs();
            char wrongChar = GetWrongDirSepChar();

            if (CMake != null)
            {
                CMake.SrcDir = ReplaceWrongDirSepChar(CMake.SrcDir, wrongChar);
                CMake.WorkingDir = ReplaceWrongDirSepChar(CMake.WorkingDir, wrongChar);
            }
            if (Build != null)
            {
                Build.WorkingDir = ReplaceWrongDirSepChar(Build.WorkingDir, wrongChar);
                if (Build.Scripts != null)
                {
                    for (var i = 0; i < Build.Scripts.Count; i++)
                    {
                        // We fix paths for values that concerns the current OS only
                        if (Build.Scripts[i].Os == os)
                        {
                            Build.Scripts[i].Name = ReplaceWrongDirSepChar(Build.Scripts[i].Name, wrongChar);
                        }
                    }
                }
            }
            if (Patch != null)
            {
                Patch = ReplaceWrongDirSepChar(Patch, wrongChar);
            }
        }

        /// <summary>
        /// Replaces strings that look like ${VAR} with their values
        /// using <paramref name="tuples"/>
        /// </summary>
        /// <returns>Final string with replaced vars</returns>
        /// <param name="str">String to be processed</param>
        /// <param name="tuples">Tuples of Var/Value separated by '=' character</param>
        protected string ReplaceVars(string str, string[] tuples)
        {
            if (str == null || str.Length == 0) return null;
            if (tuples == null || tuples.Length == 0) return null;

            foreach (string tuple in tuples)
            {
                string[] values = tuple.Split('=');
                string var = "${" + values[0] + "}";
                str = str.Replace(var, values[1]);
            }
            return str;
        }

        /// <summary>
        /// Replaces strings that look like ${VAR} with their values
        /// on the whole config
        /// </summary>
        /// <param name="varsStr">Vars from command line argument --vars</param>
        public virtual void AssignVars(string varsStr)
        {
            if (string.IsNullOrEmpty(varsStr)) return;

            string[] tuples = varsStr.Split(';');
            if (tuples.Length == 0) return;

            Name = ReplaceVars(Name, tuples);

            if (CMake != null)
            {
                if (CMake.Generators != null)
                {
                    for (var i = 0; i < CMake.Generators.Count; i++)
                    {
                        CMake.Generators[i].Name = ReplaceVars(CMake.Generators[i].Name, tuples);
                    }
                }
                if (CMake.Vars != null)
                {
                    foreach (var prop in CMake.Vars.Properties())
                    {
                        prop.Value = ReplaceVars(prop.Value.ToString(), tuples);
                    }
                }
                if (CMake.OsSpecificVars != null)
                {
                    for (var i = 0; i < CMake.OsSpecificVars.Count; i++)
                    {
                        foreach (var prop in CMake.OsSpecificVars[i].Vars.Properties())
                        {
                            prop.Value = ReplaceVars(prop.Value.ToString(), tuples);
                        }
                    }
                }
                CMake.WorkingDir = ReplaceVars(CMake.WorkingDir, tuples);
                CMake.SrcDir = ReplaceVars(CMake.SrcDir, tuples);
            }

            if (Build != null)
            {
                Build.WorkingDir = ReplaceVars(Build.WorkingDir, tuples);

                if (Build.Scripts != null)
                {
                    foreach (var script in Build.Scripts)
                    {
                        script.Os = ReplaceVars(script.Os, tuples);
                        script.Name = ReplaceVars(script.Name, tuples);
                        if (script.Args != null)
                        {
                            for (var i = 0; i < script.Args.Count; i++)
                            {
                                script.Args[i] = ReplaceVars(script.Args[i], tuples);
                            }
                        }
                    }
                }
                
                if (Build.Commands != null)
                {
                    foreach (var command in Build.Commands)
                    {
                        command.Os = ReplaceVars(command.Os, tuples);
                        command.Name = ReplaceVars(command.Name, tuples);
                        if (command.Args != null)
                        {
                            for (var i = 0; i < command.Args.Count; i++)
                            {
                                command.Args[i] = ReplaceVars(command.Args[i], tuples);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Path of this package in local vendor/ directory
        /// </summary>
        string _localPath;
        public string LocalPath
        {
            get
            {
                if (_localPath == null)
                {
                    if (string.IsNullOrEmpty(Name))
                    {
                        _localPath = Environment.CurrentDirectory;
                    }
                    else
                    {
                        _localPath = Path.Combine(Locations.VendorFolderPath, Name);
                    }
                }
                return _localPath;
            }
        }

    }
}
