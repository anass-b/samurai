using System;
using System.Diagnostics;
using System.IO;

namespace Samurai
{
    public static class Shell
    {
        public static void RunProgramWithArgs(string program, string args, string workingDirectory = null)
        {
            var process = new Process();
            process.StartInfo.FileName = program;
            process.StartInfo.Arguments = args;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            if (workingDirectory != null)
            {
                process.StartInfo.WorkingDirectory = workingDirectory;
            }
            process.Start();

            // Synchronously read the standard output of the spawned process. 
            StreamReader reader = process.StandardOutput;
            string output = reader.ReadToEnd();

            // Write the redirected output to this application's window.
            Console.WriteLine(output);

            process.WaitForExit();
            process.Close();
        }

        public static string RunProgram(string program)
        {
            var process = new Process();
            process.StartInfo.FileName = program;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();

            // Synchronously read the standard output of the spawned process. 
            StreamReader reader = process.StandardOutput;
            string output = reader.ReadToEnd();

            process.WaitForExit();
            process.Close();

            return output;
        }
    }
}
