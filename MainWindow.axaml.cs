using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace kaelus
{
    public partial class MainWindow : Window
    {

        public string resultBoxText;

        public MainWindow()
        {
            InitializeComponent();
        }
        public async void StartScan(object sender, RoutedEventArgs args)
        {
            string url = URLBox.Text;

            // Create a CancellationTokenSource to manage the cancellation
            var cancellationTokenSource = new CancellationTokenSource();

            // Run both tasks concurrently
            var progressTask = makeProgress(cancellationTokenSource.Token);
            var resultTask = RunKaelusScanAsync(url);

            // When the resultTask completes, cancel the progressTask
            await resultTask;
            cancellationTokenSource.Cancel();

            // Await progressTask to ensure proper cleanup
            try
            {
                await progressTask;
            }
            catch (OperationCanceledException)
            {
                ScanProgress.IsVisible = false;
            }
        }

        public void ShowHelp(object sender, RoutedEventArgs args)
        {
            var helpBox = MessageBoxManager.GetMessageBoxStandard("KAELUS Help", "Just enter the desired URL and click Start. KAELUS will do the rest", MsBox.Avalonia.Enums.ButtonEnum.Ok);
            var result = helpBox.ShowAsPopupAsync(this);
        }

        public async Task makeProgress(CancellationToken cancellationToken)
        {
            // Fake Progress bar (to indicate work)
            ScanProgress.Value = 0;
            ScanProgress.IsVisible = true;
            ResultBox.Text = "Scanning for email addresses...";

            while (ScanProgress.Value < ScanProgress.Maximum)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                await Task.Delay(2000, cancellationToken); 
                ScanProgress.Value = Math.Min(ScanProgress.Value + 1, ScanProgress.Maximum);
            }

        }

        private async Task RunKaelusScanAsync(string url)
        {
            string result = await Task.Run(() => Engine.kaelusScan(url));

            ResultBox.Text = result;
        }
    }
}