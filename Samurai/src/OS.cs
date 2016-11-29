using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Samurai
{
    public static class OS
    {
        public const string Windows = "win";
        public const string Linux = "linux";
        public const string MacOS = "macos";
        public const string FreeBSD = "freebsd";
        public const string Unix = "unix";

        public static string GetCurrent()
        {
            PlatformID platform = Environment.OSVersion.Platform;
            switch (platform)
            {
                case PlatformID.WinCE:
                case PlatformID.Win32Windows:
                case PlatformID.Win32S:
                case PlatformID.Win32NT:
                    return Windows;
                case PlatformID.MacOSX:
                    return MacOS;
                case PlatformID.Unix:
                    return DetectUnixName();
            }
            throw new Exception("Non supported OS");
        }

        static string DetectUnixName()
        {
            string uname = Shell.RunProgram("uname");
            uname = Regex.Replace(uname, @"\t|\n|\r", "").ToLower();
            switch (uname)
            {
                case "linux":
                    return Linux;
                case "darwin":
                    return MacOS;
                case "freebsd":
                    return FreeBSD;
                default:
                    return Unix;
            }
        }
    }
}
