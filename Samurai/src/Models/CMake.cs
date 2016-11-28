using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Samurai.Models
{
    public class CMake
    {
        public List<string> Args { get; set; }
        public JObject Vars { get; set; }
        public string Generator { get; set; }
        public string WorkingDir { get; set; }
        public string SrcDir { get; set; }
    }
}
