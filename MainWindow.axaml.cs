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
            ResultBox.Text = "This works";
        }

        public void ShowHelp(object sender, RoutedEventArgs args)
        {
            var helpBox = MessageBoxManager.GetMessageBoxStandard("KAELUS Help", "Just enter the desired URL and click Start. KAELUS will do the rest", MsBox.Avalonia.Enums.ButtonEnum.Ok);
            var result = helpBox.ShowAsPopupAsync(this);
        }
    }
}