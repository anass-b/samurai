using System;
using System.IO;

namespace Samurai
{
    public static class Defaults
    {
        /// <summary>
        /// The file that holds the config that will be parsed into
        /// <see cref="Models.Config"/>
        /// </summary>
        public static string ConfigFileName = "samurai.json";

        /// <summary>
        /// Also named sometimes "local folder" throughout the code
        /// </summary>
        public static string VendorFolderName = "vendor";
    }
}
