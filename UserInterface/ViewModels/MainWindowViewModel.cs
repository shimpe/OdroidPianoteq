using ReactiveUI;
using System.IO;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UserInterface.Services;

namespace UserInterface.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private bool somethingWentWrong = false;
        private string? homeLocation;
        private string? pianoteqLocation;
        private string startsh_path = "";
        public string PianoteqLocation 
        {
            get => pianoteqLocation ?? "";
            set => this.RaiseAndSetIfChanged(ref pianoteqLocation, value);
        }

        private string? xStartupLocation;
        public string XStartupLocation
        {
            get => xStartupLocation ?? "";
            set => this.RaiseAndSetIfChanged(ref xStartupLocation, value);
        }

        private string? systemdSystemLocation;
        public string SystemdSystemLocation
        {
            get => systemdSystemLocation ?? "";
            set => this.RaiseAndSetIfChanged(ref systemdSystemLocation, value);
        }

        private string? logInformation;
        public string LogInformation
        {
            get => logInformation ?? "";
            set => this.RaiseAndSetIfChanged(ref logInformation, value);
        }

        public Interaction<Unit, string?> ShowSelectPianoteqFileDialog { get; }
        public ReactiveCommand<Unit, Unit> BrowseForPianoteqFile { get; }

        public Interaction<Unit, string?> ShowSelectXStartupFileDialog { get; }
        public ReactiveCommand<Unit, Unit> BrowseForXStartupFolder { get; }

        public Interaction<Unit, string?> ShowSelectSystemdSystemFolderDialog { get; }
        public ReactiveCommand<Unit, Unit> BrowseForSystemdSystemFolder { get; }

        public ReactiveCommand<Unit, Unit> RunPianoteqConfiguration { get; }

        public MainWindowViewModel()
        {
            this.homeLocation = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            this.pianoteqLocation = Path.Combine(this.homeLocation, "Pianoteq/Pianoteq 7/arm-64bit/Pianoteq 7");
            this.xStartupLocation = Path.Combine(this.homeLocation, ".vnc", "xstartup");
            this.systemdSystemLocation = "/etc/systemd/system";
            this.logInformation = "";
            
            BrowseForPianoteqFile = ReactiveCommand.CreateFromTask(SelectPianoteqFileAsync);
            ShowSelectPianoteqFileDialog = new Interaction<Unit, string?>();

            BrowseForXStartupFolder = ReactiveCommand.CreateFromTask(SelectXStartupFolderAsync);
            ShowSelectXStartupFileDialog = new Interaction<Unit, string?>();

            BrowseForSystemdSystemFolder = ReactiveCommand.CreateFromTask(SelectSystemdSystemFolderAsync);
            ShowSelectSystemdSystemFolderDialog = new Interaction<Unit, string?>();

            RunPianoteqConfiguration = ReactiveCommand.CreateFromTask(runPianoteqConfigurationAsync);

            Log("System started.");
            Log("Before you continue, make sure you have installed and configured x11vnc server on the odroid with the commands");
            Log("sudo apt install x11vnc");
            Log("vncpasswd");
            Log("");
        }

        private async Task SelectPianoteqFileAsync()
        {
            var fileName = await ShowSelectPianoteqFileDialog.Handle(Unit.Default);

            if (fileName is object)
            {
                PianoteqLocation = fileName;
                Log($"Changed pianoteq executable to {fileName}");
            }
        }
        private async Task SelectXStartupFolderAsync()
        {
            var folderName = await ShowSelectXStartupFileDialog.Handle(Unit.Default);

            if (folderName is object)
            {
                XStartupLocation = folderName;
                Log($"Changed xstartup location to {folderName}");
            }
        }
        private async Task SelectSystemdSystemFolderAsync()
        {
            var folderName = await ShowSelectSystemdSystemFolderDialog.Handle(Unit.Default);

            if (folderName is object)
            {
                SystemdSystemLocation = folderName;
                Log($"Changed systemd service location to {folderName}");
            }
        }

        private string sanitizeFilename(string filename)
        {
            return filename.Replace(" ", "\\ ");
        }
        private void ClearLog()
        {
            LogInformation = "";
        }
        private void Log(string line)
        {
            var dt = DateTime.Now;
            if (!line.EndsWith("\n"))
                line += "\n";
            LogInformation += $"{dt.ToLongTimeString()} : {line}";
        }

        public string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork && ip.GetAddressBytes()[0] != 127)
                {
                    return ip.ToString();
                }
            }
            return "<Error: No network adapters with an IPv4 address in the system!>";
        }

        private async Task RunCmd(string cmd, string[] args)
        {
            try
            {
                await CommandLine.Run(cmd, args);
            }
            catch (Exception e)
            {
                Log(e.ToString());
            }
        }
        private async Task runPianoteqConfigurationAsync()
        {
            ClearLog();
            Log("Start configuration task");

            try
            {
                startsh_path = Path.Combine(homeLocation ?? "", "StartPianoteq", "start.sh");
                backupStartShScript();
                createStartShScript();
                backupXStartupScript();
                writeXStartupScript();
                backupSystemdService();
                addSystemdService();
            }
            catch (OperationCanceledException e)
            {
                Log($"Something went wrong. Setup is aborted. Not all steps were completed.\n{e.Message}");
                somethingWentWrong = true;
            }
            finally
            {
                if (somethingWentWrong)
                {
                    Log("Please read the errors and correct the problem before retrying.");
                    Log("Make sure you run this setup program with superuser privileges.");
                }
                else
                {
                    Log("Setup finished.");
                    Log("");
                    Log("");
                    Log("Making sure the xstartup script is executable with command");
                    Log($"chmod +x {xStartupLocation}");
                    await RunCmd("chmod", new string[] {"+x", $"{xStartupLocation}"});
                    
                    Log("I will now reload the systemd services, and then start the vnc server service (superuser priviliges required for this to work)");
                    Log("(sudo) systemctl daemon-reload");
                    await RunCmd("systemctl", new string[] {"daemon-reload"});

                    Log("(sudo) systemctl enable vncserver@1.service");
                    await RunCmd("systemctl", new string[] {"enable", "vncserver@1.service"});

                    Log("(sudo) systemctl restart vncserver@1");
                    await RunCmd("systemctl", new string[] {"restart", "vncserver@1"});

                    Log("");
                    Log("");
                    Log("On your tablet/phone/laptop make sure to install a vnc viewer (e.g. tightvnc or tigervnc).");
                    Log("If all went well, you can then connect to the odroid from the tablet using the following parameters:");
                    Log($"vncviewer {GetLocalIPAddress()} :1");
                    Log("The system will ask you to provide the vnc password you've set up on odroid when you installed x11vnc.");
                    Log("");
                    Log("Thank you for flying with Avalonia and have a nice day!");
                }
            }
        }

        private void backupStartShScript()
        {
            Log("Create backup of vnc xstartup script");
            string extralog = "";
            string error = FileBackup.Backup(ref extralog, startsh_path, ".bak");
            if (extralog != "")
                Log(extralog);
            if (error != "")
            {
                throw new OperationCanceledException(error);
            }
        }
        private void createStartShScript()
        {
            string directory = Path.GetDirectoryName(startsh_path) ?? "";
            Log($"Adding a start.sh script in the {directory} folder");
            string pianoteqLocationEscaped = sanitizeFilename(pianoteqLocation);
            string startsh_contents = $"taskset -c 2,3,4,5 {pianoteqLocationEscaped} --fullscreen\n";
            try
            {
                if (directory != "")
                    Directory.CreateDirectory(directory);
                File.WriteAllText(startsh_path, startsh_contents);
            }
            catch (Exception e)
            {
                throw new OperationCanceledException(e.ToString());
            }
        }

        private void backupXStartupScript() 
        {
            Log("Create backup of vnc xstartup script");
            string extralog = "";
            string error = FileBackup.Backup(ref extralog, xStartupLocation, ".bak");
            if (extralog != "")
                Log(extralog);
            if (error != "")
            {
                throw new OperationCanceledException(error);
            }
        }

        private void writeXStartupScript()
        {
            string xstartup = @"#!/bin/sh
xrdb $HOME/.Xresources
xsetroot -solid grey
export XKL_XMODMAP_DISABLE=1
export DBUS_SESSION_BUS_ADDRESS=`cat /proc/$(pidof -s mate-session)/environ | tr '\0' '\n' | grep DBUS_SESSION_BUS_ADDRESS | cut -d '=' -f2-`
mate-session &
export DISPLAY=:1
systemctl --user stop pulseaudio.socket
systemctl --user stop pulseaudio.service
@STARTSH@ &
            ".Replace("@STARTSH@", Path.Combine(startsh_path, "start.sh"));
            try 
            {
                Log($"Writing a new vnc xstartup script in {xStartupLocation}");
                File.WriteAllText(xStartupLocation, xstartup);
            }
            catch (Exception e)
            {
                throw new OperationCanceledException(e.ToString());
            }
        }

        private void backupSystemdService()
        {
            var serviceLocation = Path.Combine(systemdSystemLocation, "vncserver@.service");
            Log($"Create backup of systemd service script {serviceLocation}");
            string extralog = "";
            string error = FileBackup.Backup(ref extralog, serviceLocation, ".bak");
            if (extralog != "")
                Log(extralog);
            if (error != "")
            {
                throw new OperationCanceledException(error);
            }
        }

        private void addSystemdService()
        {
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
                Log($"Writing a new vnc xstartup script in {systemdServiceName}");
                File.WriteAllText(systemdServiceName, serviceTxt);
            }
            catch (System.Exception e)
            {
                throw new OperationCanceledException(e.ToString());
            }
        }
    }
}
