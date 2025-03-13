using AudioSplitter.BL;
using AudioSplitter.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;

namespace AudioSplitter
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IHost HostContainer { get; private set; }
        public static Mutex mutex;

        public App()
        {
            mutex = new Mutex(false, "AudioSplitter");

            if (!mutex.WaitOne())
            {
                Application.Current.Shutdown();
            }

            HostContainer = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<IAudioSplitter, SimpleAudioSplitter>();
                    services.AddSingleton<IAduioTagWriter, SimpleAduioTagWriter>();
                    services.AddSingleton<MainWindowViewModel>();
                })
                .Build();

        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.DataContext = HostContainer.Services.GetRequiredService<MainWindowViewModel>();
            mainWindow.ShowDialog();
        }
    }

}
