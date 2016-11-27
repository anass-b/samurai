using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Samurai
{
    public class Locations
    {
        public static string DotFolderName = ".samurai";
        public static string ConfigFileName = "samurai.json";
        public static string VendorFolderName = "vendor";

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
