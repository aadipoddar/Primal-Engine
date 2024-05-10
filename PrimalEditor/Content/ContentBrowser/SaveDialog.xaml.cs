using System.ComponentModel;
using System.IO;
using System.Windows;

namespace PrimalEditor.Content
{
	/// <summary>
	/// Interaction logic for SaveDialog.xaml
	/// </summary>
	public partial class SaveDialog : Window
	{
		public string SaveFilePath { get; private set; }

		public SaveDialog()
		{
			InitializeComponent();
			Closing += OnSaveDialogClosing;
		}

		private bool ValidateFileName(out string saveFilePath)
		{
			var contentBrowser = contentBrowserView.DataContext as ContentBrowser;
			var path = contentBrowser.SelectedFolder;
			if (!Path.EndsInDirectorySeparator(path)) path += @"\";
			var fileName = fileNameTextBox.Text.Trim();
			if (string.IsNullOrEmpty(fileName))
			{
				saveFilePath = string.Empty;
				return false;
			}

			if (!fileName.EndsWith(Asset.AssetFileExtension))
				fileName += Asset.AssetFileExtension;

			path += $@"{fileName}";
			var isValid = false;
			string errorMsg = string.Empty;

			if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
			{
				errorMsg = "Invalid Character(s) used in asset file name";
			}
			else if (File.Exists(path) &&
				MessageBox.Show("File Already exists. Overwrite?", "Overwrite File", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
			{
				// Do Nothing. Just return False
			}
			else { isValid = true; }

			if (!string.IsNullOrEmpty(errorMsg))
			{
				MessageBox.Show(errorMsg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}

			saveFilePath = path;
			return isValid;
		}

		private void OnSave_Button_Click(object sender, RoutedEventArgs e)
		{
			if (ValidateFileName(out var saveFilePath))
			{
				SaveFilePath = saveFilePath;
				DialogResult = true;
				Close();
			}
		}

		private void OnContentBrowser_Mouse_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if ((e.OriginalSource as FrameworkElement).DataContext == contentBrowserView.SelectedItem &&
				contentBrowserView.SelectedItem.FileName == fileNameTextBox.Text)
			{
				OnSave_Button_Click(sender, null);
			}
		}

		private void OnSaveDialogClosing(object sender, CancelEventArgs e)
		{
			contentBrowserView.Dispose();
		}
	}
}
