// See https://aka.ms/new-console-template for more information
using System;
using System.IO;

namespace MyApp // Note: actual namespace depends on the project name.
{
    public class FileBackup
    {
        public static string Backup(ref string logtxt, string srcfile, string extension=".bak")
        {
            if (!File.Exists(srcfile))
            {
                logtxt = $"No backup made since file {srcfile} doesn't exist";
                return "";
            }

            string newFilename = srcfile + extension;
            if (File.Exists(newFilename))
            {
                int suffix = 1;
                var tempNewFilename = $"{newFilename}_{suffix}";
                while (File.Exists(tempNewFilename))
                {
                    suffix++;
                    tempNewFilename = $"{newFilename}_{suffix}";
                }
                newFilename = tempNewFilename;
            }

            try
            {
                logtxt += $"Backup {srcfile} to {newFilename}";
                File.Copy(srcfile, newFilename);
            }
            catch (Exception e)
            {
                return e.ToString();
            }
            return "";
        }
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            string homeLocation = args[1];
            string systemdSystemLocation = args[2];
            var serviceLocation = Path.Combine(systemdSystemLocation, "vncserver@.service");
            string extralog = "";
            string error = FileBackup.Backup(ref extralog, serviceLocation, ".bak");
            if (extralog != "")
                Console.WriteLine(extralog);
            if (error != "")
            {
                Console.WriteLine(error);
            }

            string serviceTxt = @"[Unit]
Description=Start VNC server at startup
After=syslog.target network.target

[Service]
Type=forking
User=@USER@
Group=@USER@
WorkingDirectory=@HOME@

PIDFile=@HOME@/.vnc/%H:%i.pid
ExecStartPre=-/usr/bin/vncserver -kill :%i > /dev/null 2>&1
ExecStart=/usr/bin/vncserver -depth 24 -geometry 1024x768 :%i
ExecStop=/usr/bin/vncserver -kill :%i

[Install]
WantedBy=multi-user.target
                        ".Replace("@USER@", Path.GetFileName(homeLocation)).Replace("@HOME@", homeLocation);
            try
            {
                string systemdServiceName = Path.Combine(systemdSystemLocation, "vncserver@.service");
                Console.WriteLine($"Writing a new vnc xstartup script in {systemdServiceName}");
                File.WriteAllText(systemdServiceName, serviceTxt);
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
