using Microsoft.Extensions.CommandLineUtils;
using Samurai.Models;
using System;
using System.IO;
using Newtonsoft.Json;

namespace Samurai
{
    public class Controller
    {
        Config _config;

        CommandOption AddVarsOption(CommandLineApplication target)
        {
            return target.Option("--vars|-v", "Variables", CommandOptionType.SingleValue);
        }

        CommandOption AddSelfOption(CommandLineApplication target)
        {
            return target.Option("--self|-s", "Run self section", CommandOptionType.NoValue);
        }

        CommandOption AddConfigFileOption(CommandLineApplication target)
        {
            return target.Option("--config|-c", "Config file", CommandOptionType.SingleValue); 
        }

        string GetConfigFileNameOrDefault(CommandOption option)
        {
            if (option.HasValue())
            {
                return Path.GetFullPath(option.Value());
            }
            else
            {
                return Path.Combine(Environment.CurrentDirectory, Defaults.ConfigFileName);
            }
        }

        void ParseConfig(CommandOption configFile, CommandOption vars)
        {
            string configFilePath = GetConfigFileNameOrDefault(configFile);
            try
            {
                _config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(configFilePath));
                _config.PostParsingInit(vars.Value());
            }
            catch (FileNotFoundException)
            {
                Logs.PrintException($"{configFilePath} not found.");
                Environment.Exit(1);
            }
            catch (Exception)
            {
                Logs.PrintException($"Cannot open {configFilePath}.");
                Environment.Exit(1);
            }
        }

        public void AddFetchCli(CommandLineApplication cli)
        {
            CommandOption vars = null, configFile = null;
            cli.Command("fetch", (command) =>
            {
                configFile = AddConfigFileOption(command);
                vars = AddVarsOption(command);
            })
            .OnExecute(() =>
            {
                ParseConfig(configFile: configFile, vars: vars);
                return Fetch();
            });
        }

        public void AddPatchCli(CommandLineApplication cli)
        {
            CommandOption vars = null, configFile = null;
            cli.Command("patch", (command) =>
            {
                configFile = AddConfigFileOption(command);
                vars = AddVarsOption(command);
            })
            .OnExecute(() =>
            {
                ParseConfig(configFile: configFile, vars: vars);
                return Patch();
            });
        }

        public void AddCMakeCli(CommandLineApplication cli)
        {
            CommandOption vars = null, self = null, configFile = null;
            cli.Command("cmake", (command) =>
            {
                configFile = AddConfigFileOption(command);
                vars = AddVarsOption(command);
                self = AddSelfOption(command);
            })
            .OnExecute(() =>
            {
                ParseConfig(configFile: configFile, vars: vars);
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
            CommandOption vars = null, self = null, configFile = null;
            cli.Command("build", (command) =>
            {
                configFile = AddConfigFileOption(command);
                vars = AddVarsOption(command);
                self = AddSelfOption(command);
            })
            .OnExecute(() =>
            {
                ParseConfig(configFile: configFile, vars: vars);
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
            CommandOption vars = null, configFile = null;
            cli.Command("all", (command) =>
            {
                configFile = AddConfigFileOption(command);
                vars = AddVarsOption(command);
            })
            .OnExecute(() =>
            {
                ParseConfig(configFile: configFile, vars: vars);
                return All();
            });
        }

        int All()
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
                Logs.PrintException(e);
                return 1;
            }
        }

        int Fetch()
        {
            try
            {
                foreach (var dep in _config.Dependencies)
                {
                    dep.Fetch();
                }

                return 0;
            }
            catch (Exception e)
            {
                Logs.PrintException(e);
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
                Logs.PrintException(e);
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
                Logs.PrintException(e);
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
                Logs.PrintException(e);
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
                Logs.PrintException(e);
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
                Logs.PrintException(e);
                return 1;
            }
        }
    }
}
