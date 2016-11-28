using System;
using System.IO;

namespace Samurai
{
    public class Locations
    {
        public static string DotFolderName = ".samurai";

        /// <summary>
        /// The file that holds the config that will be parsed into
        /// <see cref="Models.Config"/>
        /// </summary>
        public static string ConfigFileName = "samurai.json";

        /// <summary>
        /// Also named sometimes "local folder" throughout the code
        /// </summary>
        public static string VendorFolderName = "vendor";

        /// <summary>
        /// Full path of the global .samurai directory
        /// </summary>
        static string _dotFolderPath;
        public static string DotFolderPath
        {
            get
            {
                if (_dotFolderPath == null)
                {
                    _dotFolderPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                        DotFolderName);
                }
                return _dotFolderPath;
            }
        }

        /// <summary>
        /// Full path of samurai.json
        /// </summary>
        static string _configFilePath;
        public static string ConfigFilePath
        {
            get
            {
                if (_configFilePath == null)
                {
                    _configFilePath = Path.Combine(Environment.CurrentDirectory, ConfigFileName);
                }
                return _configFilePath;
            }
        }

        /// <summary>
        /// Full path of the local vendor file
        /// </summary>
        static string _vendorFolderPath;
        public static string VendorFolderPath
        {
            get
            {
                if (_vendorFolderPath == null)
                {
                    _vendorFolderPath = Path.Combine(Environment.CurrentDirectory, VendorFolderName);
                }
                return _vendorFolderPath;
            }
        }
    }
}
