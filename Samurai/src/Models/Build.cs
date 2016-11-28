using System.Collections.Generic;

namespace Samurai.Models
{
    public class Script
    {
        public string Os { get; set; }
        public string Run { get; set; }
        public List<string> Args { get; set; }
    }

    public class Build
    {
        public string WorkingDir { get; set; }
        public List<Script> Scripts { get; set; }
    }
}
