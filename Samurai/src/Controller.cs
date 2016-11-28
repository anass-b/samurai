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
            _config.FixDirSeparatorInPaths();
        }

        CommandOption AddVarsOption(CommandLineApplication target)
        {
            return target.Option("--vars|-v", "Variables", CommandOptionType.SingleValue);
        }

        CommandOption AddSelfOption(CommandLineApplication target)
        {
            return target.Option("--self|-s", "Run self section", CommandOptionType.NoValue);
        }

        public void AddFetchCli(CommandLineApplication cli)
        {
            CommandOption vars = null;
            cli.Command("fetch", (command) =>
            {
                vars = AddVarsOption(command);
            })
            .OnExecute(() =>
            {
                _config.AssignVars(vars.Value());
                return Fetch();
            });
        }

        public void AddPatchCli(CommandLineApplication cli)
        {
            CommandOption vars = null;
            cli.Command("patch", (command) =>
            {
                vars = AddVarsOption(command);
            })
            .OnExecute(() =>
            {
                _config.AssignVars(vars.Value());
                return Patch();
            });
        }

        public void AddCMakeCli(CommandLineApplication cli)
        {
            CommandOption vars = null, self = null;
            cli.Command("cmake", (command) =>
            {
                vars = AddVarsOption(command);
                self = AddSelfOption(command);
            })
            .OnExecute(() =>
            {
                _config.AssignVars(vars.Value());
                if (self.HasValue())
                {
                    return SelfCMake();
                }
                else
                {
                    return CMake();
                }
            });
        }

        public void AddBuildCli(CommandLineApplication cli)
        {
            CommandOption vars = null, self = null;
            cli.Command("build", (command) =>
            {
                vars = AddVarsOption(command);
                self = AddSelfOption(command);
            })
            .OnExecute(() =>
            {
                _config.AssignVars(vars.Value());
                if (self.HasValue())
                {
                    return SelfBuild();
                }
                else
                {
                    return Build();
                }
            });
        }

        public void AddAllCli(CommandLineApplication cli)
        {
            CommandOption vars = null;
            cli.Command("all", (command) =>
            {
                vars = AddVarsOption(command);
            })
            .OnExecute(() =>
            {
                _config.AssignVars(vars.Value());
                return Build();
            });
        }

        int All(string vars)
        {
            try
            {
                Fetch();
                Patch();
                CMake();
                Build();
                SelfCMake();
                SelfBuild();
                return 0;
            }
            catch (Exception e)
            {
                Common.PrintException(e);
                return 1;
            }
        }

        int Fetch()
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

        int Patch()
        {
            try
            {
                foreach (var dep in _config.Dependencies)
                {
                    dep.ApplyPatch();
                }
                return 0;
            }
            catch (Exception e)
            {
                Common.PrintException(e);
                return 1;
            }
        }

        int CMake()
        {
            try
            {
                foreach (var dep in _config.Dependencies)
                {
                    dep.RunCMake();
                }
                return 0;
            }
            catch (Exception e)
            {
                Common.PrintException(e);
                return 1;
            }
        }

        int SelfCMake()
        {
            try
            {
                _config.Self.RunCMake();
                return 0;
            }
            catch (Exception e)
            {
                Common.PrintException(e);
                return 1;
            }
        }

        int Build()
        {
            try
            {
                foreach (var dep in _config.Dependencies)
                {
                    dep.RunBuild();
                }
                return 0;
            }
            catch (Exception e)
            {
                Common.PrintException(e);
                return 1;
            }
        }

        int SelfBuild()
        {
            try
            {
                _config.Self.RunBuild();
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
