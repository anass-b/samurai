using Microsoft.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using Samurai.Models;
using System;
using System.IO;

namespace Samurai
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var app = new CommandLineApplication(throwOnUnexpectedArg: true);

            try
            {
                Config config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(Locations.ConfigFilePath));
                var ctrl = new Controller(config);
                ctrl.AddFetchCli(app);
                ctrl.AddPatchCli(app);
                ctrl.AddCMakeCli(app);
                ctrl.AddBuildCli(app);
                ctrl.AddAllCli(app);
                ctrl.AddZombiesCli(app);

                app.HelpOption("-? | -h | --help");
            }
            catch (FileNotFoundException)
            {
                Logs.PrintException($"{Locations.ConfigFileName} not found.");
                return 1;
            }
            catch (Exception e)
            {
                Logs.PrintException(e);
                return 1;
            }

            return app.Execute(args);
        }
    }
}
