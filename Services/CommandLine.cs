using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OdroidPianoteq.Services
{
    public class CommandLine
    {
        public async static Task Run(string Cmd, string[] Args)
        {
            ProcessRunner pr = new ProcessRunner();
            await pr.RunAsync(Cmd, Args);
        }
    }
}
