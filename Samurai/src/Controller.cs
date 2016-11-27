using Microsoft.Extensions.CommandLineUtils;
using Samurai.Models;
using System;
using System.IO;

namespace Samurai
{
    public class Controller
    {
        readonly Config _config;

        public Controller(Config config)
        {
            _config = config;
        }

        public void AddFetchCli(CommandLineApplication cli)
        {
            cli.Command("fetch", (command) =>
            {
            })
            .OnExecute(() =>
            {
                return Fetch();
            });
        }

        public int Fetch()
        {
            try
            {
                if (!Directory.Exists(Locations.DotFolderPath))
                {
                    Directory.CreateDirectory(Locations.DotFolderPath);
                }

                if (!Directory.Exists(Locations.VendorFolderPath))
                {
                    Directory.CreateDirectory(Locations.VendorFolderPath);
                }

                foreach (var dep in _config.Dependencies)
                {
                    dep.Fetch();
                }

                return 0;
            }
            catch (Exception e)
            {
                Common.PrintException(e);
                return 1;
            }
        }
    }
}
