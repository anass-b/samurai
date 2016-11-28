using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace Samurai.Models
{
    public class Project
    {
        public string Name { get; set; }
        public string Patch { get; set; }
        public CMake CMake { get; set; }
        public Build Build { get; set; }

        string GetGeneratorForCurrentOs()
        {
            string os = Common.GetOs();
            foreach (var generator in CMake.Generators)
            {
                if (generator.Os == os) return generator.Name;
            }
            throw new Exception("Non supported OS");
        }

        JObject GetCurrentOsCMakeVars()
        {
            string os = Common.GetOs();
            foreach (var varSet in CMake.OsSpecificVars)
            {
                if (varSet.Os == os) return varSet.Vars;
            }
            throw new Exception("Non supported OS");
        }

        string CMakeVarsToArgs(JObject vars)
        {
            var args = "";
            foreach (var prop in vars.Properties())
            {
                args += $" -D{prop.Name}=\"{prop.Value}\"";
            }
            return args;
        }

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

        void RunBuildScriptForOs(string os)
        {
            foreach (var script in Build.Scripts)
            {
                if (script.Os == os)
                {
                    string argsStr = "";
                    foreach (var arg in script.Args)
                    {
                        argsStr += $" {arg}";
                    }
                    string workingDir = Path.Combine(LocalPath, Build.WorkingDir);
                    string scriptPath = Path.Combine(Environment.CurrentDirectory, script.Run);
                    Common.RunCommand(scriptPath, argsStr.Trim(), workingDir);
                    return;
                }
            }
        }

        public void RunBuild()
        {
            if (Build == null) return;

            RunBuildScriptForOs(Common.GetOs());
        }

        protected string ReplaceWrongDirSepChar(string path, char wrongChar)
        {
            return path = path.Replace(wrongChar, Path.DirectorySeparatorChar);
        }

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
                for (var i = 0; i < Build.Scripts.Count; i++)
                {
                    // We fix paths for values that concerns the current OS only
                    if (Build.Scripts[i].Os == os)
                    {
                        Build.Scripts[i].Run = ReplaceWrongDirSepChar(Build.Scripts[i].Run, wrongChar);
                    }
                }
            }
            if (Patch != null)
            {
                Patch = ReplaceWrongDirSepChar(Patch, wrongChar);
            }
        }

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

        public virtual void AssignVars(string varsStr)
        {
            if (varsStr == null || varsStr.Length == 0) return;

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
                foreach (var script in Build.Scripts)
                {
                    script.Os = ReplaceVars(script.Os, tuples);
                    script.Run = ReplaceVars(script.Run, tuples);
                    for (var i = 0; i < script.Args.Count; i++)
                    {
                        script.Args[i] = ReplaceVars(script.Args[i], tuples);
                    }
                }
            }
        }

        string _localPath;
        public string LocalPath
        {
            get
            {
                if (_localPath == null)
                {
                    if (Name == null || Name.Length == 0)
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
