using System.Collections.Generic;

namespace Samurai.Models
{
    public class Config
    {
        public List<Dependency> Dependencies { get; set; }
        public Buildable Self { get; set; }
    }
}
