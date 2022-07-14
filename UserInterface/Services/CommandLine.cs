using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserInterface.Services
{
    public class CommandLine
    {
        public async static Task Run(string Cmd, string[] Args)
        {
            ProcessRunner pr = new ProcessRunner();
            await pr.RunAsync(Cmd, Args);
        }

        public async static Task RunAsSuperUser(string Cmd, string[] Args)
        {
            string execPath = AppDomain.CurrentDomain.BaseDirectory;
            ProcessRunner pr = new ProcessRunner(execPath);
            await pr.RunAsSuperUserAsync(Cmd, Args);
        }
    }
}
