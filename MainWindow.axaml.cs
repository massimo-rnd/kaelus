using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia;
using System;

namespace kaelus
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        public void StartScan(object sender, RoutedEventArgs args)
        {
            //Check if URL is valid
            URLBox.Text = Engine.CheckUrl(URLBox.Text);
            //Url = SourceUrl.StringValue;

            //is URL valid?
            if (URLBox.Text.Equals("invalid"))
            {
                ResultBox.Text = "Invalid URL provided. Please try another URL.";
            }
            else
            {
                //Extract Emails
                //ResultBox.StringValue = Grabber.ExtractEmails(Url);
                ResultBox.Text = Engine.ExtractEmails(URLBox.Text);
            }
        }

        public void ShowHelp(object sender, RoutedEventArgs args)
        {
            var helpBox = MessageBoxManager.GetMessageBoxStandard("KAELUS Help", "Just enter the desired URL and click Start. KAELUS will do the rest", MsBox.Avalonia.Enums.ButtonEnum.Ok);
            var result = helpBox.ShowAsPopupAsync(this);
        }
    }
}