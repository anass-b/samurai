using System.Collections.Generic;

namespace Samurai.Models
{
    public class Runnable
    {
        public string Os { get; set; }
        public string Name { get; set; }
        public List<string> Args { get; set; }
        public string WorkingDir { get; set; }
    }

    public class Build
    {
        public List<Runnable> Scripts { get; set; }
        public List<Runnable> Commands { get; set; }
    }
}
