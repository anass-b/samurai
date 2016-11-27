using Microsoft.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using Samurai.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

                app.HelpOption("-? | -h | --help");
            }
            catch (FileNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{Locations.ConfigFileName} not found.");
                Console.ResetColor();
                return 1;
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.Clear();
                return 1;
            }

            return app.Execute(args);
        }
    }
}
