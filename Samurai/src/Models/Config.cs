using System.Collections.Generic;
using System.IO;

namespace Samurai.Models
{
    public class Config
    {
        public List<Dependency> Dependencies { get; set; }

        public string ReplaceVars(string str, string[] tuples)
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

        string ReplaceWrongDirSepChar(string path, char wrongChar)
        {
            return path = path.Replace(wrongChar, Path.DirectorySeparatorChar);
        }

        public void FixDirSeparatorInPaths()
        {
            char wrongChar;
            string os = Common.GetOs();
            if (os == Common.OsIdWin)
            {
                wrongChar = '/';
            }
            else if (os == Common.OsIdUnix || os == Common.OsIdMacOS)
            {
                wrongChar = '\\';
            }
            else
            {
                return;
            }

            foreach (var dep in Dependencies)
            {
                if (dep.CMake != null)
                {
                    dep.CMake.SrcDir = ReplaceWrongDirSepChar(dep.CMake.SrcDir, wrongChar);
                    dep.CMake.WorkingDir = ReplaceWrongDirSepChar(dep.CMake.WorkingDir, wrongChar);
                }
                if (dep.Build != null)
                {
                    dep.Build.WorkingDir = ReplaceWrongDirSepChar(dep.Build.WorkingDir, wrongChar);
                    for (var i = 0; i < dep.Build.Scripts.Count; i++)
                    {
                        dep.Build.Scripts[i].Run = ReplaceWrongDirSepChar(dep.Build.Scripts[i].Run, wrongChar);
                    }
                }
                if (dep.Patch != null)
                {
                    dep.Patch = ReplaceWrongDirSepChar(dep.Patch, wrongChar);
                }
            }
        }

        public void AssignVars(string varsStr)
        {
            if (varsStr == null || varsStr.Length == 0) return;

            string[] tuples = varsStr.Split(';');
            if (tuples.Length == 0) return;

            foreach (var dep in Dependencies)
            {
                dep.Name = ReplaceVars(dep.Name, tuples);
                dep.Version = ReplaceVars(dep.Version, tuples);

                dep.Source.Type = ReplaceVars(dep.Source.Type, tuples);
                dep.Source.Url = ReplaceVars(dep.Source.Url, tuples);

                if (dep.CMake != null)
                {
                    if (dep.CMake.Generators != null)
                    {
                        for (var i = 0; i < dep.CMake.Generators.Count; i++)
                        {
                            var generator = dep.CMake.Generators[i];
                            generator.Name = ReplaceVars(generator.Name, tuples);
                        }
                    }
                    dep.CMake.WorkingDir = ReplaceVars(dep.CMake.WorkingDir, tuples);
                    dep.CMake.SrcDir = ReplaceVars(dep.CMake.SrcDir, tuples);
                }
                
                if (dep.Build != null)
                {
                    dep.Build.WorkingDir = ReplaceVars(dep.Build.WorkingDir, tuples);
                    foreach (var script in dep.Build.Scripts)
                    {
                        script.Os = ReplaceVars(script.Os, tuples);
                        script.Run = ReplaceVars(script.Run, tuples);
                        for (var i = 0; i < script.Args.Count; i++)
                        {
                            var arg = script.Args[i];
                            arg = ReplaceVars(arg, tuples);
                        }
                    }
                }
            }
        }
    }
}
