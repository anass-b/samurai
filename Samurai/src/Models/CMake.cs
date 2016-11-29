using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Samurai.Models
{
    public class Generator
    {
        public string Os { get; set; }
        public string Name { get; set; }
    }

    public class OsSpecificCMakeVarSet
    {
        public string Os { get; set; }
        public JObject Vars { get; set; }
    }

    public class CMake
    {
        public List<string> ExcludeOS { get; set; }
        public List<string> Args { get; set; }
        public JObject Vars { get; set; }
        public List<OsSpecificCMakeVarSet> OsSpecificVars { get; set; }

        /// <summary>
        /// If this is not empty or null, it overwrites <see cref="Generators"/>
        /// </summary>
        /// <value>The generator.</value>
        public string Generator { get; set; }

        public List<Generator> Generators { get; set; }
        public string WorkingDir { get; set; }
        public string SrcDir { get; set; }
    }
}
