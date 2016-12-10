using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Samurai
{
    public static class StringExtensions
    {
        /// <summary>
        /// Replaces occurences of <paramref name="wrongChar"/> with
        /// <see cref="Path.DirectorySeparatorChar"/>
        /// </summary>
        /// <returns>The wrong dir sep char.</returns>
        /// <param name="path">Path.</param>
        /// <param name="wrongChar">Wrong char.</param>
        public static string ReplaceWrongDirSepChar(this string path, char wrongChar = '\0')
        {
            if (wrongChar == '\0')
            {
                string os = OS.GetCurrent();
                if (os == OS.Windows)
                {
                    wrongChar = '/';
                }
                else if (os == OS.Linux || os == OS.MacOS)
                {
                    wrongChar = '\\';
                }
                else
                {
                    throw new Exception("Non supported OS");
                }
            }
            return path = path.Replace(wrongChar, Path.DirectorySeparatorChar);
        }
    }
}
