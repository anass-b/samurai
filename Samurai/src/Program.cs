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
                var ctrl = new Controller();
                ctrl.AddFetchCli(app);
                ctrl.AddPatchCli(app);
                ctrl.AddCMakeCli(app);
                ctrl.AddBuildCli(app);
                ctrl.AddAllCli(app);

                app.HelpOption("-? | -h | --help");
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
