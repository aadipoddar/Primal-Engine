﻿using System.IO;
using System.Windows;

namespace PrimalEditor
{
    /// <summary>
    /// Interaction logic for EnginePathDialog.xaml
    /// </summary>
    public partial class EnginePathDialog : Window
    {
        public string PrimalPath { get; private set; }

        public EnginePathDialog()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
        }

        private void OnOk_ButtonClick(object sender, RoutedEventArgs e)
        {
            var path = pathTextBox.Text.Trim();
            messageTextBlock.Text = string.Empty;

            if (string.IsNullOrEmpty(path))
            {
                messageTextBlock.Text = "Please enter a valid path";
            }
            else if (path.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            {
                messageTextBlock.Text = "Invalid character(s) used in path";
            }
            else if (!Directory.Exists(Path.Combine(path, @"Engine\EngineAPI")))
            {
                messageTextBlock.Text = "Unable to find Engine in the specified location";
            }
            if (string.IsNullOrEmpty(messageTextBlock.Text))
            {
                if (!Path.EndsInDirectorySeparator(path)) path += @"\";
                PrimalPath = path;
                DialogResult = true;
                Close();
            }
        }
    }
}
