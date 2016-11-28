using System.Collections.Generic;

namespace Samurai.Models
{
    public class Config
    {
        public List<RemotePackage> Dependencies { get; set; }
        public Package Self { get; set; }

        public void FixDirSeparatorInPaths()
        {
            if (Dependencies != null)
            {
                foreach (var dep in Dependencies)
                {
                    dep.FixDirSeparatorInPaths();
                }
            }

            if (Self != null)
            {
                Self.FixDirSeparatorInPaths();
            }
        }

        public void AssignVars(string varsStr)
        {
            if (Dependencies != null)
            {
                foreach (var dep in Dependencies)
                {
                    dep.AssignVars(varsStr);
                }
            }

            if (Self != null)
            {
                Self.AssignVars(varsStr);
            }
        }
    }
}
