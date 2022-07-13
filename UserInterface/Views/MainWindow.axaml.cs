using System;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using UserInterface.ViewModels;
using ReactiveUI;

namespace UserInterface.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
            // When the window is activated, registers a handler for the ShowOpenFileDialog interaction.
            this.WhenActivated(d => d(ViewModel.ShowSelectPianoteqFileDialog.RegisterHandler(BrowseForPianoteqFile)));
            this.WhenActivated(d => d(ViewModel.ShowSelectXStartupFileDialog.RegisterHandler(BrowseForXStartupFile)));
            this.WhenActivated(d => d(ViewModel.ShowSelectSystemdSystemFolderDialog.RegisterHandler(BrowseForSystemdSystemFolder)));
        }

        private async Task BrowseForPianoteqFile(InteractionContext<Unit, string?> interaction)
        {
            var dialog = new OpenFileDialog();
            dialog.Directory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            dialog.Title = "Browse for unzipped pianoteq arm-64bit executable";
            var fileName = await dialog.ShowAsync(this);
            interaction.SetOutput(fileName?.FirstOrDefault());
        }

        private async Task BrowseForXStartupFile(InteractionContext<Unit, string?> interaction)
        {
            var dialog = new OpenFileDialog();
            dialog.Directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                                            ".vnc", "xstartup");    
            dialog.Title = "Browse for ~/.vnc/xstartup script";
            var fileNames = await dialog.ShowAsync(this);
            interaction.SetOutput(fileNames?.FirstOrDefault());
        }

        private async Task BrowseForSystemdSystemFolder(InteractionContext<Unit, string?> interaction)
        {
            var dialog = new OpenFolderDialog();
            dialog.Directory = "/etc/systemd/system";
            dialog.Title = "Browse for systemd service folder";
            var folderName = await dialog.ShowAsync(this);
            interaction.SetOutput(folderName);
        }

    }
}