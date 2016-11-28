using System.Collections.Generic;

namespace Samurai.Models
{
    public class Config
    {
        public List<Dependency> Dependencies { get; set; }

        public string ReplaceVars(string str, string[] tuples)
        {
            foreach (string tuple in tuples)
            {
                string[] values = tuple.Split('=');
                string var = "${" + values[0] + "}";
                str = str.Replace(var, values[1]);
            }
            return str;
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
                    dep.CMake.Generator = ReplaceVars(dep.CMake.Generator, tuples);
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
                            script.Args[i] = ReplaceVars(script.Args[i], tuples);
                        }
                    }
                }
            }
        }
    }
}
