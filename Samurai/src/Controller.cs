using Microsoft.Extensions.CommandLineUtils;
using Samurai.Models;
using System;
using System.IO;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

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
                var text = File.ReadAllText(configFilePath);
                text = AssignVars(text, vars.Value());
                _config = JsonConvert.DeserializeObject<Config>(text);
                _config.PostParsingInit();
            }
            catch (FileNotFoundException)
            {
                Logs.PrintException($"{configFilePath} not found.");
                Environment.Exit(1);
            }
            catch (JsonSerializationException e)
            {
                Logs.PrintException(e);
            }
            catch (Exception e)
            {
                Logs.PrintException(e);
                Environment.Exit(1);
            }
        }

        string EscapeBackslash(string str)
        {
            return str.Replace(@"\", @"\\");
        }

        /// <summary>
        /// Replaces strings that look like ${VAR} with their values
        /// using <paramref name="tuples"/>
        /// </summary>
        /// <returns>Final string with replaced vars</returns>
        /// <param name="str">String to be processed</param>
        /// <param name="tuples">Tuples of Var/Value separated by '=' character</param>
        protected string ReplaceVars(string str, string[] tuples)
        {
            if (str == null || str.Length == 0) return null;
            if (tuples == null || tuples.Length == 0) return null;

            foreach (string tuple in tuples)
            {
                string[] values = tuple.Split('=');
                string var = "${" + values[0] + "}";
                str = str.Replace(var, EscapeBackslash(values[1]));
            }
            return str;
        }

        /// <summary>
        /// Replaces strings that look like ${VAR} with their values from the CLI option --vars
        /// and @{VAR} with values from the environment
        /// on the whole config
        /// </summary>
        /// <param name="text">Config text content</param>
        /// <param name="varsStr">Vars from command line argument --vars</param>
        public string AssignVars(string text, string varsStr)
        {
            if (!string.IsNullOrWhiteSpace(varsStr))
            {
                string[] tuples = varsStr.Split(';');
                if (tuples.Length == 0) return text;

                text = ReplaceVars(text, tuples);
            }

            var regex = new Regex("@{(.*)}", RegexOptions.IgnoreCase);
            var match = regex.Match(text);
            while (match.Success)
            {
                var decoratedVar = match.Value;
                
                // Extract the variable without decoration
                var var = decoratedVar.Substring(2, decoratedVar.Length - 3);

                var envValue = Environment.GetEnvironmentVariable(var);
                if (envValue != null)
                {
                    // Replace the decorated variable with the value from the environement
                    text = text.Replace(decoratedVar, EscapeBackslash(envValue));
                }

                match = match.NextMatch();
            }

            return text;
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
            if (!Directory.Exists(_config.BasePath))
            {
                Directory.CreateDirectory(_config.BasePath);
            }

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
                if (_config.Self != null)
                {
                    _config.Self.RunCMake();
                }
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
                if (_config.Self != null)
                {
                    _config.Self.RunBuild();
                }
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
