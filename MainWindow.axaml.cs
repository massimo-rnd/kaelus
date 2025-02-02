using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Input;

namespace kaelus
{
    public partial class MainWindow : Window
    {

        public string resultBoxText;

        public MainWindow()
        {
            InitializeComponent();
            
            // Get the TextBox by Name
            var textBox = this.FindControl<TextBox>("InputBox");

            // Subscribe to the KeyDown event
            URLBox.KeyDown += (sender, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    StartScan(null, null);
                }
            };
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

        public async void MultiProcess(object sender, RoutedEventArgs args)
        {
            var openFileDialog = new OpenFileDialog
            {
                AllowMultiple = false,
                Title = "Select URL List File"
            };

            string[]? result = await openFileDialog.ShowAsync(this);
            if (result == null || result.Length == 0) return;

            string filePath = result[0];
            if (!File.Exists(filePath)) return;

            List<string> urls = File.ReadAllLines(filePath)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => line.StartsWith("http://") || line.StartsWith("https://") ? line : "https://" + line)
                .ToList();

            if (urls.Count == 0) return;

            ResultBox.Text = "Starting multi-processing of URLs...\n";

            var emailResults = new Dictionary<string, HashSet<string>>();
            var tasks = new List<Task>();

            foreach (var url in urls)
            {
                tasks.Add(Task.Run(async () =>
                {
                    string result = await Task.Run(() => Engine.kaelusScan(url));
                    lock (emailResults)
                    {
                        if (!emailResults.ContainsKey(url))
                        {
                            emailResults[url] = new HashSet<string>();
                        }
                        foreach (var email in result.Split('\n').Where(e => !string.IsNullOrWhiteSpace(e)))
                        {
                            emailResults[url].Add(email);
                        }
                    }
                }));
            }

            await Task.WhenAll(tasks);

            // Display results grouped by domain
            ResultBox.Text = "";
            foreach (var entry in emailResults)
            {
                ResultBox.Text += $"--- {entry.Key} ---\n";
                foreach (var email in entry.Value)
                {
                    ResultBox.Text += email + "\n";
                }
                ResultBox.Text += "\n";
            }
        }

        public void ShowHelp(object sender, RoutedEventArgs args)
        {
            var helpBox = MessageBoxManager.GetMessageBoxStandard("KAELUS Help", "To process a single Website, just enter the URL and click start. KAELUS will do the rest.\nIf you want to Process Multiple URLs at once, create a txt file with one URL per Line, click the Multi-Scan Button and select it.", MsBox.Avalonia.Enums.ButtonEnum.Ok);
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
            string result = "";
            result = await Task.Run(() => Engine.kaelusScan(url));

            ResultBox.Text = result;
        }
    }
}