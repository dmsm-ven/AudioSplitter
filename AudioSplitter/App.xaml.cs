using AudioSplitter.BL;
using AudioSplitter.Interfaces;
using AudioSplitter.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
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
                    services.AddSingleton<IAudioSplitter, FfmpegAudioSplitter>();
                    services.AddSingleton<IAduioTagWriter, TagLibAduioTagWriter>();
                    services.AddSingleton<IAudioFileAnalyser, FFProbeAudioFileAnalyser>();
                    services.AddSingleton<AudioSplitterManager>();
                    services.AddSingleton<MainWindowViewModel>();
                })
                .Build();

        }

        protected override void OnExit(ExitEventArgs e)
        {
            mutex.ReleaseMutex();
            base.OnExit(e);
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            var requiredFiles = new[] { "ffmpeg.exe", "ffprobe.exe" };
            var vm = HostContainer.Services.GetRequiredService<MainWindowViewModel>();

            foreach (var file in requiredFiles)
            {
                if (!File.Exists(file))
                {
                    string error = $"{file} должен находится в корне программы\r\nСкачать можно по ссылке: https://github.com/BtbN/FFmpeg-Builds/releases";
                    vm.Title = error;
                    vm.IsEnabled = false;
                    MessageBox.Show(error, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                }
            }

            var mainWindow = new MainWindow();
            mainWindow.DataContext = vm;
            mainWindow.ShowDialog();
        }
    }

}
